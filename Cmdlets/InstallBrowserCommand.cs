using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Install, "Browser")]
    [OutputType(typeof(string))]
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
                EnsureBrowserStorageExists();
                
                // Use simple browser type name
                var browserName = BrowserType.ToString();
                var namedBrowserPath = GetNamedBrowserPath(browserName);
                
                // Check if browser is already installed
                if (Directory.Exists(namedBrowserPath))
                {
                    WriteObject($"✅ Browser '{browserName}' is already installed!");
                    WriteObject($"📍 Type: {BrowserType}");
                    WriteObject($"📁 Location: {namedBrowserPath}");
                    return;
                }
                
                var result = DownloadBrowserSync(browserName);
                
                WriteObject($"✅ Browser '{browserName}' installed successfully!");
                WriteObject($"📍 Type: {BrowserType}");
                WriteObject($"📁 Location: {result}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "BrowserInstallFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private string DownloadBrowserSync(string browserName)
        {
            var namedBrowserPath = GetNamedBrowserPath(browserName);
            Directory.CreateDirectory(namedBrowserPath);
            
            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = namedBrowserPath,
                Browser = BrowserType
            });

            // Show download status with simple messages
            WriteObject($"🔄 Downloading {BrowserType}... This may take a few minutes depending on your connection.");
            WriteVerbose($"Download path: {namedBrowserPath}");

            // Download the latest stable version
            var downloadTask = browserFetcher.DownloadAsync();
            var revisionInfo = downloadTask.GetAwaiter().GetResult();
            
            WriteObject($"✅ Download completed!");
            
            return revisionInfo.GetExecutablePath();
        }
    }
}
