#Requires -Modules Pester

BeforeAll {
    # Import the module
    $ModulePath = Join-Path $PSScriptRoot '..' 'bin' 'Debug' 'netstandard2.0' 'PowerBrowser.dll'
    Import-Module $ModulePath -Force
    
    # Test configuration
    $TestBrowserName = 'Chrome'
    $TestTimeout = 30
}

Describe "PowerBrowser Core Functionality" {
    Context "Browser Management" {
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

        It "Should list available browsers" {
            $browsers = Get-Browser
            $browsers | Should -Not -BeNullOrEmpty
            
            # Filter out any string output and get only browser objects
            $browserObjects = $browsers | Where-Object { $_ -is [object] -and $_.Name }
            $browserObjects.Count | Should -BeGreaterThan 0
            $browserObjects[0].Name | Should -Not -BeNullOrEmpty
        }

        It "Should start and stop browser successfully" {
            $browser = Start-Browser -Name $TestBrowserName
            
            $browser | Should -Not -BeNullOrEmpty
            $browser.Name | Should -Be $TestBrowserName
            $browser.IsConnected | Should -Be $true
            
            # Verify browser is running
            $runningBrowser = Get-Browser | Where-Object { $_.Name -eq $TestBrowserName -and $_.Running }
            $runningBrowser | Should -Not -BeNullOrEmpty
            
            # Stop browser
            Stop-Browser -Name $TestBrowserName
            
            # Verify browser is stopped
            $stoppedBrowser = Get-Browser | Where-Object { $_.Name -eq $TestBrowserName -and $_.Running }
            $stoppedBrowser | Should -BeNullOrEmpty
        }

        It "Should handle browser start failure gracefully" {
            { Start-Browser -Name 'NonExistentBrowser' -ErrorAction Stop } | Should -Throw
        }
    }

    Context "Page Management" {
        BeforeAll {
            $script:TestBrowser = Start-Browser -Name $TestBrowserName
        }
        
        AfterAll {
            Stop-Browser -Name $TestBrowserName -ErrorAction SilentlyContinue
        }

        It "Should create pages with sequential names" {
            $page1 = New-BrowserPage -BrowserName $TestBrowserName
            $page2 = New-BrowserPage -BrowserName $TestBrowserName
            $page3 = New-BrowserPage -BrowserName $TestBrowserName
            
            $page1.PageName | Should -Be 'Page1'
            $page2.PageName | Should -Be 'Page2'
            $page3.PageName | Should -Be 'Page3'
            
            $page1.PageId | Should -Be "$TestBrowserName`_Page1"
            $page2.PageId | Should -Be "$TestBrowserName`_Page2"
            $page3.PageId | Should -Be "$TestBrowserName`_Page3"
        }

        It "Should create page with custom name" {
            $customPage = New-BrowserPage -BrowserName $TestBrowserName -Name 'CustomTest'
            
            $customPage.PageName | Should -Be 'CustomTest'
            $customPage.PageId | Should -Be "$TestBrowserName`_CustomTest"
        }

        It "Should handle custom name conflicts" {
            New-BrowserPage -BrowserName $TestBrowserName -Name 'DuplicateName'
            
            { New-BrowserPage -BrowserName $TestBrowserName -Name 'DuplicateName' -ErrorAction Stop } | 
            Should -Throw "*already exists*"
        }

        It "Should navigate to URL when specified" {
            $page = New-BrowserPage -BrowserName $TestBrowserName -Url 'https://httpbin.org/html'
            
            # Give page time to load
            Start-Sleep -Seconds 2
            
            $page.Url | Should -Match 'httpbin.org'
        }
    }

    Context "Pipeline Integration" {
        BeforeAll {
            $script:TestBrowser = Start-Browser -Name $TestBrowserName
        }
        
        AfterAll {
            Stop-Browser -Name $TestBrowserName -ErrorAction SilentlyContinue
        }

        It "Should support browser to page pipeline" {
            $page = $script:TestBrowser | New-BrowserPage
            
            $page | Should -Not -BeNullOrEmpty
            $page.BrowserName | Should -Be $TestBrowserName
            $page.PageName | Should -Match '^Page\d+$'
        }

        It "Should support page to element finding pipeline" {
            $page = New-BrowserPage -BrowserName $TestBrowserName -Url 'https://httpbin.org/html'
            Start-Sleep -Seconds 2
            
            $element = $page | Find-BrowserElement -Selector 'h1'
            
            $element | Should -Not -BeNullOrEmpty
            $element.TagName | Should -Be 'H1'
        }
    }

    Context "Element Interaction" {
        BeforeAll {
            $script:TestBrowser = Start-Browser -Name $TestBrowserName
            $script:TestPage = New-BrowserPage -BrowserName $TestBrowserName -Url 'https://httpbin.org/forms/post'
            Start-Sleep -Seconds 3  # Wait for page to load
        }
        
        AfterAll {
            Stop-Browser -Name $TestBrowserName -ErrorAction SilentlyContinue
        }

        It "Should find elements by selector" {
            $element = Find-BrowserElement -Page $script:TestPage -Selector 'input[name="custname"]'
            
            $element | Should -Not -BeNullOrEmpty
            $element.TagName | Should -Be 'INPUT'
        }

        It "Should type text into elements" {
            $element = Find-BrowserElement -Page $script:TestPage -Selector 'input[name="custname"]'
            $result = $element | Set-BrowserElementText -Text 'Test Customer'
            
            $result | Should -Not -BeNullOrEmpty
        }

        It "Should click elements" {
            $element = Find-BrowserElement -Page $script:TestPage -Selector 'input[name="custname"]'
            $result = $element | Invoke-BrowserElementClick
            
            $result | Should -Not -BeNullOrEmpty
        }

        It "Should get element attributes" {
            $element = Find-BrowserElement -Page $script:TestPage -Selector 'input[name="custname"]'
            $result = $element | Get-BrowserElementAttribute -AttributeName 'name'
            
            # The result is a PowerBrowserElement with AttributeValue property
            $result.AttributeValue | Should -Be 'custname'
        }
    }

    Context "Error Handling" {
        BeforeAll {
            $script:TestBrowser = Start-Browser -Name $TestBrowserName
        }
        
        AfterAll {
            Stop-Browser -Name $TestBrowserName -ErrorAction SilentlyContinue
        }

        It "Should handle missing browser gracefully" {
            { New-BrowserPage -BrowserName 'NonExistentBrowser' -ErrorAction Stop } | 
            Should -Throw "*not running*"
        }

        It "Should handle invalid selectors gracefully" {
            $page = New-BrowserPage -BrowserName $TestBrowserName -Url 'https://httpbin.org/html'
            Start-Sleep -Seconds 2
            
            { Find-BrowserElement -Page $page -Selector 'invalid-selector-that-does-not-exist' -ErrorAction Stop } | 
            Should -Throw "*failed*"
        }
    }
}

