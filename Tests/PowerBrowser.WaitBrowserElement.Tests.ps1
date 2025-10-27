#Requires -Modules Pester

Describe "Wait-BrowserElement Cmdlet Tests" -Tags @("Wait", "Element", "Core") {
    BeforeAll {
        $ModulePath = Join-Path $PSScriptRoot '..' 'bin' 'Debug' 'netstandard2.0' 'PowerBrowser.dll'
        Import-Module $ModulePath -Force
        $TestBrowserName = 'Chrome'
    }

    BeforeEach {
        $browser = Start-Browser -Name $TestBrowserName -Headless
        $page = $browser | New-BrowserPage -Name "WaitTest"
        $page | Move-BrowserPage -Url "https://example.com" -WaitForLoad
    }

    It "Should wait for element to be visible" {
        $result = $page | Wait-BrowserElement -Selector "h1" -Condition Visible -Timeout 5000
        $result | Should -Not -BeNullOrEmpty
        $result.TagName | Should -Be "H1"
    }

    It "Should wait for element to be hidden (should fail)" {
        { $page | Wait-BrowserElement -Selector "h1" -Condition Hidden -Timeout 1000 -ErrorAction Stop } | Should -Throw -ErrorId "ResourceUnavailable,PowerBrowser.Exceptions.ResourceUnavailableException,PowerBrowser.Cmdlets.WaitBrowserElementCommand"
    }

    It "Should wait for element to be enabled" {
        $result = $page | Wait-BrowserElement -Selector "h1" -Condition Enabled -Timeout 5000
        $result | Should -Not -BeNullOrEmpty
    }

    It "Should wait for element to be disabled (should fail)" {
        { $page | Wait-BrowserElement -Selector "h1" -Condition Disabled -Timeout 1000 -ErrorAction Stop } | Should -Throw -ErrorId "ResourceUnavailable,PowerBrowser.Exceptions.ResourceUnavailableException,PowerBrowser.Cmdlets.WaitBrowserElementCommand"
    }

    It "Should wait for element text to contain value" {
        $result = $page | Wait-BrowserElement -Selector "h1" -Condition TextContains -Value "Example Domain" -Timeout 5000
        $result | Should -Not -BeNullOrEmpty
    }

    It "Should wait for element attribute to equal value (should fail)" {
        { $page | Wait-BrowserElement -Selector "h1" -Condition AttributeEquals -Attribute "id" -Value "notfound" -Timeout 1000 -ErrorAction Stop } | Should -Throw -ErrorId "ResourceUnavailable,PowerBrowser.Exceptions.ResourceUnavailableException,PowerBrowser.Cmdlets.WaitBrowserElementCommand"
    }

    It "Should throw for unknown condition" {
        { $page | Wait-BrowserElement -Selector "h1" -Condition "NotACondition" -Timeout 1000 -ErrorAction Stop } | Should -Throw -ErrorId "RequiredParameter,PowerBrowser.Exceptions.RequiredParameterException,PowerBrowser.Cmdlets.WaitBrowserElementCommand"
    }

    It "Should throw for missing selector" {
        { $page | Wait-BrowserElement -Selector "" -Condition Visible -Timeout 1000 -ErrorAction Stop } | Should -Throw -ErrorId "ParameterArgumentValidationErrorEmptyStringNotAllowed,PowerBrowser.Cmdlets.WaitBrowserElementCommand"
    }
}
