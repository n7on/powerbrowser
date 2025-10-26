# PowerBrowser - Object-Oriented Pipeline Examples

This document demonstrates the comprehensive object-oriented pipeline functionality in PowerBrowser, enabling seamless command chaining for browser automation.

## 🚀 Quick Start Examples

### Basic Browser and Page Management
```powershell
# Start a browser and create a page in one pipeline
$page = Start-Browser -Name Chrome | New-BrowserPage -Url "https://example.com"

# Get all pages for a browser
$browser = Start-Browser -Name Chrome
$pages = $browser | Get-BrowserPage

# Close pages using pipeline
$browser | Get-BrowserPage | Remove-BrowserPage -Force
```

### Element Finding and Interaction
```powershell
# Find and click elements in a complete pipeline
Start-Browser -Name Chrome | 
    New-BrowserPage -Url "https://example.com" | 
    Find-BrowserElement -Selector "a" -First | 
    Invoke-BrowserElementClick -WaitForNavigation

# Find multiple elements and get their attributes
$elements = Start-Browser -Name Chrome | 
    New-BrowserPage -Url "https://example.com" | 
    Find-BrowserElement -Selector "p"

$elements | Get-BrowserElementAttribute | Select-Object TagName, InnerText
```

### Form Interaction Pipeline
```powershell
# Complete form filling workflow
$browser = Start-Browser -Name Chrome
$page = $browser | New-BrowserPage -Url "https://httpbin.org/forms/post"

# Find and fill form fields
$page | Find-BrowserElement -Selector "input[name='custname']" | Set-BrowserElementText -Text "John Doe"
$page | Find-BrowserElement -Selector "input[name='custtel']" | Set-BrowserElementText -Text "555-1234"
$page | Find-BrowserElement -Selector "textarea[name='comments']" | Set-BrowserElementText -Text "Test comment"

# Submit the form
$page | Find-BrowserElement -Selector "input[type='submit']" | Invoke-BrowserElementClick -WaitForNavigation
```

## 📋 Object Types and Properties

### PowerBrowserInstance
The main browser object returned by `Start-Browser`:

```powershell
Name              : Chrome
Browser           : PuppeteerSharp.Cdp.CdpBrowser
StartTime         : 2025-10-26 19:24:08
Headless          : False
WindowSize        : 1280x720
ProcessId         : 12345
WebSocketEndpoint : ws://127.0.0.1:9222/devtools/browser/...
IsConnected       : True
PageCount         : 2
```

### PowerBrowserPage
Page objects returned by `New-BrowserPage` and `Get-BrowserPage`:

```powershell
PageId         : Chrome_Page1
PageName       : Page1
Browser        : Chrome (PID: 12345, Pages: 2, Connected: True)
Page           : PuppeteerSharp.Cdp.CdpPage
CreatedTime    : 2025-10-26 19:24:30
ViewportWidth  : 1280
ViewportHeight : 720
BrowserName    : Chrome
Url            : https://example.com
IsClosed       : False
ViewportSize   : 1280x720
Title          : Example Domain
```

### PowerBrowserElement
Element objects returned by `Find-BrowserElement`:

```powershell
ElementId : Page1_Element_638971037582242670_0
PageName  : Page1
Element   : JSHandle@node
Page      : PuppeteerSharp.Cdp.CdpPage
Selector  : h1
Index     : 0
FoundTime : 2025-10-26 19:29:18
TagName   : h1
InnerText : Example Domain
InnerHTML : Example Domain
ClassName : 
Id        : 
IsVisible : True
```

## 🔗 Complete Pipeline Commands

### Browser Management
- `Start-Browser` → Returns `PowerBrowserInstance`
- `Stop-Browser` → Accepts `PowerBrowserInstance` or browser name
- `Get-Browser` → Lists installed browsers

### Page Management  
- `New-BrowserPage` → Accepts `PowerBrowserInstance`, returns `PowerBrowserPage`
- `Get-BrowserPage` → Accepts `PowerBrowserInstance`, returns `PowerBrowserPage[]`
- `Remove-BrowserPage` → Accepts `PowerBrowserPage` or page ID/name

### Element Operations
- `Find-BrowserElement` → Accepts `PowerBrowserPage`, returns `PowerBrowserElement[]`
- `Invoke-BrowserElementClick` → Accepts `PowerBrowserElement`, returns same element
- `Set-BrowserElementText` → Accepts `PowerBrowserElement`, returns same element  
- `Get-BrowserElementAttribute` → Accepts `PowerBrowserElement`, returns attribute info

## 🎯 Advanced Pipeline Scenarios

