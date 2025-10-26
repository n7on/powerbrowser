@{
    RootModule = 'PowerBrowser.dll'
    ModuleVersion = '1.0.0'
    GUID = '17b431d1-d9da-44e6-b740-8ad3bfb4c0cf'
    Author = 'Anton Lindström'
    CompanyName = 'Anton Lindström'
    Copyright = '(c) 2025 Anton Lindström. All rights reserved.'
    Description = 'PowerShell module for browser automation powered by PuppeteerSharp'
    
    PowerShellVersion = '5.1'
    DotNetFrameworkVersion = '4.7.2'
    
    FunctionsToExport = @()
    CmdletsToExport = @(
        'Install-Browser',
        'Start-Browser',
        'Stop-Browser',
        'New-BrowserPage',
        'Enter-BrowserUrl',
        'Get-BrowserScreenshot',
        'Find-BrowserElement',
        'Invoke-BrowserClick',
        'Set-BrowserInput',
        'Get-BrowserContent'
    )
    VariablesToExport = @()
    AliasesToExport = @('Navigate-Browser', 'Export-BrowserScreenshot')
    
    PrivateData = @{
        PSData = @{
            Tags = @('Browser', 'Automation', 'PowerShell', 'WebScraping', 'Testing', 'Puppeteer', 'Chrome')
            LicenseUri = 'https://github.com/yourusername/PowerBrowser/blob/main/LICENSE'
            ProjectUri = 'https://github.com/yourusername/PowerBrowser'
            IconUri = ''
            ReleaseNotes = @'
Initial release of PowerBrowser v1.0.0

Features:
- Install and launch Chrome/Chromium browsers
- Create and manage browser pages
- Navigate to URLs
- Take screenshots
- Find and interact with page elements
- Click elements and fill forms
- Extract page content

Powered by PuppeteerSharp
'@
        }
    }
}