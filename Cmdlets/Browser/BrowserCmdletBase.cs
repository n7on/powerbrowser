using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using PuppeteerSharp;
using PowerBrowser.Models;
using System.Management.Automation;

namespace PowerBrowser.Cmdlets.Browser
{
    public abstract class BrowserCmdletBase : PSCmdlet
    {
        /// <summary>
        /// Gets the browser storage path where all browsers are installed
        /// </summary>
        protected string GetBrowserStoragePath()
        {
            var userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var browserPath = Path.Combine(userDataPath, "PowerBrowser", "Browsers");
            
            WriteVerbose($"Browser storage path: {browserPath}");
            return browserPath;
        }


        /// <summary>
        /// Gets the path for a specific named browser installation
        /// </summary>
        protected string GetNamedBrowserPath(string browserName)
        {
            var storagePath = GetBrowserStoragePath();
            return Path.Combine(storagePath, browserName);
        }

        /// <summary>
        /// Gets all installed browser names
        /// </summary>
        protected string[] GetInstalledBrowserNames()
        {
            var storagePath = GetBrowserStoragePath();
            if (!Directory.Exists(storagePath))
                return Array.Empty<string>();

            return Directory.GetDirectories(storagePath)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Cast<string>()
                .OrderBy(name => name)
                .ToArray();
        }
        protected string GetDirectorySize(string path)
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
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
            catch
            {
                return "Unknown";
            }
        }

        protected BrowserRunningStatus GetBrowserRunningStatus(string browserName)
        {
            try
            {
                var browserInstances = GetBrowserInstances();

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

        protected void DownloadBrowserSync(string browserName, SupportedBrowser BrowserType)
        {
            var namedBrowserPath = GetNamedBrowserPath(browserName);
            Directory.CreateDirectory(namedBrowserPath);
            
            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = namedBrowserPath,
                Browser = BrowserType
            });

            var downloadTask = browserFetcher.DownloadAsync();
            var revisionInfo = downloadTask.GetAwaiter().GetResult();
            
            WriteVerbose($"Download completed!");
        }

        protected PowerBrowserInstance CreateBrowserInstance(string browserName, string browserPath)
        {
            // Get size information
            var sizeInfo = GetDirectorySize(browserPath);

            var runningStatus = GetBrowserRunningStatus(browserName);
            // Create PowerBrowserInstance object (not running, so no actual IBrowser)
            var browserInstance = new PowerBrowserInstance(
                browserName,
                null, // No IBrowser instance for installed-but-not-running browser
                false, // headless - unknown for installed browser
                "Unknown" // windowSize - unknown for installed browser
            )
            {
                // Set additional properties for Install-Browser display
                Size = sizeInfo,
                Path = browserPath
            };

            return browserInstance;
        }

        private Dictionary<string, IBrowser> GetBrowserInstances()
        {
            // This method should return the currently active browser instances.
            // Implementation depends on how browser instances are managed in the application.
            // For now, returning an empty dictionary to avoid breaking changes.
            return new Dictionary<string, IBrowser>();
        }

        protected Dictionary<string, IBrowser> GetRunningBrowsers()
        {
            WriteVerbose("Retrieving running browsers from tracked instances.");

            var runningBrowsers = new Dictionary<string, IBrowser>();

            // Access the static list of running browsers from StartBrowserCommand
            foreach (var browser in StartBrowserCommand.RunningBrowsers)
            {
                if (browser.IsConnected)
                {
                    runningBrowsers[browser.Process.Id.ToString()] = browser;
                }
            }

            WriteVerbose($"Found {runningBrowsers.Count} running browser(s).");
            return runningBrowsers;
        }
    }
    public class BrowserRunningStatus
    {
        public bool IsRunning { get; set; }
        public int ProcessId { get; set; }
        public string PageCount { get; set; } = "-";
    }
}
