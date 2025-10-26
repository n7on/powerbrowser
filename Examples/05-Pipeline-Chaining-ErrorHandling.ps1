# Pipeline Chaining and Error Handling Example
# This example demonstrates advanced pipeline chaining techniques and robust error handling

# Import the PowerBrowser module
Import-Module "../PowerBrowser.psd1" -Force

Write-Host "🔄 Pipeline Chaining and Error Handling Example" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

# Function to safely execute pipeline operations with error handling
function Invoke-SafePipeline {
    param(
        [string]$OperationName,
        [scriptblock]$Pipeline,
        [switch]$ContinueOnError
    )
    
    Write-Host "  🔄 $OperationName..." -ForegroundColor Cyan
    try {
        $result = & $Pipeline
        Write-Host "    ✅ Success: $OperationName completed" -ForegroundColor Green
        return $result
    }
    catch {
        Write-Host "    ❌ Error in $OperationName : $($_.Exception.Message)" -ForegroundColor Red
        if (!$ContinueOnError) {
            throw
        }
        return $null
    }
}

try {
    # Demonstrate successful pipeline chaining
    Write-Host "🚀 Successful Pipeline Chaining..." -ForegroundColor Yellow
    
    # Chain 1: Browser -> Page -> Elements -> Attributes
    $result1 = Invoke-SafePipeline "Complete Web Scraping Chain" {
        Start-Browser -Name Chrome |
            New-BrowserPage -Url "https://example.com" -Name "ChainTest" |
            Find-BrowserElement -Selector "h1, p, a" |
            ForEach-Object { $_ | Get-BrowserElementAttribute } |
            Select-Object TagName, InnerText, @{Name="Length"; Expression={$_.InnerText.Length}}
    }
    
    Write-Host "📊 Chain 1 Results:" -ForegroundColor Cyan
    $result1 | Format-Table -AutoSize | Out-String | Write-Host -ForegroundColor White
    
    # Get the browser reference for further operations
    $browser = Get-Variable -Name "*" -ValueOnly | Where-Object { $_ -is [PowerBrowser.Cmdlets.PowerBrowserInstance] } | Select-Object -First 1
    
    if ($browser) {
        # Chain 2: Multi-page creation and processing
        Write-Host "🌐 Multi-Page Processing Chain..." -ForegroundColor Yellow
        
        $urls = @("https://httpbin.org/html", "https://httpbin.org/json")
        $multiPageResults = @()
        
        foreach ($url in $urls) {
            $pageResult = Invoke-SafePipeline "Processing $url" {
                $browser |
                    New-BrowserPage -Url $url -WaitForLoad |
                    Find-BrowserElement -Selector "title, h1, h2, h3" -First |
                    Get-BrowserElementAttribute |
                    Select-Object @{Name="URL"; Expression={$url}}, TagName, InnerText
            } -ContinueOnError
            
            if ($pageResult) {
                $multiPageResults += $pageResult
            }
        }
        
        Write-Host "📊 Multi-Page Results:" -ForegroundColor Cyan
        $multiPageResults | Format-Table -AutoSize | Out-String | Write-Host -ForegroundColor White
        
        # Chain 3: Complex element interaction chain
        Write-Host "🎯 Complex Interaction Chain..." -ForegroundColor Yellow
        
        $interactionResult = Invoke-SafePipeline "Complex Element Interaction" {
            # Create a new page for interaction testing
            $testPage = $browser | New-BrowserPage -Url "https://httpbin.org/forms/post" -Name "InteractionTest" -WaitForLoad
            
            # Chain multiple operations together
            $formElements = $testPage |
                Find-BrowserElement -Selector "input, textarea, select" |
                ForEach-Object {
                    $elementInfo = $_ | Get-BrowserElementAttribute -Properties -AttributeName "name"
                    [PSCustomObject]@{
                        ElementId = $_.ElementId
                        TagName = $_.TagName
                        Name = $elementInfo.AttributeValue
                        Type = ($_ | Get-BrowserElementAttribute -AttributeName "type").AttributeValue
                        Value = $elementInfo.Properties.value
                        Required = ($_ | Get-BrowserElementAttribute -AttributeName "required").AttributeValue -ne $null
                    }
                }
            
            return $formElements
        } -ContinueOnError
        
        if ($interactionResult) {
            Write-Host "📊 Form Elements Analysis:" -ForegroundColor Cyan
            $interactionResult | Format-Table -AutoSize | Out-String | Write-Host -ForegroundColor White
        }
    }
    
    # Demonstrate error handling scenarios
    Write-Host "⚠️ Error Handling Scenarios..." -ForegroundColor Yellow
    
    # Scenario 1: Non-existent selector
    $errorResult1 = Invoke-SafePipeline "Finding Non-existent Elements" {
        $browser |
            Get-BrowserPage |
            Select-Object -First 1 |
            Find-BrowserElement -Selector ".this-class-does-not-exist" -Timeout 2000 |
            Get-BrowserElementAttribute
    } -ContinueOnError
    
    if ($errorResult1 -eq $null) {
        Write-Host "    ℹ️ Gracefully handled non-existent element scenario" -ForegroundColor Yellow
    }
    
    # Scenario 2: Invalid URL (would normally cause error)
    $errorResult2 = Invoke-SafePipeline "Opening Invalid URL" {
        $browser | New-BrowserPage -Url "not-a-valid-url" -Name "ErrorTest"
    } -ContinueOnError
    
    if ($errorResult2 -eq $null) {
        Write-Host "    ℹ️ Gracefully handled invalid URL scenario" -ForegroundColor Yellow
    }
    
    # Demonstrate pipeline with conditional logic
    Write-Host "🧠 Conditional Pipeline Logic..." -ForegroundColor Yellow
    
    $conditionalResult = Invoke-SafePipeline "Conditional Element Processing" {
        $browser |
            Get-BrowserPage |
            ForEach-Object {
                $page = $_
                $elements = $page | Find-BrowserElement -Selector "h1, h2, h3"
                
                if ($elements.Count -gt 0) {
                    # Process pages with headings
                    $elements |
                        Get-BrowserElementAttribute |
                        Select-Object @{Name="PageName"; Expression={$page.PageName}}, TagName, InnerText
                } else {
                    # Handle pages without headings
                    [PSCustomObject]@{
                        PageName = $page.PageName
                        TagName = "No headings"
                        InnerText = "This page has no heading elements"
                    }
                }
            }
    }
    
    Write-Host "📊 Conditional Processing Results:" -ForegroundColor Cyan
    $conditionalResult | Format-Table -AutoSize | Out-String | Write-Host -ForegroundColor White
    
    # Advanced pipeline with filtering and transformation
    Write-Host "🔄 Advanced Pipeline Transformation..." -ForegroundColor Yellow
    
    $transformationResult = Invoke-SafePipeline "Advanced Transformation Pipeline" {
        $browser |
            Get-BrowserPage |
            ForEach-Object {
                $page = $_
                $allElements = $page | Find-BrowserElement -Selector "*" | Select-Object -First 20
                
                $allElements |
                    Get-BrowserElementAttribute |
                    Where-Object { $_.InnerText.Trim().Length -gt 0 } |
                    Select-Object @{Name="Page"; Expression={$page.PageName}}, 
                                TagName, 
                                @{Name="TextLength"; Expression={$_.InnerText.Length}}, 
                                @{Name="TextPreview"; Expression={
                                    if ($_.InnerText.Length -gt 50) { 
                                        $_.InnerText.Substring(0, 50) + "..." 
                                    } else { 
                                        $_.InnerText 
                                    }
                                }} |
                    Sort-Object TextLength -Descending
            }
    }
    
    Write-Host "📊 Top Elements by Text Length:" -ForegroundColor Cyan
    $transformationResult | Select-Object -First 10 | Format-Table -AutoSize | Out-String | Write-Host -ForegroundColor White
    
} catch {
    Write-Error "❌ Critical error occurred: $($_.Exception.Message)"
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
} finally {
    # Comprehensive cleanup
    Write-Host "🧹 Comprehensive cleanup..." -ForegroundColor Yellow
    
    try {
        # Find any remaining browser instances
        $browsers = Get-Variable -Name "*" -ValueOnly | Where-Object { $_ -is [PowerBrowser.Cmdlets.PowerBrowserInstance] }
        
        foreach ($browser in $browsers) {
            Write-Host "  🔄 Cleaning up browser: $($browser.Name)" -ForegroundColor Cyan
            
            # Close all pages first
            $pages = $browser | Get-BrowserPage
            if ($pages) {
                $pages | Remove-BrowserPage -Force
                Write-Host "    📄 Closed $($pages.Count) pages" -ForegroundColor White
            }
            
            # Stop the browser
            $browser | Stop-Browser -Force
            Write-Host "    🛑 Browser stopped" -ForegroundColor White
        }
        
        Write-Host "✅ All resources cleaned up successfully" -ForegroundColor Green
    }
    catch {
        Write-Warning "⚠️ Error during cleanup: $($_.Exception.Message)"
    }
}

Write-Host "🎉 Pipeline chaining and error handling example completed!" -ForegroundColor Green