using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "BrowserPage")]
    [OutputType(typeof(PowerBrowserPage))]
    public class NewBrowserPageCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserInstance object from Start-Browser or browser name")]
        [ArgumentCompleter(typeof(RunningBrowserCompleter))]
        public object Browser { get; set; }

        [Parameter(
            HelpMessage = "Name of the running browser (used when Browser parameter is not provided)")]
        [ArgumentCompleter(typeof(RunningBrowserCompleter))]
        public string BrowserName { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Custom name for the page (if not specified, will be Page1, Page2, etc.)")]
        public string Name { get; set; } = string.Empty;

        [Parameter(HelpMessage = "URL to navigate to when creating the page")]
        public string Url { get; set; } = "about:blank";

        [Parameter(HelpMessage = "Wait for page to load completely before returning")]
        public SwitchParameter WaitForLoad { get; set; }

        [Parameter(HelpMessage = "Page width (default: 1280)")]
        public int Width { get; set; } = 1280;

        [Parameter(HelpMessage = "Page height (default: 720)")]
        public int Height { get; set; } = 720;

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve browser instance and name from various input sources
                var (browserInstance, actualBrowserName) = ResolveBrowserInstance();
                
                if (browserInstance == null)
                {
                    return; // Error already written in ResolveBrowserInstance
                }

                var browser = browserInstance;

                if (!browser.IsConnected)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Browser '{actualBrowserName}' is no longer connected."),
                        "BrowserDisconnected", ErrorCategory.ConnectionError, actualBrowserName));
                    return;
                }

                WriteVerbose($"📄 Creating new page in {actualBrowserName}...");

                var page = CreatePageSync(browser);
                
                var sessionStore = SessionState.PSVariable;

                // Store the page for backward compatibility and cross-command access
                var pageInstances = sessionStore.GetValue("PowerBrowserPages") as Dictionary<string, IPage> 
                    ?? new Dictionary<string, IPage>();
                var powerPageInstances = sessionStore.GetValue("PowerBrowserPageObjects") as Dictionary<string, PowerBrowserPage>
                    ?? new Dictionary<string, PowerBrowserPage>();
                
                var pageName = GeneratePageName(pageInstances, actualBrowserName);
                var pageId = $"{actualBrowserName}_{pageName}";
                
                // Check if page ID already exists
                if (pageInstances.ContainsKey(pageId))
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Page with name '{pageName}' already exists for browser '{actualBrowserName}'."),
                        "PageNameExists", ErrorCategory.ResourceExists, pageName));
                    return;
                }
                
                // Get or create PowerBrowserInstance
                var powerBrowserInstances = sessionStore.GetValue("PowerBrowserObjects") as Dictionary<string, PowerBrowserInstance>;
                PowerBrowserInstance powerBrowserInstance = null;
                
                if (powerBrowserInstances != null && powerBrowserInstances.ContainsKey(actualBrowserName))
                {
                    powerBrowserInstance = powerBrowserInstances[actualBrowserName];
                }
                else
                {
                    // Create a temporary PowerBrowserInstance for backward compatibility
                    powerBrowserInstance = new PowerBrowserInstance(actualBrowserName, browser, false, $"{Width}x{Height}");
                }

                // Create PowerBrowserPage wrapper
                var powerPage = new PowerBrowserPage(
                    pageId,
                    pageName,
                    powerBrowserInstance,
                    page,
                    Width,
                    Height
                );
                
                pageInstances[pageId] = page;
                powerPageInstances[pageId] = powerPage;
                sessionStore.Set("PowerBrowserPages", pageInstances);
                sessionStore.Set("PowerBrowserPageObjects", powerPageInstances);

                WriteVerbose($"✅ Page '{pageName}' created successfully!");
                WriteVerbose($"📋 Page ID: {pageId}");
                WriteVerbose($"🌐 URL: {page.Url}");
                WriteVerbose($"📏 Viewport: {Width}x{Height}");

                // Return the PowerBrowserPage object for pipeline chaining
                WriteObject(powerPage);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "NewBrowserPageFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private IPage CreatePageSync(IBrowser browser)
        {
            var page = browser.NewPageAsync().GetAwaiter().GetResult();

            // Set viewport size
            page.SetViewportAsync(new ViewPortOptions
            {
                Width = Width,
                Height = Height
            }).GetAwaiter().GetResult();

            // Navigate to URL if specified
            if (!string.IsNullOrEmpty(Url) && Url != "about:blank")
            {
                WriteVerbose($"Navigating to: {Url}");
                
                if (WaitForLoad.IsPresent)
                {
                    page.GoToAsync(Url, new NavigationOptions
                    {
                        WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded }
                    }).GetAwaiter().GetResult();
                }
                else
                {
                    page.GoToAsync(Url).GetAwaiter().GetResult();
                }
            }

            return page;
        }

        private (IBrowser browser, string browserName) ResolveBrowserInstance()
        {
            var sessionStore = SessionState.PSVariable;
            var browserInstances = sessionStore.GetValue("PowerBrowserInstances") as Dictionary<string, IBrowser>;
            var powerBrowserInstances = sessionStore.GetValue("PowerBrowserObjects") as Dictionary<string, PowerBrowserInstance>;

            if (browserInstances == null)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("No browsers are running. Use Start-Browser first."),
                    "NoBrowsersRunning", ErrorCategory.ObjectNotFound, null));
                return (null, null);
            }

            // Check if Browser parameter contains a PowerBrowserInstance object
            // Handle PowerShell PSObject wrapping
            var actualBrowser = Browser;
            if (Browser is PSObject psObj && psObj.BaseObject is PowerBrowserInstance)
            {
                actualBrowser = psObj.BaseObject;
            }
            
            if (actualBrowser is PowerBrowserInstance powerBrowser)
            {
                return (powerBrowser.Browser, powerBrowser.Name);
            }

            // Check if Browser parameter is a string (browser name)
            var browserName = Browser?.ToString() ?? BrowserName;
            
            if (string.IsNullOrEmpty(browserName))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Browser name must be specified either through -Browser or -BrowserName parameter, or by piping a PowerBrowserInstance object."),
                    "BrowserNameRequired", ErrorCategory.InvalidArgument, null));
                return (null, null);
            }

            if (!browserInstances.ContainsKey(browserName))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Browser '{browserName}' is not running. Use Start-Browser first."),
                    "BrowserNotRunning", ErrorCategory.ObjectNotFound, browserName));
                return (null, null);
            }

            return (browserInstances[browserName], browserName);
        }

        private string GeneratePageName(Dictionary<string, IPage> existingPages, string browserName)
        {
            // If user provided a custom name, use it
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            // Generate automatic name (Page1, Page2, etc.)
            var browserPagePrefix = $"{browserName}_";
            var pagePattern = new Regex($@"^{Regex.Escape(browserPagePrefix)}Page(\d+)$", RegexOptions.Compiled);
            
            var existingPageNumbers = existingPages.Keys
                .Select(key => pagePattern.Match(key))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .ToList();

            var nextNumber = existingPageNumbers.Count > 0 ? existingPageNumbers.Max() + 1 : 1;
            return $"Page{nextNumber}";
        }

        private string GetPageTitle(IPage page)
        {
            try
            {
                var titleTask = page.GetTitleAsync();
                return titleTask.GetAwaiter().GetResult();
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}