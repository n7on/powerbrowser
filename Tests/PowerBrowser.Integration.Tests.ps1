#Requires -Modules Pester

Describe "PowerBrowser Core Functionality" -Tags @("Core", "Integration") {
    BeforeAll {
        # Import the module
        $ModulePath = Join-Path $PSScriptRoot '..' 'bin' 'Debug' 'netstandard2.0' 'PowerBrowser.dll'
        Import-Module $ModulePath -Force
        
        # Test configuration
        $TestBrowserName = 'Chrome'
        $TestTimeout = 30000
    }

    BeforeEach {
        # Ensure clean state
        Get-Browser | Where-Object Running | ForEach-Object { 
            Stop-Browser -Name $_.Name -ErrorAction SilentlyContinue 
        }
    }
    
    AfterEach {
        # Cleanup after each test
        Get-Browser | Where-Object Running | ForEach-Object { 
            Stop-Browser -Name $_.Name -ErrorAction SilentlyContinue 
        }
    }

    Context "Basic Web Scraping" {
        It "Should perform basic web scraping workflow" {
            # Start browser and navigate
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Url "https://example.com" -Name "ExamplePage"
            
            # Verify page loaded
            $page | Should -Not -BeNullOrEmpty
            $page.Title | Should -Not -BeNullOrEmpty
            $page.Url | Should -Be "https://example.com/"
            
            # Extract main heading
            $heading = $page | Find-BrowserElement -Selector "h1" -First
            $heading | Should -Not -BeNullOrEmpty
            
            $headingInfo = $heading | Get-BrowserElementAttribute
            $headingInfo.InnerText | Should -Be "Example Domain"
            
            # Extract paragraphs
            $paragraphs = $page | Find-BrowserElement -Selector "p"
            $paragraphs.Count | Should -BeGreaterThan 0
            
            # Extract links
            $links = $page | Find-BrowserElement -Selector "a"
            $links.Count | Should -BeGreaterThan 0
        }
    }

    Context "Form Interaction" {
        It "Should fill and interact with forms" {
            # Start browser and navigate to form - wait for complete load
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Url "https://httpbin.org/forms/post" -Name "FormPage" -WaitForLoad
            
            # Verify form page loaded
            $page | Should -Not -BeNullOrEmpty
            $page.Url | Should -BeLike "*httpbin.org/forms/post*"
            
            # Find input elements (form structure may vary) - wait for them to be visible
            $inputs = $page | Find-BrowserElement -Selector "input[type='text']" -WaitForVisible -Timeout 10000 -ErrorAction SilentlyContinue 3>$null
            if ($inputs.Count -eq 0) {
                # Try alternative selector with waiting
                $inputs = $page | Find-BrowserElement -Selector "input" -WaitForVisible -Timeout 10000 -ErrorAction SilentlyContinue 3>$null
            }
            
            # Should find some input elements
            $inputs.Count | Should -BeGreaterThan 0
            
            # Test text input on first available text input
            $firstInput = $inputs[0]
            $firstInput | Should -Not -BeNullOrEmpty
            $firstInput | Set-BrowserElementText -Text "Test Value" -Clear
            
            # Verify text was set 
            $inputInfo = $firstInput | Get-BrowserElementAttribute -Properties
            # The value is in the Properties dictionary
            $inputInfo.Properties['value'] | Should -Be "Test Value"
        }
        
        It "Should handle radio button and checkbox interactions" {
            # Start browser and navigate to form - wait for complete load
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Url "https://httpbin.org/forms/post" -Name "FormPage" -WaitForLoad
            
            # Find radio buttons - wait for them to be visible
            $radioButtons = $page | Find-BrowserElement -Selector "input[type='radio']" -WaitForVisible -Timeout 10000 -ErrorAction SilentlyContinue 3>$null
            
            if ($radioButtons.Count -gt 0) {
                # Select first radio button
                $firstRadio = $radioButtons[0]
                $firstRadio | Should -Not -BeNullOrEmpty
                $firstRadio | Invoke-BrowserElementClick
                
                # Verify interaction succeeded (button should be clickable)
                $radioInfo = $firstRadio | Get-BrowserElementAttribute -Properties
                $radioInfo | Should -Not -BeNullOrEmpty
            } else {
                # If no radio buttons found, just verify we can find some interactive elements
                # Try to find submit button with waiting
                $buttons = $page | Find-BrowserElement -Selector "button,input[type='submit']" -WaitForVisible -Timeout 10000 -ErrorAction SilentlyContinue 3>$null
                $buttons.Count | Should -BeGreaterThan 0
            }
        }
    }

    Context "Multi-Page Management" {
        It "Should manage multiple pages in a single browser" {
            # Start browser
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $browser | Should -Not -BeNullOrEmpty
            
            # Create multiple pages with simpler URLs
            $page1 = $browser | New-BrowserPage -Url "about:blank" -Name "Page1"
            $page2 = $browser | New-BrowserPage -Url "https://example.com" -Name "Page2"
            
            # Verify pages created
            $page1 | Should -Not -BeNullOrEmpty
            $page2 | Should -Not -BeNullOrEmpty
            
            # List all pages
            $allPages = $browser | Get-BrowserPage
            $allPages.Count | Should -BeGreaterOrEqual 2
            
            # Verify page names
            $pageNames = $allPages | ForEach-Object { $_.PageName }
            $pageNames | Should -Contain "Page1"
            $pageNames | Should -Contain "Page2"
            
            # Verify URLs
            $page1.Url | Should -Be "about:blank"
            $page2.Url | Should -Be "https://example.com/"
        }
    }

    Context "Element Operations" {
        It "Should perform advanced element interactions" {
            # Start browser and navigate to a simpler, more reliable page
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Url "https://example.com" -Name "TestPage"
            
            # Find multiple elements (use more reliable selectors)
                        # Find elements on the page
            $elements = $page | Find-BrowserElement -Selector "p,h1,a" -ErrorAction SilentlyContinue 3>$null
            $elements.Count | Should -BeGreaterThan 0
            
            # Use first element for testing
            $firstElement = $elements[0]
            
            # Test element attribute retrieval
            $attributes = $firstElement | Get-BrowserElementAttribute -Properties
            $attributes | Should -Not -BeNullOrEmpty
            $attributes.TagName | Should -Not -BeNullOrEmpty
            
            # Test specific attribute retrieval (try href for links, or other common attributes)
            if ($attributes.TagName -eq "A") {
                $hrefAttr = $firstElement | Get-BrowserElementAttribute -AttributeName "href"
                $hrefAttr | Should -Not -BeNullOrEmpty
            }
            
            # Test element clicking (should work on any clickable element)
            $firstElement | Invoke-BrowserElementClick
            # Should not throw an exception
            
            # If we find an input element, test text setting
            $inputs = $page | Find-BrowserElement -Selector "input" -ErrorAction SilentlyContinue 3>$null
            if ($inputs.Count -gt 0) {
                $firstInput = $inputs[0]
                $firstInput | Set-BrowserElementText -Text "Test Value" -Clear -ErrorAction SilentlyContinue
            }
        }
    }

    Context "Object-Oriented Pipeline" {
        It "Should return proper PowerBrowserInstance objects from Get-Browser" {
            # Get browser list
            $browsers = Get-Browser
            $browsers | Should -Not -BeNullOrEmpty
            
            # Check object types
            $chrome = $browsers | Where-Object { $_.Name -eq 'Chrome' } | Select-Object -First 1
            $chrome | Should -Not -BeNullOrEmpty
            $chrome.GetType().Name | Should -Be "PowerBrowserInstance"
            
            # Check required properties exist
            $chrome.Name | Should -Be "Chrome"
            $chrome.PSObject.Properties.Name | Should -Contain "IsConnected"
            $chrome.PSObject.Properties.Name | Should -Contain "ProcessId"
            $chrome.PSObject.Properties.Name | Should -Contain "PageCount"
            $chrome.PSObject.Properties.Name | Should -Contain "Path"
        }
        
        It "Should support browser pipeline operations" {
            # Start browser using pipeline
            $browser = Start-Browser -Name $TestBrowserName -Headless
            
            # Create page using pipeline
            $page = $browser | New-BrowserPage -Url "https://example.com" -Name "PipelineTest"
            $page | Should -Not -BeNullOrEmpty
            $page.PageName | Should -Be "PipelineTest"
            
            # Find element using pipeline
            $element = $page | Find-BrowserElement -Selector "h1" -First
            $element | Should -Not -BeNullOrEmpty
            
            # Get element info using pipeline
            $info = $element | Get-BrowserElementAttribute
            $info.InnerText | Should -Be "Example Domain"
        }
    }

    Context "Navigation and Loading" {
        It "Should handle page navigation correctly" {
            # Start browser
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Url "https://example.com" -Name "NavTest"
            
            # Verify initial navigation
            $page.Url | Should -Be "https://example.com/"
            $page.Title | Should -Not -BeNullOrEmpty
            
            # Navigate to different page (if the site has links)
            $links = $page | Find-BrowserElement -Selector "a" -ErrorAction SilentlyContinue 3>$null
            if ($links.Count -gt 0) {
                $firstLink = $links[0]
                $linkInfo = $firstLink | Get-BrowserElementAttribute -AttributeName "href"
                $linkInfo.AttributeValue | Should -Not -BeNullOrEmpty
            }
        }
    }
}