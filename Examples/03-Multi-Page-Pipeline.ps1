# Multi-Page Pipeline Example
# This example demonstrates managing multiple pages in a single browser and processing them in parallel

# Import the PowerBrowser module
Import-Module "../PowerBrowser.psd1" -Force

Write-Host "🌐 Multi-Page Pipeline Example" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green

try {
    # Start browser
    Write-Host "📱 Starting browser..." -ForegroundColor Yellow
    $browser = Start-Browser -Name Chrome
    
    # Define URLs to visit
    $urls = @(
        @{ Url = "https://example.com"; Name = "ExampleSite" },
        @{ Url = "https://httpbin.org/html"; Name = "HttpBinHTML" },
        @{ Url = "https://httpbin.org/json"; Name = "HttpBinJSON" }
    )
    
    # Create multiple pages
    Write-Host "📄 Creating multiple pages..." -ForegroundColor Yellow
    $pages = @()
    foreach ($site in $urls) {
        Write-Host "  • Opening $($site.Name) at $($site.Url)" -ForegroundColor Cyan
        $page = $browser | New-BrowserPage -Url $site.Url -Name $site.Name -WaitForLoad
        $pages += $page
        Start-Sleep -Seconds 1  # Brief pause between page loads
    }
    
    Write-Host "✅ Created $($pages.Count) pages successfully" -ForegroundColor Green
    
    # List all pages using Get-BrowserPage
    Write-Host "📋 Listing all browser pages..." -ForegroundColor Yellow
    $allPages = $browser | Get-BrowserPage
    Write-Host "🔍 Found $($allPages.Count) active pages:" -ForegroundColor Cyan
    foreach ($page in $allPages) {
        Write-Host "  • $($page.PageName): $($page.Title) ($($page.Url))" -ForegroundColor White
    }
    
    # Process each page to extract information
    Write-Host "🔍 Processing each page for content..." -ForegroundColor Yellow
    $results = @()
    
    foreach ($page in $allPages) {
        Write-Host "  📄 Processing $($page.PageName)..." -ForegroundColor Cyan
        
        $pageData = @{
            PageName = $page.PageName
            Title = $page.Title
            Url = $page.Url
            Headings = @()
            Links = @()
            Paragraphs = @()
        }
        
        # Find headings (h1, h2, h3)
        $headings = $page | Find-BrowserElement -Selector "h1, h2, h3"
        foreach ($heading in $headings) {
            $headingInfo = $heading | Get-BrowserElementAttribute
            $pageData.Headings += @{
                Tag = $headingInfo.TagName
                Text = $headingInfo.InnerText
            }
        }
        
        # Find links
        $links = $page | Find-BrowserElement -Selector "a[href]"
        foreach ($link in $links) {
            $linkInfo = $link | Get-BrowserElementAttribute -AttributeName "href"
            $linkText = $link | Get-BrowserElementAttribute
            if ($linkText.InnerText.Trim() -ne "") {
                $pageData.Links += @{
                    Text = $linkText.InnerText.Trim()
                    Href = $linkInfo.AttributeValue
                }
            }
        }
        
        # Find paragraphs
        $paragraphs = $page | Find-BrowserElement -Selector "p"
        foreach ($paragraph in $paragraphs) {
            $paragraphInfo = $paragraph | Get-BrowserElementAttribute
            if ($paragraphInfo.InnerText.Trim() -ne "") {
                $pageData.Paragraphs += $paragraphInfo.InnerText.Trim()
            }
        }
        
        $results += $pageData
    }
    
    # Display results
    Write-Host "📊 Content Analysis Results:" -ForegroundColor Green
    Write-Host "=============================" -ForegroundColor Green
    
    foreach ($result in $results) {
        Write-Host "🌐 Page: $($result.PageName)" -ForegroundColor Cyan
        Write-Host "  📍 URL: $($result.Url)" -ForegroundColor White
        Write-Host "  📝 Title: $($result.Title)" -ForegroundColor White
        Write-Host "  📋 Headings: $($result.Headings.Count)" -ForegroundColor White
        foreach ($heading in $result.Headings) {
            Write-Host "    • $($heading.Tag.ToUpper()): $($heading.Text)" -ForegroundColor Gray
        }
        Write-Host "  🔗 Links: $($result.Links.Count)" -ForegroundColor White
        foreach ($link in $result.Links) {
            Write-Host "    • '$($link.Text)' -> $($link.Href)" -ForegroundColor Gray
        }
        Write-Host "  📄 Paragraphs: $($result.Paragraphs.Count)" -ForegroundColor White
        foreach ($paragraph in $result.Paragraphs) {
            $preview = if ($paragraph.Length -gt 80) { $paragraph.Substring(0, 80) + "..." } else { $paragraph }
            Write-Host "    • $preview" -ForegroundColor Gray
        }
        Write-Host ""
    }
    
    # Demonstrate selective page operations
    Write-Host "🎯 Selective Page Operations..." -ForegroundColor Yellow
    
    # Find a specific page and interact with it
    $examplePage = $allPages | Where-Object { $_.PageName -eq "ExampleSite" }
    if ($examplePage) {
        Write-Host "  🔍 Working with Example.com page..." -ForegroundColor Cyan
        $exampleHeading = $examplePage | Find-BrowserElement -Selector "h1" -First
        $headingInfo = $exampleHeading | Get-BrowserElementAttribute
        Write-Host "  📋 Main heading: '$($headingInfo.InnerText)'" -ForegroundColor White
    }
    
} catch {
    Write-Error "❌ Error occurred: $($_.Exception.Message)"
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
} finally {
    # Cleanup all pages
    Write-Host "🧹 Cleaning up all pages..." -ForegroundColor Yellow
    if ($browser) {
        $browser | Get-BrowserPage | Remove-BrowserPage -Force
        $browser | Stop-Browser -Force
        Write-Host "✅ All pages closed and browser stopped" -ForegroundColor Green
    }
}

Write-Host "🎉 Multi-page pipeline example completed!" -ForegroundColor Green