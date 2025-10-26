# Form Interaction Example
# This example demonstrates form filling and submission using PowerBrowser's pipeline

# Import the PowerBrowser module
Import-Module "../PowerBrowser.psd1" -Force

Write-Host "📝 Form Interaction Example" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green

try {
    # Start browser and navigate to a form page
    Write-Host "📱 Starting browser and navigating to httpbin.org form..." -ForegroundColor Yellow
    $browser = Start-Browser -Name Chrome
    $page = $browser | New-BrowserPage -Url "https://httpbin.org/forms/post" -Name "FormPage" -WaitForLoad
    
    Write-Host "✅ Form page loaded: $($page.Title)" -ForegroundColor Green
    
    # Find and fill the customer name field
    Write-Host "📝 Filling customer name field..." -ForegroundColor Yellow
    $nameField = $page | Find-BrowserElement -Selector "input[name='custname']" -WaitForVisible -Timeout 5000
    $nameField | Set-BrowserElementText -Text "John Doe" -Clear
    
    # Find and fill the telephone field
    Write-Host "📞 Filling telephone field..." -ForegroundColor Yellow
    $phoneField = $page | Find-BrowserElement -Selector "input[name='custtel']" -First
    $phoneField | Set-BrowserElementText -Text "555-123-4567" -Clear
    
    # Find and fill the email field
    Write-Host "📧 Filling email field..." -ForegroundColor Yellow
    $emailField = $page | Find-BrowserElement -Selector "input[name='custemail']" -First
    $emailField | Set-BrowserElementText -Text "john.doe@example.com" -Clear
    
    # Find and select a radio button (size)
    Write-Host "📊 Selecting pizza size..." -ForegroundColor Yellow
    $mediumSize = $page | Find-BrowserElement -Selector "input[value='medium']" -First
    $mediumSize | Invoke-BrowserElementClick
    
    # Find and fill the comments textarea
    Write-Host "💬 Adding comments..." -ForegroundColor Yellow
    $commentsField = $page | Find-BrowserElement -Selector "textarea[name='comments']" -First
    $commentsField | Set-BrowserElementText -Text "Please deliver to the front door. Thank you!" -Clear
    
    # Verify form fields are filled
    Write-Host "🔍 Verifying form data..." -ForegroundColor Yellow
    
    $nameValue = $nameField | Get-BrowserElementAttribute -Properties
    $phoneValue = $phoneField | Get-BrowserElementAttribute -Properties
    $emailValue = $emailField | Get-BrowserElementAttribute -Properties
    $commentsValue = $commentsField | Get-BrowserElementAttribute -Properties
    
    Write-Host "📋 Form Data Summary:" -ForegroundColor Cyan
    Write-Host "  Name: $($nameValue.Properties.value)" -ForegroundColor White
    Write-Host "  Phone: $($phoneValue.Properties.value)" -ForegroundColor White
    Write-Host "  Email: $($emailValue.Properties.value)" -ForegroundColor White
    Write-Host "  Comments: $($commentsValue.Properties.value)" -ForegroundColor White
    
    # Submit the form
    Write-Host "🚀 Submitting form..." -ForegroundColor Yellow
    $submitButton = $page | Find-BrowserElement -Selector "input[type='submit']" -First
    $submitButton | Invoke-BrowserElementClick -WaitForNavigation -NavigationTimeout 10000
    
    # Wait a moment for the page to load
    Start-Sleep -Seconds 2
    
    # Check the result page
    Write-Host "✅ Form submitted! Checking response..." -ForegroundColor Green
    $responseContent = $page | Find-BrowserElement -Selector "pre" -First
    if ($responseContent) {
        $responseInfo = $responseContent | Get-BrowserElementAttribute
        Write-Host "📄 Server Response:" -ForegroundColor Cyan
        Write-Host $responseInfo.InnerText -ForegroundColor White
    }
    
} catch {
    Write-Error "❌ Error occurred: $($_.Exception.Message)"
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
} finally {
    # Cleanup
    Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
    if ($browser) {
        $browser | Stop-Browser -Force
        Write-Host "✅ Browser closed successfully" -ForegroundColor Green
    }
}

Write-Host "🎉 Form interaction example completed!" -ForegroundColor Green