Describe "PowerBrowser Regex Implementation" {
    Context "Page Name Generation" {
        BeforeAll {
            $script:TestBrowser = Start-Browser -Name $TestBrowserName
        }
        
        AfterAll {
            Stop-Browser -Name $TestBrowserName -ErrorAction SilentlyContinue
        }

        It "Should generate sequential page numbers with regex" {
            $pages = @()
            $pages += New-BrowserPage -BrowserName $TestBrowserName
            $pages += New-BrowserPage -BrowserName $TestBrowserName -Name 'CustomPage'
            $pages += New-BrowserPage -BrowserName $TestBrowserName
            $pages += New-BrowserPage -BrowserName $TestBrowserName
            
            $pages[0].PageName | Should -Be 'Page1'
            $pages[1].PageName | Should -Be 'CustomPage'
            $pages[2].PageName | Should -Be 'Page2'
            $pages[3].PageName | Should -Be 'Page3'
        }

        It "Should handle custom names in numbering sequence" {
            $page1 = New-BrowserPage -BrowserName $TestBrowserName  # Page1
            $page5 = New-BrowserPage -BrowserName $TestBrowserName -Name 'Page5'  # Custom Page5
            $pageNext = New-BrowserPage -BrowserName $TestBrowserName  # Should be Page6 (max + 1)
            
            $page1.PageName | Should -Be 'Page1'
            $page5.PageName | Should -Be 'Page5'
            $pageNext.PageName | Should -Be 'Page6'  # Current logic: finds max numeric page (5) and adds 1
        }
    }
}