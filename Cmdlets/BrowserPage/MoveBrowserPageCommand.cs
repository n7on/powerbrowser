using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets.BrowserPage
{
    [Cmdlet(VerbsCommon.Move, "BrowserPage")]
    [OutputType(typeof(PowerBrowserPage))]
    public class MoveBrowserPageCommand : BrowserPageCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "ByObject",
            HelpMessage = "PowerBrowserPage object from New-BrowserPage")]
        public PowerBrowserPage Page { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "ByName",
            HelpMessage = "Page name to navigate")]
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
        public new int Timeout { get; set; } = 30000;

        [Parameter(
            HelpMessage = "What to wait for during navigation")]
        public new WaitUntilNavigation[] WaitUntil { get; set; } = new[] { WaitUntilNavigation.Load };

        [Parameter(
            HelpMessage = "Referer header to send with the navigation request")]
        public new string Referer { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
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

                WriteVerbose($"Navigating page '{PageName}' to: {Url}");

                // Perform navigation
                NavigateToUrl(Page, Url, WaitForLoad);

                // URL is automatically updated by the page object itself

                WriteVerbose($"Navigation completed successfully");

                // Return the updated page object
                WriteObject(Page);
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
    }
}