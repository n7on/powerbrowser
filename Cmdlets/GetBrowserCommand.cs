using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Browser")]
    [OutputType(typeof(PowerBrowserInstance))]
    public class GetBrowserCommand : BrowserCmdletBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                var installedBrowsers = GetInstalledBrowserNames();
                
                if (!installedBrowsers.Any())
                {
                    WriteVerbose("No browsers are currently installed.");
                    WriteVerbose("Use 'Install-Browser -BrowserType <Type>' to install a browser.");
                    return;
                }

                WriteVerbose($"📋 Installed browsers ({installedBrowsers.Length}):");
                WriteVerbose("");

                foreach (var browserName in installedBrowsers)
                {
                    var browserPath = GetNamedBrowserPath(browserName);
                    
                    // Get size information
                    var sizeInfo = GetDirectorySize(browserPath);
                    
                    // Check if browser is running
                    var runningStatus = GetBrowserRunningStatus(browserName);
                    
                    // Get the actual browser instance if it's running
                    IBrowser actualBrowser = null;
                    var sessionStore = SessionState.PSVariable;
                    var browserInstances = sessionStore.GetValue("PowerBrowserInstances") as Dictionary<string, IBrowser>;
                    
                    if (browserInstances != null && browserInstances.ContainsKey(browserName))
                    {
                        actualBrowser = browserInstances[browserName];
                    }
                    
                    // Create PowerBrowserInstance object
                    var browserInstance = new PowerBrowserInstance(
                        browserName, 
                        actualBrowser,
                        false, // headless - we don't know this from Get-Browser
                        "Unknown" // windowSize - we don't know this from Get-Browser
                    );
                    
                    // Set additional properties for Get-Browser display
                    browserInstance.Size = sizeInfo;
                    browserInstance.Path = browserPath;
                    
                    WriteObject(browserInstance);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetBrowserFailed", ErrorCategory.OperationStopped, null));
            }
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

        private BrowserRunningStatus GetBrowserRunningStatus(string browserName)
        {
            try
            {
                var sessionStore = SessionState.PSVariable;
                var browserInstances = sessionStore.GetValue("PowerBrowserInstances") as Dictionary<string, IBrowser>;

                if (browserInstances != null && browserInstances.ContainsKey(browserName))
                {
                    var browser = browserInstances[browserName];
                    if (browser.IsConnected)
                    {
                        try
                        {
                            var pages = browser.PagesAsync().GetAwaiter().GetResult();
                            return new BrowserRunningStatus
                            {
                                IsRunning = true,
                                ProcessId = browser.Process?.Id ?? -1,
                                PageCount = pages.Length.ToString()
                            };
                        }
                        catch
                        {
                            return new BrowserRunningStatus
                            {
                                IsRunning = true,
                                ProcessId = browser.Process?.Id ?? -1,
                                PageCount = "Unknown"
                            };
                        }
                    }
                }

                return new BrowserRunningStatus { IsRunning = false, ProcessId = -1, PageCount = "-" };
            }
            catch
            {
                return new BrowserRunningStatus { IsRunning = false, ProcessId = -1, PageCount = "-" };
            }
        }
    }

    public class BrowserRunningStatus
    {
        public bool IsRunning { get; set; }
        public int ProcessId { get; set; }
        public string PageCount { get; set; } = "-";
    }
}
