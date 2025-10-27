using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "BrowserPage")]
    [OutputType(typeof(PowerBrowserPage))]
    public class GetBrowserPageCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserInstance object from Start-Browser or browser name")]
        public object Browser { get; set; }

        [Parameter(HelpMessage = "Filter by browser name (used when Browser parameter is not provided)")]
        public string BrowserName { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Specific page ID to get details for")]
        public string PageId { get; set; } = string.Empty;

        private readonly Dictionary<string, PowerBrowserPage> _powerBrowserPageObjects;
        private readonly Dictionary<string, IPage> _powerBrowserPages;

        public GetBrowserPageCommand()
        {
            _powerBrowserPageObjects = new Dictionary<string, PowerBrowserPage>();
            _powerBrowserPages = new Dictionary<string, IPage>();
        }

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve browser name from input if provided
                var targetBrowserName = ResolveBrowserName();

                if ((_powerBrowserPageObjects == null || _powerBrowserPageObjects.Count == 0) && 
                    (_powerBrowserPages == null || _powerBrowserPages.Count == 0))
                {
                    WriteWarning("No browser pages are currently open.");
                    WriteInformation("Use 'Start-Browser | New-BrowserPage' to create pages.", new string[] { "PSHOST" });
                    return;
                }

                // Get pages to return, prioritizing PowerBrowserPage objects
                var pagesToReturn = new List<PowerBrowserPage>();

                if (_powerBrowserPageObjects != null)
                {
                    var query = _powerBrowserPageObjects.Values.Where(p => p.Page != null && !p.Page.IsClosed);
                    
                    // Filter by browser name if specified
                    if (!string.IsNullOrEmpty(targetBrowserName))
                    {
                        query = query.Where(p => p.BrowserName.Equals(targetBrowserName, StringComparison.OrdinalIgnoreCase));
                    }

                    // Filter by PageId if specified
                    if (!string.IsNullOrEmpty(PageId))
                    {
                        query = query.Where(p => p.PageId == PageId);
                    }

                    pagesToReturn.AddRange(query);
                }

                if (pagesToReturn.Count == 0)
                {
                    WriteWarning("No active browser pages found matching the criteria.");
                    return;
                }

                // Return PowerBrowserPage objects for pipeline chaining
                foreach (var powerPage in pagesToReturn)
                {
                    WriteObject(powerPage);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetBrowserPageFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private string ResolveBrowserName()
        {
            // Handle PowerShell PSObject wrapping
            var actualBrowser = Browser;
            if (Browser is PSObject psObj && psObj.BaseObject is PowerBrowserInstance)
            {
                actualBrowser = psObj.BaseObject;
            }

            // Check if Browser parameter contains a PowerBrowserInstance object
            if (actualBrowser is PowerBrowserInstance powerBrowser)
            {
                return powerBrowser.Name;
            }

            // Check if Browser parameter is a string (browser name)
            return Browser?.ToString() ?? BrowserName;
        }

        private string ExtractBrowserName(string pageId)
        {
            var parts = pageId.Split('_');
            return parts.Length > 0 ? parts[0] : "Unknown";
        }

        private string ExtractPageName(string pageId)
        {
            var parts = pageId.Split('_');
            return parts.Length > 1 ? string.Join("_", parts.Skip(1)) : "Unknown";
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