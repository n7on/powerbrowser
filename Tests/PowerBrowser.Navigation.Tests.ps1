#Requires -Modules Pester

Describe "Move-BrowserPage Navigation Tests" -Tags @("Navigation", "Core") {
    BeforeAll {
        # Import the module
        $ModulePath = Join-Path $PSScriptRoot '..' 'bin' 'Debug' 'netstandard2.0' 'PowerBrowser.dll'
        Import-Module $ModulePath -Force
        
        # Test configuration
        $TestBrowserName = 'Chrome'
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

    Context "Basic Navigation" {
        It "Should navigate to a URL and update page properties" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "NavTest"
            
            # Initial state
            $page.Url | Should -Be "about:blank"
            
            # Act
            $result = $page | Move-BrowserPage -Url "https://example.com" -WaitForLoad
            
            # Assert
            $result | Should -Not -BeNullOrEmpty
            $result.Url | Should -BeLike "*example.com*"
            $result.Title | Should -Not -BeNullOrEmpty
            $page.Url | Should -BeLike "*example.com*"  # Original object should be updated
        }
        
        It "Should handle pipeline input correctly" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "PipelineTest"
            
            # Act - pipeline the page object
            $result = $page | Move-BrowserPage -Url "https://httpbin.org/user-agent"
            
            # Assert
            $result.PageName | Should -Be "PipelineTest"
            $result.Url | Should -BeLike "*httpbin.org*"
        }
        
        It "Should work with page name parameter" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "NamedTest"
            
            # Act - use page name instead of object
            $result = Move-BrowserPage -PageName "NamedTest" -Url "https://example.com"
            
            # Assert
            $result.PageName | Should -Be "NamedTest"
            $result.Url | Should -BeLike "*example.com*"
        }
    }
    
    Context "Navigation Options" {
        It "Should respect timeout parameter" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "TimeoutTest"
            
            # Act & Assert - Should complete within reasonable time
            $measure = Measure-Command {
                $page | Move-BrowserPage -Url "https://example.com" -Timeout 5000 -WaitForLoad
            }
            
            $measure.TotalMilliseconds | Should -BeLessThan 5000
        }
        
        It "Should support different WaitUntil options" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "WaitTest"
            
            # Act - Test DOMContentLoaded
            $result = $page | Move-BrowserPage -Url "https://example.com" -WaitUntil DOMContentLoaded
            
            # Assert
            $result.Url | Should -BeLike "*example.com*"
        }
    }
    
    Context "URL Validation and Handling" {
        It "Should handle absolute URLs correctly" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "AbsoluteTest"
            
            # Act
            $result = $page | Move-BrowserPage -Url "https://httpbin.org/json"
            
            # Assert
            $result.Url | Should -BeLike "https://httpbin.org/json*"
        }
        
        It "Should auto-prefix URLs without protocol" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "PrefixTest"
            
            # Act
            $result = $page | Move-BrowserPage -Url "example.com"
            
            # Assert
            $result.Url | Should -BeLike "http://example.com*"
        }
    }
    
    Context "Error Handling" {
        It "Should throw error for invalid URL" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "ErrorTest"
            
            # Act & Assert
            { $page | Move-BrowserPage -Url "not-a-valid-url://invalid" -ErrorAction Stop } | 
                Should -Throw
        }
        
        It "Should throw error for non-existent page" {
            # Act & Assert
            { Move-BrowserPage -PageName "NonExistentPage" -Url "https://example.com" -ErrorAction Stop } | 
                Should -Throw
        }
        
        It "Should handle empty URL parameter" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $page = $browser | New-BrowserPage -Name "EmptyUrlTest"
            
            # Act & Assert
            { $page | Move-BrowserPage -Url "" -ErrorAction Stop } | 
                Should -Throw
        }
    }
    
    Context "Return Value Validation" {
        It "Should return the same PowerBrowserPage object with updated properties" {
            # Arrange
            $browser = Start-Browser -Name $TestBrowserName -Headless
            $originalPage = $browser | New-BrowserPage -Name "ReturnTest"
            $originalPageId = $originalPage.PageId
            
            # Act
            $returnedPage = $originalPage | Move-BrowserPage -Url "https://example.com"
            
            # Assert - Should be the same object instance with updated properties
            $returnedPage.PageId | Should -Be $originalPageId
            $returnedPage.PageName | Should -Be $originalPage.PageName
            $returnedPage.Url | Should -BeLike "*example.com*"
            $returnedPage.Url | Should -Not -Be "about:blank"
        }
    }
}