### Multi-Step Automation Workflow
```powershell
# Complete e-commerce workflow example
$browser = Start-Browser -Name Chrome
$page = $browser | New-BrowserPage -Url "https://example-shop.com"

# Search for a product
$searchBox = $page | Find-BrowserElement -Selector "input[name='search']"
$searchBox | Set-BrowserElementText -Text "laptop" -PressEnter

# Wait for results and click first product
Start-Sleep 2
$firstProduct = $page | Find-BrowserElement -Selector ".product-item" -First
$productInfo = $firstProduct | Get-BrowserElementAttribute
Write-Host "Found product: $($productInfo.InnerText)"

# Click to view details
$firstProduct | Invoke-BrowserElementClick -WaitForNavigation
```

### Error Handling in Pipelines
```powershell
try {
    $result = Start-Browser -Name Chrome |
        New-BrowserPage -Url "https://example.com" |
        Find-BrowserElement -Selector ".non-existent" -Timeout 2000
}
catch {
    Write-Warning "Element not found, continuing with backup strategy..."
    # Alternative approach
}
```

### Parallel Page Processing
```powershell
$browser = Start-Browser -Name Chrome

# Create multiple pages
$urls = @("https://example.com", "https://httpbin.org", "https://github.com")
$pages = $urls | ForEach-Object { $browser | New-BrowserPage -Url $_ }

# Process each page
$results = $pages | ForEach-Object {
    $_ | Find-BrowserElement -Selector "title, h1" -First | Get-BrowserElementAttribute
}

$results | Select-Object @{Name="URL"; Expression={$_.Url}}, @{Name="Title"; Expression={$_.InnerText}}
```

## 🛠️ Pipeline Best Practices

### 1. Object Chaining
Always prefer object chaining over string-based parameters:
```powershell
# ✅ Good - Object chaining
Start-Browser -Name Chrome | New-BrowserPage | Find-BrowserElement -Selector "h1"

# ❌ Avoid - String-based (still works for backward compatibility)
Start-Browser -Name Chrome
New-BrowserPage -BrowserName Chrome
Find-BrowserElement -PageName Page1 -Selector "h1"
```

### 2. Variable Storage for Reuse
Store objects when you need to reuse them:
```powershell
$browser = Start-Browser -Name Chrome
$page1 = $browser | New-BrowserPage -Url "https://site1.com"
$page2 = $browser | New-BrowserPage -Url "https://site2.com"

# Work with both pages
$page1 | Find-BrowserElement -Selector "h1"
$page2 | Find-BrowserElement -Selector "h1"
```

### 3. Cleanup
Always clean up resources:
```powershell
# Clean shutdown
$browser | Get-BrowserPage | Remove-BrowserPage -Force
$browser | Stop-Browser -Force
```

## 🔍 Troubleshooting Pipeline Issues

### Common Issues and Solutions

1. **"No browsers are running"**
   - Ensure `Start-Browser` completed successfully
   - Check browser installation with `Get-Browser`

2. **"Page not found"**
   - Verify page was created successfully
   - Use `Get-BrowserPage` to list available pages

3. **"Element not found"**
   - Increase timeout: `Find-BrowserElement -Selector "h1" -Timeout 10000`
   - Wait for page load: `New-BrowserPage -Url "..." -WaitForLoad`
   - Use `Find-BrowserElement -WaitForVisible` for dynamic content

4. **PSObject Wrapping Issues**
   - PowerBrowser automatically handles PowerShell's PSObject wrapping
   - Objects should flow seamlessly through pipelines

### Debug Information
Use `-Verbose` flag to see detailed pipeline operations:
```powershell
Start-Browser -Name Chrome -Verbose | New-BrowserPage -Url "https://example.com" -Verbose
```

## 📈 Performance Tips

1. **Reuse Browser Instances**: Create one browser, multiple pages
2. **Batch Operations**: Use `ForEach-Object` for multiple similar operations
3. **Selective Element Finding**: Use specific selectors to reduce search time
4. **Cleanup Unused Pages**: Remove pages when done to free memory

## 🎉 Summary

PowerBrowser's object-oriented pipeline enables:
- **Seamless Command Chaining**: Natural PowerShell pipeline flow
- **Type Safety**: Strongly-typed objects with IntelliSense support  
- **Flexibility**: Both object and string-based parameters supported
- **Extensibility**: Easy to add new cmdlets that integrate with existing pipeline
- **PowerShell Native**: Follows PowerShell conventions and best practices

The pipeline transforms browser automation from procedural steps into fluid, composable operations that feel natural in PowerShell.