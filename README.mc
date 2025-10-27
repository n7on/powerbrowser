# PowerBrowser
PowerBrowser is a native PowerShell module made for browser automation. It's build upon PuppeteerSharp, which is a .net library using DevTools API in order to automate the browser. It targets the netstandard 2.0, so it's fully supported on all powershell versions. 



# Development

## Windows
Uninstall the old `Pester`that is shipped with Windows, and never updated.

``` PowerShell
$module = "C:\Program Files\WindowsPowerShell\Modules\Pester"
takeown /F $module /A /R
icacls $module /reset
icacls $module /grant "*S-1-5-32-544:F" /inheritance:d /T
Remove-Item -Path $module -Recurse -Force -Confirm:$false

Install-Module -Name Pester  -MaximumVersion 4.99 -Force

```