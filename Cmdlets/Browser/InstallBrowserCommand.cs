using System;
using System.IO;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Helpers;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsLifecycle.Install, "Browser")]
    [OutputType(typeof(PowerBrowserInstance))]
    public class InstallBrowserCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            HelpMessage = "Browser type to install (Chrome, Firefox, or Chromium - use -Headless flag with Start-Browser instead of ChromeHeadlessShell)")]
        public SupportedBrowser BrowserType { get; set; } = SupportedBrowser.Chrome;

        protected override void ProcessRecord()
        {
            try
            {
                var browserName = BrowserType.ToString();
                var path = BrowserHelper.GetBrowserInstancePath(BrowserType);
                
                // Check if browser is already installed
                if (!Directory.Exists(path))
                {
                    WriteVerbose($"Downloading {BrowserType}... This may take a few minutes depending on your connection.");
                    BrowserHelper.DownloadBrowser(BrowserType);
                }
                else
                {
                    WriteVerbose($"{BrowserType} is already installed at {path}.");
                }
                
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "BrowserInstallFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}
