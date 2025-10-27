using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.Move, "BrowserPage")]
    [OutputType(typeof(PowerBrowserPage))]
    public class MoveBrowserPageCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "ByObject",
            HelpMessage = "PowerBrowserPage object from New-BrowserPage")]
        public object Page { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "ByName",
            HelpMessage = "Page name to navigate")]
        [ArgumentCompleter(typeof(RunningPageCompleter))]
        public string PageName { get; set; } = string.Empty;

        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = "URL to navigate to")]
        public string Url { get; set; } = string.Empty;

        [Parameter(
            HelpMessage = "Wait for page to load completely before returning")]
        public SwitchParameter WaitForLoad { get; set; }

        [Parameter(
            HelpMessage = "Timeout in milliseconds for navigation (default: 30000)")]
        public int Timeout { get; set; } = 30000;

        [Parameter(
            HelpMessage = "What to wait for during navigation")]
        public WaitUntilNavigation[] WaitUntil { get; set; } = new[] { WaitUntilNavigation.Load };

        [Parameter(
            HelpMessage = "Referer header to send with the navigation request")]
        public string Referer { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve page instance from various input sources
                var (powerPage, actualPageName) = ResolvePageInstance();

                if (powerPage?.Page == null)
                {
                    throw new ResourceNotFoundException($"Page '{actualPageName}' not found or is no longer available.");
                }

                // Validate URL
                if (string.IsNullOrWhiteSpace(Url))
                {
                    throw new RequiredParameterException("URL parameter is required for navigation.");
                }

                if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
                {
                    // Try to create as relative URL with http:// prefix
                    if (!Uri.TryCreate($"http://{Url}", UriKind.Absolute, out uri))
                    {
                        throw new RequiredParameterException($"Invalid URL format: '{Url}'. Please provide a valid URL (e.g., 'https://example.com' or 'example.com').");
                    }
                    Url = uri.ToString();
                }

                WriteVerbose($"🧭 Navigating page '{actualPageName}' to: {Url}");

                // Perform navigation
                NavigateToUrl(powerPage);

                // URL is automatically updated by the page object itself

                WriteVerbose($"✅ Navigation completed successfully");

                // Return the updated page object
                WriteObject(powerPage);
            }
            catch (PowerBrowserException ex)
            {
                WriteError(new ErrorRecord(ex, ex.ErrorId, ex.Category, null));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "NavigateBrowserPageFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private (PowerBrowserPage powerPage, string pageName) ResolvePageInstance()
        {
            // Handle PowerShell PSObject wrapping
            var actualPage = Page;
            if (Page is PSObject psObj && psObj.BaseObject is PowerBrowserPage)
            {
                actualPage = psObj.BaseObject;
            }

            // Check if Page parameter contains a PowerBrowserPage object
            if (actualPage is PowerBrowserPage powerPage)
            {
                return (powerPage, powerPage.PageName);
            }

            // Check if Page parameter is a string (page name)
            var pageName = Page?.ToString() ?? PageName;
            
            if (string.IsNullOrEmpty(pageName))
            {
                throw new RequiredParameterException("Page name is required when navigating to a specific page instance.");
            }

            // Get page instances from session store
            var sessionStore = SessionState.PSVariable;
            var powerPageInstances = sessionStore.GetValue("PowerBrowserPageObjects") as Dictionary<string, PowerBrowserPage>;

            if (powerPageInstances == null)
            {
                throw new ResourceUnavailableException("No pages are currently open. Use 'Start-Browser | New-BrowserPage' to create pages.");
            }

            // Try to find page by exact ID first
            if (powerPageInstances.ContainsKey(pageName))
            {
                return (powerPageInstances[pageName], pageName);
            }

            // Try to find page by partial name (e.g., "MyPage" -> "Chrome_MyPage")
            var matchingPage = powerPageInstances.FirstOrDefault(kvp => 
                kvp.Key.EndsWith($"_{pageName}") || kvp.Key == pageName);

            if (matchingPage.Key != null)
            {
                return (matchingPage.Value, matchingPage.Key);
            }

            throw new ResourceNotFoundException($"Page '{pageName}' not found.");
        }

        private void NavigateToUrl(PowerBrowserPage powerPage)
        {
            try
            {
                var navigationOptions = new NavigationOptions
                {
                    Timeout = Timeout
                };

                // Set referer if provided
                if (!string.IsNullOrEmpty(Referer))
                {
                    navigationOptions.Referer = Referer;
                }

                // Configure what to wait for
                if (WaitForLoad.IsPresent || WaitUntil.Length > 0)
                {
                    navigationOptions.WaitUntil = WaitUntil.Length > 0 ? WaitUntil : new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded };
                }

                WriteVerbose($"🔄 Navigating with timeout: {Timeout}ms, WaitUntil: {string.Join(", ", navigationOptions.WaitUntil)}");

                // Perform the navigation
                var navigationTask = powerPage.Page.GoToAsync(Url, navigationOptions);
                var response = navigationTask.GetAwaiter().GetResult();

                if (response == null)
                {
                    WriteWarning($"Navigation to '{Url}' completed but no response received. This may indicate a client-side navigation or redirect.");
                }
                else
                {
                    WriteVerbose($"📄 Navigation response: {response.Status} {response.StatusText}");
                    
                    // Check for navigation errors
                    if ((int)response.Status >= 400)
                    {
                        WriteWarning($"Navigation resulted in HTTP {response.Status} {response.StatusText}");
                    }
                }
            }
            catch (TimeoutException)
            {
                throw new ResourceUnavailableException($"Navigation to '{Url}' timed out after {Timeout}ms. The page may be taking too long to load.");
            }
            catch (NavigationException navEx)
            {
                throw new ResourceUnavailableException($"Navigation to '{Url}' failed: {navEx.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("net::"))
            {
                throw new ResourceUnavailableException($"Network error during navigation to '{Url}': {ex.Message}");
            }
        }
    }
}