using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets.BrowserPage
{
    [Cmdlet(VerbsCommon.New, "BrowserPage")]
    [OutputType(typeof(PowerBrowserPage))]
    public class NewBrowserPageCommand : BrowserPageCmdletBase
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
                
                // No null check needed - if ResolveBrowserInstance returns, values are guaranteed valid

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
                
                // Create a unique ID for the new page
                var pageId = $"{actualBrowserName}_{Name}";

                // Return the PowerBrowserPage object for pipeline chaining
                WriteObject(new PowerBrowserPage(pageId, Name, new PowerBrowserInstance(actualBrowserName, browser, false, $"{Width}x{Height}"), page, Width, Height));
                
                WriteVerbose($"✅ Page '{Name}' created successfully!");
                WriteVerbose($"📋 Page ID: {pageId}");
                WriteVerbose($"🌐 URL: {page.Url}");
                WriteVerbose($"📏 Viewport: {Width}x{Height}");
            }
            catch (PowerBrowserException ex)
            {
                // Handle custom PowerBrowser exceptions with their built-in error information
                WriteError(new ErrorRecord(ex, ex.ErrorId, ex.Category, null));
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
            var browserInstances = GetBrowserInstances();
            var powerBrowserInstances = GetPowerBrowserInstances();

            if (browserInstances == null)
            {
                throw new ResourceUnavailableException("No browsers are currently running.");
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
                throw new RequiredParameterException("Browser name is required when creating a page for a specific browser instance.");
            }

            if (!browserInstances.ContainsKey(browserName))
            {
                throw new ResourceNotFoundException($"Browser instance '{browserName}' is not currently running.");
            }

            return (browserInstances[browserName], browserName);
        }

        private Dictionary<string, IBrowser> GetBrowserInstances()
        {
            // This method should retrieve the currently running browser instances
            // Implementation depends on the rest of the PowerBrowser module
            throw new NotImplementedException();
        }

        private Dictionary<string, PowerBrowserInstance> GetPowerBrowserInstances()
        {
            // This method should retrieve the PowerBrowserInstance objects
            // Implementation depends on the rest of the PowerBrowser module
            throw new NotImplementedException();
        }
    }
    }