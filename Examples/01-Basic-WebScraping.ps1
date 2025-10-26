# Basic Web Scraping Example
# This example demonstrates basic web scraping using PowerBrowser's object-oriented pipeline

# Import the PowerBrowser module
Import-Module "../PowerBrowser.psd1" -Force

Write-Host "🌐 Basic Web Scraping Example" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

try {
    # Start browser and navigate to example site
    Write-Host "📱 Starting browser and navigating to example.com..." -ForegroundColor Yellow
    $browser = Start-Browser -Name Chrome
    $page = $browser | New-BrowserPage -Url "https://example.com" -Name "ExamplePage"
    
    Write-Host "✅ Page loaded: $($page.Title)" -ForegroundColor Green
    
    # Extract the main heading
    Write-Host "🔍 Finding main heading..." -ForegroundColor Yellow
    $heading = $page | Find-BrowserElement -Selector "h1" -First
    $headingInfo = $heading | Get-BrowserElementAttribute
    
    Write-Host "📋 Main Heading: '$($headingInfo.InnerText)'" -ForegroundColor Cyan
    
    # Extract all paragraphs
    Write-Host "🔍 Finding all paragraphs..." -ForegroundColor Yellow
    $paragraphs = $page | Find-BrowserElement -Selector "p"
    
    Write-Host "📄 Found $($paragraphs.Count) paragraphs:" -ForegroundColor Cyan
    foreach ($i in 0..($paragraphs.Count - 1)) {
        $paragraphInfo = $paragraphs[$i] | Get-BrowserElementAttribute
        Write-Host "  $($i + 1). $($paragraphInfo.InnerText)" -ForegroundColor White
    }
    
    # Extract links
    Write-Host "🔍 Finding all links..." -ForegroundColor Yellow
    $links = $page | Find-BrowserElement -Selector "a"
    
    Write-Host "🔗 Found $($links.Count) links:" -ForegroundColor Cyan
    foreach ($link in $links) {
        $linkInfo = $link | Get-BrowserElementAttribute -AttributeName "href"
        $linkText = $link | Get-BrowserElementAttribute
        Write-Host "  • '$($linkText.InnerText)' -> $($linkInfo.AttributeValue)" -ForegroundColor White
    }
    
    # Get page metadata
    Write-Host "📊 Page Information:" -ForegroundColor Cyan
    Write-Host "  URL: $($page.Url)" -ForegroundColor White
    Write-Host "  Title: $($page.Title)" -ForegroundColor White
    Write-Host "  Viewport: $($page.ViewportSize)" -ForegroundColor White
    Write-Host "  Created: $($page.CreatedTime)" -ForegroundColor White
    
} catch {
    Write-Error "❌ Error occurred: $($_.Exception.Message)"
} finally {
    # Cleanup
    Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
    if ($browser) {
        $browser | Stop-Browser -Force
        Write-Host "✅ Browser closed successfully" -ForegroundColor Green
    }
}

Write-Host "🎉 Web scraping example completed!" -ForegroundColor Green