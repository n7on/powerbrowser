using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Browser")]
    [OutputType(typeof(PSObject))]
    public class GetBrowserCommand : BrowserCmdletBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                var installedBrowsers = GetInstalledBrowserNames();
                
                if (!installedBrowsers.Any())
                {
                    WriteObject("No browsers are currently installed.");
                    WriteObject("Use 'Install-Browser -BrowserType <Type>' to install a browser.");
                    return;
                }

                WriteObject($"📋 Installed browsers ({installedBrowsers.Length}):");
                WriteObject("");

                foreach (var browserName in installedBrowsers)
                {
                    var browserPath = GetNamedBrowserPath(browserName);
                    var browserInfo = new PSObject();
                    
                    // Get size information
                    var sizeInfo = GetDirectorySize(browserPath);
                    
                    // Check if browser is running
                    var runningStatus = GetBrowserRunningStatus(browserName);
                    
                    browserInfo.Properties.Add(new PSNoteProperty("Name", browserName));
                    browserInfo.Properties.Add(new PSNoteProperty("Type", browserName)); // Same as name now
                    browserInfo.Properties.Add(new PSNoteProperty("Size", sizeInfo));
                    browserInfo.Properties.Add(new PSNoteProperty("Running", runningStatus.IsRunning));
                    browserInfo.Properties.Add(new PSNoteProperty("ProcessId", runningStatus.ProcessId));
                    browserInfo.Properties.Add(new PSNoteProperty("PageCount", runningStatus.PageCount));
                    browserInfo.Properties.Add(new PSNoteProperty("Path", browserPath));
                    
                    WriteObject(browserInfo);
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
