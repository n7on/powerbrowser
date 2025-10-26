# Advanced Element Interaction Example
# This example demonstrates advanced element interaction techniques including dynamic waiting,
# attribute inspection, and complex user interactions

# Import the PowerBrowser module
Import-Module "../PowerBrowser.psd1" -Force

Write-Host "⚡ Advanced Element Interaction Example" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

try {
    # Start browser and navigate to a dynamic content page
    Write-Host "📱 Starting browser..." -ForegroundColor Yellow
    $browser = Start-Browser -Name Chrome
    $page = $browser | New-BrowserPage -Url "https://httpbin.org/html" -Name "TestPage" -WaitForLoad
    
    Write-Host "✅ Page loaded: $($page.Title)" -ForegroundColor Green
    
    # Demonstrate different element finding strategies
    Write-Host "🔍 Element Finding Strategies..." -ForegroundColor Yellow
    
    # Find by tag name
    Write-Host "  • Finding by tag name (h1)..." -ForegroundColor Cyan
    $headings = $page | Find-BrowserElement -Selector "h1"
    Write-Host "    Found $($headings.Count) h1 elements" -ForegroundColor White
    
    # Find by class (if any exist)
    Write-Host "  • Finding elements with classes..." -ForegroundColor Cyan
    $classElements = $page | Find-BrowserElement -Selector "[class]"
    Write-Host "    Found $($classElements.Count) elements with class attributes" -ForegroundColor White
    
    # Find by multiple selectors
    Write-Host "  • Finding multiple element types..." -ForegroundColor Cyan
    $multipleElements = $page | Find-BrowserElement -Selector "h1, h2, h3, p, a"
    Write-Host "    Found $($multipleElements.Count) heading, paragraph, and link elements" -ForegroundColor White
    
    # Detailed element analysis
    Write-Host "🔬 Detailed Element Analysis..." -ForegroundColor Yellow
    
    # Analyze the first heading
    if ($headings.Count -gt 0) {
        $firstHeading = $headings[0]
        Write-Host "  📋 Analyzing first heading element..." -ForegroundColor Cyan
        
        # Get comprehensive element information
        $elementInfo = $firstHeading | Get-BrowserElementAttribute
        $elementAttrs = $firstHeading | Get-BrowserElementAttribute -AttributeName "class"
        $elementProps = $firstHeading | Get-BrowserElementAttribute -Properties
        $elementBounds = $firstHeading | Get-BrowserElementAttribute -BoundingBox
        
        Write-Host "    🏷️  Tag: $($elementInfo.TagName)" -ForegroundColor White
        Write-Host "    📝 Text: '$($elementInfo.InnerText)'" -ForegroundColor White
        Write-Host "    🔧 HTML: $($elementInfo.InnerHTML)" -ForegroundColor White
        Write-Host "    👁️  Visible: $($elementInfo.IsVisible)" -ForegroundColor White
        Write-Host "    📊 Selector: $($firstHeading.Selector)" -ForegroundColor White
        Write-Host "    🕐 Found: $($firstHeading.FoundTime)" -ForegroundColor White
        
        if ($elementBounds.BoundingBox) {
            $bounds = $elementBounds.BoundingBox
            Write-Host "    📐 Position: ($($bounds.X), $($bounds.Y))" -ForegroundColor White
            Write-Host "    📏 Size: $($bounds.Width) x $($bounds.Height)" -ForegroundColor White
        }
    }
    
    # Find and analyze links
    Write-Host "🔗 Link Analysis..." -ForegroundColor Yellow
    $links = $page | Find-BrowserElement -Selector "a[href]"
    
    if ($links.Count -gt 0) {
        Write-Host "  📊 Found $($links.Count) links with href attributes" -ForegroundColor Cyan
        
        foreach ($i in 0..([Math]::Min($links.Count - 1, 2))) {  # Analyze first 3 links
            $link = $links[$i]
            $linkInfo = $link | Get-BrowserElementAttribute
            $hrefInfo = $link | Get-BrowserElementAttribute -AttributeName "href"
            $titleInfo = $link | Get-BrowserElementAttribute -AttributeName "title"
            
            Write-Host "    🔗 Link $($i + 1):" -ForegroundColor White
            Write-Host "      Text: '$($linkInfo.InnerText.Trim())'" -ForegroundColor Gray
            Write-Host "      URL: $($hrefInfo.AttributeValue)" -ForegroundColor Gray
            if ($titleInfo.AttributeValue) {
                Write-Host "      Title: $($titleInfo.AttributeValue)" -ForegroundColor Gray
            }
            Write-Host "      Visible: $($linkInfo.IsVisible)" -ForegroundColor Gray
        }
    }
    
    # Demonstrate element interaction patterns
    Write-Host "🖱️ Element Interaction Patterns..." -ForegroundColor Yellow
    
    # Find all clickable elements
    $clickableElements = $page | Find-BrowserElement -Selector "a, button, input[type='button'], input[type='submit']"
    Write-Host "  🎯 Found $($clickableElements.Count) potentially clickable elements" -ForegroundColor Cyan
    
    # If we have clickable elements, demonstrate clicking
    if ($clickableElements.Count -gt 0) {
        $firstClickable = $clickableElements[0]
        $clickableInfo = $firstClickable | Get-BrowserElementAttribute
        
        Write-Host "  🖱️ Demonstrating click on: '$($clickableInfo.InnerText.Trim())'" -ForegroundColor Cyan
        
        # Check if it's a link that would navigate away
        if ($firstClickable.TagName -eq "a") {
            $hrefInfo = $firstClickable | Get-BrowserElementAttribute -AttributeName "href"
            if ($hrefInfo.AttributeValue -and $hrefInfo.AttributeValue -ne "#") {
                Write-Host "    ℹ️ This is a navigation link, demonstrating click without navigation..." -ForegroundColor Yellow
                # Just click without waiting for navigation for demo purposes
                $firstClickable | Invoke-BrowserElementClick
                Write-Host "    ✅ Click demonstrated successfully" -ForegroundColor Green
            } else {
                Write-Host "    ℹ️ This is a non-navigation link, safe to click..." -ForegroundColor Yellow
                $firstClickable | Invoke-BrowserElementClick
                Write-Host "    ✅ Click completed successfully" -ForegroundColor Green
            }
        } else {
            # For non-link elements, demonstrate regular click
            $firstClickable | Invoke-BrowserElementClick
            Write-Host "    ✅ Click completed successfully" -ForegroundColor Green
        }
    }
    
    # Demonstrate element visibility and positioning
    Write-Host "👁️ Element Visibility Analysis..." -ForegroundColor Yellow
    
    $allElements = $page | Find-BrowserElement -Selector "*" | Select-Object -First 10  # First 10 elements
    $visibleCount = 0
    $hiddenCount = 0
    
    foreach ($element in $allElements) {
        $elementInfo = $element | Get-BrowserElementAttribute
        if ($elementInfo.IsVisible) {
            $visibleCount++
        } else {
            $hiddenCount++
        }
    }
    
    Write-Host "  📊 Visibility Summary (first 10 elements):" -ForegroundColor Cyan
    Write-Host "    👁️ Visible: $visibleCount" -ForegroundColor Green
    Write-Host "    🙈 Hidden: $hiddenCount" -ForegroundColor Red
    
    # Page statistics
    Write-Host "📈 Page Statistics..." -ForegroundColor Yellow
    
    $allPageElements = $page | Find-BrowserElement -Selector "*"
    $headingCount = ($page | Find-BrowserElement -Selector "h1, h2, h3, h4, h5, h6").Count
    $linkCount = ($page | Find-BrowserElement -Selector "a[href]").Count
    $imageCount = ($page | Find-BrowserElement -Selector "img").Count
    $formCount = ($page | Find-BrowserElement -Selector "form").Count
    $inputCount = ($page | Find-BrowserElement -Selector "input, textarea, select").Count
    
    Write-Host "  📊 Element Count Summary:" -ForegroundColor Cyan
    Write-Host "    🏷️  Total Elements: $($allPageElements.Count)" -ForegroundColor White
    Write-Host "    📋 Headings: $headingCount" -ForegroundColor White
    Write-Host "    🔗 Links: $linkCount" -ForegroundColor White
    Write-Host "    🖼️  Images: $imageCount" -ForegroundColor White
    Write-Host "    📝 Forms: $formCount" -ForegroundColor White
    Write-Host "    ⌨️  Input Fields: $inputCount" -ForegroundColor White
    
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

Write-Host "🎉 Advanced element interaction example completed!" -ForegroundColor Green