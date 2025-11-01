using System;
using System.IO;
using System.Management.Automation;
using PowerBrowser.Transport;
using PowerBrowser.Services;
using PowerBrowser.Common;

namespace PowerBrowser.Commands.Browser
{
    [Cmdlet(VerbsLifecycle.Install, "Browser")]
    [OutputType(typeof(PBrowser))]
    public class InstallBrowserCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            HelpMessage = "Browser type to install (Chrome, Firefox, or Chromium - use -Headless flag with Start-Browser instead of ChromeHeadlessShell)")]
        public SupportedPBrowser BrowserType { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var browserService = ServiceFactory.CreateBrowserService(SessionState);

                
                // Check if browser is already installed
                if (!browserService.IsBrowserTypeInstalled(BrowserType))
                {
                    WriteVerbose($"Downloading {BrowserType}... This may take a few minutes depending on your connection.");
                    browserService.DownloadBrowser(BrowserType);
                }
                else
                {
                    WriteVerbose($"{BrowserType} is already installed.");
                }
                
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "BrowserInstallFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}
