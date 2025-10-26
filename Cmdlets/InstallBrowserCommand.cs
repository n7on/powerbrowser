using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Install, "Browser")]
    [OutputType(typeof(string))]
    public class InstallBrowserCommand : PSCmdlet
    {
        [Parameter(HelpMessage = "Browser revision to download")]
        public string? Revision { get; set; }

        [Parameter(HelpMessage = "Custom path where browsers should be downloaded")]
        public string? BrowserPath { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                // Use custom path if provided, otherwise use user-specific directory
                string browserPath;
                if (!string.IsNullOrEmpty(BrowserPath))
                {
                    browserPath = BrowserPath;
                }
                else
                {
                    var userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    browserPath = Path.Combine(userDataPath, "PowerBrowser", "Browsers");
                    
                    // Debug: Show what path we're using
                    WriteVerbose($"UserDataPath: {userDataPath}");
                    WriteVerbose($"Computed browser path: {browserPath}");
                }
                
                // Ensure the directory exists
                Directory.CreateDirectory(browserPath);
                
                // Show progress before starting download
                var progress = new ProgressRecord(0, "Downloading Browser", 
                    $"Downloading Chromium browser to: {browserPath}");
                WriteProgress(progress);
                
                // Perform the download synchronously
                var result = DownloadBrowserSync(browserPath);
                
                // Complete the progress
                progress.RecordType = ProgressRecordType.Completed;
                WriteProgress(progress);
                
                WriteObject($"Browser installed successfully at: {result}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "BrowserInstallFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private string DownloadBrowserSync(string browserPath)
        {
            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = browserPath
            });

            var downloadTask = string.IsNullOrEmpty(Revision)
                ? browserFetcher.DownloadAsync()
                : browserFetcher.DownloadAsync(Revision);
            
            // Wait for the download to complete synchronously
            var revisionInfo = downloadTask.GetAwaiter().GetResult();
            
            return revisionInfo.GetExecutablePath();
        }
    }
}