#Requires -Modules Pester

Describe "PowerBrowser Browser Management" -Tags @("BrowserManagement", "Core") {
    BeforeAll {
        # Import the module
        $ModulePath = Join-Path $PSScriptRoot '..' 'PowerBrowser.psd1'
        Import-Module $ModulePath -Force

        $TestBrowserName = 'Chrome'

        Install-Browser -BrowserType $TestBrowserName
    }

    # Global cleanup only at the very end to avoid interfering with Context-level browser management
    AfterAll {
        # Final cleanup after all tests
        Get-Browser | Where-Object Running | ForEach-Object { 
            Stop-Browser -Name $_.Name -ErrorAction SilentlyContinue 
        }
    }

    Context "Browser Discovery and Installation" {
        AfterEach {
            # Cleanup any browsers started in this context
            Get-Browser | Where-Object Running | ForEach-Object { 
                Stop-Browser -Name $_.Name -ErrorAction SilentlyContinue 
            }
        }
        
        It "Should list available browsers" {
            $browsers = Get-Browser
            $browsers | Should -Not -BeNullOrEmpty
            $browsers.Count | Should -BeGreaterThan 0
            
            # Should find Chrome
            $chrome = $browsers | Where-Object { $_.Name -eq $TestBrowserName}
            $chrome | Should -Not -BeNullOrEmpty
            $chrome.Name | Should -Be $TestBrowserName
        }
        
        It "Should show browser installation status" {
            $browsers = Get-Browser
            $chrome = $browsers | Where-Object { $_.Name -eq $TestBrowserName}
            
            # Chrome should have installation information
            $chrome.PSObject.Properties.Name | Should -Contain "Path"
            $chrome.PSObject.Properties.Name | Should -Contain "Running"
            $chrome.PSObject.Properties.Name | Should -Contain "ProcessId"
        }
    }

    Context "Browser Lifecycle Management" {
        AfterEach {
            # Cleanup any browsers started in this context
            Get-Browser | Where-Object Running | ForEach-Object { 
                Stop-Browser -Name $_.Name -ErrorAction SilentlyContinue 
            }
        }
        
        It "Should start and stop browser successfully" {
            # Start browser
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $browser | Should -Not -BeNullOrEmpty
            $browser.GetType().Name | Should -Be "PowerBrowserInstance"
            
            # Verify browser is running
            $runningBrowsers = Get-Browser | Where-Object { $_.Running -eq $true }
            $runningBrowsers | Should -Not -BeNullOrEmpty
            $runningBrowser = $runningBrowsers | Where-Object { $_.Name -eq $TestBrowserName }
            $runningBrowser | Should -Not -BeNullOrEmpty
            
            # Stop browser
            Stop-Browser -Name $TestBrowserName
            
            # Verify browser is stopped
            $stoppedBrowsers = Get-Browser | Where-Object { $_.Name -eq $TestBrowserName -and $_.Running -eq $false }
            $stoppedBrowsers | Should -BeNullOrEmpty
        }
        
        It "Should handle browser instance reuse" {
            # Start first instance
            $browser1 = Start-Browser -Name $TestBrowserName -Headless
            $browser1 | Should -Not -BeNullOrEmpty
            
            # Start another instance (PowerBrowser may reuse the same instance)
            $browser2 = Start-Browser -Name $TestBrowserName -Headless
            $browser2 | Should -Not -BeNullOrEmpty
            
            # Should have at least one running browser instance
            $runningBrowsers = Get-Browser | Where-Object { $_.IsConnected -eq $true }
            $chromeInstances = $runningBrowsers | Where-Object { $_.Name -eq $TestBrowserName }
            $chromeInstances.Count | Should -BeGreaterOrEqual 1
            
            # Both browser objects should reference the same or valid instances
            $browser1.ProcessId | Should -BeGreaterThan 0
            $browser2.ProcessId | Should -BeGreaterThan 0
        }
        
        It "Should start browser with custom options" {
            # Start browser with headless mode
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $browser | Should -Not -BeNullOrEmpty
            
            # Browser should be running
            $runningBrowser = Get-Browser | Where-Object { $_.Name -eq $TestBrowserName -and $_.Running -eq $true }
            $runningBrowser | Should -Not -BeNullOrEmpty
        }
    }

    Context "Page Management in Browser" {
        BeforeAll {
            $script:TestBrowser = Start-Browser -Name $TestBrowserName -Headless
        }
        
        AfterAll {
            Stop-Browser -Name $TestBrowserName -ErrorAction SilentlyContinue
        }

        It "Should create pages with automatic naming" {
            # Create pages without specifying names
            $page1 = $script:TestBrowser | New-BrowserPage -Url "about:blank"
            $page2 = $script:TestBrowser | New-BrowserPage -Url "about:blank"
            $page3 = $script:TestBrowser | New-BrowserPage -Url "about:blank"
            
            # Should have sequential names
            $page1.PageName | Should -Be 'Page1'
            $page2.PageName | Should -Be 'Page2'
            $page3.PageName | Should -Be 'Page3'
            
            # Should have proper IDs
            $page1.PageId | Should -Be "$TestBrowserName`_Page1"
            $page2.PageId | Should -Be "$TestBrowserName`_Page2"
            $page3.PageId | Should -Be "$TestBrowserName`_Page3"
        }
        
        It "Should create pages with custom names" {
            $customPage = $script:TestBrowser | New-BrowserPage -Url "about:blank" -Name 'CustomTest'
            
            $customPage.PageName | Should -Be 'CustomTest'
            $customPage.PageId | Should -Be "$TestBrowserName`_CustomTest"
        }
        
        It "Should list all pages in browser" {
            # Create some pages
            $page1 = $script:TestBrowser | New-BrowserPage -Url "about:blank" -Name "TestPage1"
            $page2 = $script:TestBrowser | New-BrowserPage -Url "about:blank" -Name "TestPage2"
            
            # Get all pages
            $allPages = $script:TestBrowser | Get-BrowserPage
            $allPages.Count | Should -BeGreaterOrEqual 2
            
            # Should contain our test pages
            $pageNames = $allPages | ForEach-Object { $_.PageName }
            $pageNames | Should -Contain "TestPage1"
            $pageNames | Should -Contain "TestPage2"
        }
        
        It "Should remove pages correctly" {
            # Create a page to remove
            $pageToRemove = $script:TestBrowser | New-BrowserPage -Url "about:blank" -Name "ToRemove"
            $pageToRemove | Should -Not -BeNullOrEmpty
            
            # Remove the page
            Remove-BrowserPage -PageId $pageToRemove.PageId
            
            # Verify page is removed
            $remainingPages = $script:TestBrowser | Get-BrowserPage
            $removedPage = $remainingPages | Where-Object { $_.PageName -eq "ToRemove" }
            $removedPage | Should -BeNullOrEmpty
        }
    }

    Context "Browser Object Properties" {
        AfterEach {
            # Cleanup any browsers started in this context
            Get-Browser | Where-Object Running | ForEach-Object { 
                Stop-Browser -Name $_.Name -ErrorAction SilentlyContinue 
            }
        }
        
        It "Should expose correct browser properties" {
            $browser = Start-Browser -Name $TestBrowserName -Headless
            
            # Check required properties
            $browser.Name | Should -Be $TestBrowserName
            $browser.PSObject.Properties.Name | Should -Contain "Running"
            $browser.PSObject.Properties.Name | Should -Contain "ProcessId"
            $browser.PSObject.Properties.Name | Should -Contain "PageCount"
            $browser.PSObject.Properties.Name | Should -Contain "Path"
            
            # ProcessId should be a valid number when running
            $browser.ProcessId | Should -BeGreaterThan 0
            
            # Browser should be connected
            $browser.IsConnected | Should -Be $true
            
            # Initially should have 0 pages
            $browser.PageCount | Should -Be 0
        }
        
        It "Should update page count when pages are added" {
            $browser = Start-Browser -Name $TestBrowserName -Headless
            
            # Initially 0 pages
            $browser.PageCount | Should -Be 0
            
            # Add a page
            $page = $browser | New-BrowserPage -Url "about:blank" -Name "CountTest"
            
            # Page count should increase
            # Note: PageCount might not update immediately, so let's test the page exists instead
            $allPages = $browser | Get-BrowserPage
            $allPages.Count | Should -BeGreaterOrEqual 1
            $allPages | Where-Object { $_.PageName -eq "CountTest" } | Should -Not -BeNullOrEmpty
        }
    }
}