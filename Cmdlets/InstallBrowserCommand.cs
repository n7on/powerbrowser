using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PuppeteerSharp;
using PowerBrowser.Models;

namespace PowerBrowser.Cmdlets
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
                EnsureBrowserStorageExists();
                
                // Use simple browser type name
                var browserName = BrowserType.ToString();
                var namedBrowserPath = GetNamedBrowserPath(browserName);
                
                // Check if browser is already installed
                if (Directory.Exists(namedBrowserPath))
                {
                    WriteVerbose($"✅ Browser '{browserName}' is already installed!");
                    WriteVerbose($"📍 Type: {BrowserType}");
                    WriteVerbose($"📁 Location: {namedBrowserPath}");
                    
                    // Create PowerBrowserInstance for existing installation
                    var existingInstance = CreateBrowserInstance(browserName, namedBrowserPath);
                    WriteObject(existingInstance);
                    return;
                }
                
                var executablePath = DownloadBrowserSync(browserName);
                
                WriteVerbose($"✅ Browser '{browserName}' installed successfully!");
                WriteVerbose($"📍 Type: {BrowserType}");
                WriteVerbose($"📁 Location: {executablePath}");
                
                // Create PowerBrowserInstance for new installation
                var newInstance = CreateBrowserInstance(browserName, namedBrowserPath);
                WriteObject(newInstance);
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
            WriteVerbose($"🔄 Downloading {BrowserType}... This may take a few minutes depending on your connection.");
            WriteVerbose($"Download path: {namedBrowserPath}");

            // Download the latest stable version
            var downloadTask = browserFetcher.DownloadAsync();
            var revisionInfo = downloadTask.GetAwaiter().GetResult();
            
            WriteVerbose($"✅ Download completed!");
            
            return revisionInfo.GetExecutablePath();
        }

        private PowerBrowserInstance CreateBrowserInstance(string browserName, string browserPath)
        {
            // Get size information
            var sizeInfo = GetDirectorySize(browserPath);
            
            // Create PowerBrowserInstance object (not running, so no actual IBrowser)
            var browserInstance = new PowerBrowserInstance(
                browserName, 
                null, // No IBrowser instance for installed-but-not-running browser
                false, // headless - unknown for installed browser
                "Unknown" // windowSize - unknown for installed browser
            );
            
            // Set additional properties for Install-Browser display
            browserInstance.Size = sizeInfo;
            browserInstance.Path = browserPath;
            
            return browserInstance;
        }

        private string GetDirectorySize(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return "Unknown";

                var totalBytes = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Sum(file => new FileInfo(file).Length);

                // Convert to human readable format
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = totalBytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
