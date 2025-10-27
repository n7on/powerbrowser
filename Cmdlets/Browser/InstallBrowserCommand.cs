using System;
using System.IO;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsLifecycle.Install, "Browser")]
    [OutputType(typeof(PowerBrowserInstance))]
    public class InstallBrowserCommand : BrowserCmdletBase
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
                var namedBrowserPath = GetNamedBrowserPath(browserName);
                
                // Check if browser is already installed
                if (!Directory.Exists(namedBrowserPath))
                {
                    WriteVerbose($"Downloading {BrowserType}... This may take a few minutes depending on your connection.");
                    DownloadBrowserSync(browserName, BrowserType);
                }
                
                WriteObject(CreateBrowserInstance(browserName, namedBrowserPath));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "BrowserInstallFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}
