using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "BrowserPage")]
    [OutputType(typeof(string))]
    public class RemoveBrowserPageCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserPage object from Get-BrowserPage or page ID/name")]
        public object Page { get; set; }

        [Parameter(
            HelpMessage = "Page ID or Page Name to close (used when Page parameter is not provided)")]
        public string PageId { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Force close without confirmation")]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve page instance from various input sources
                var (pageInstance, pageId, actualPageName) = ResolvePageInstance();
                
                // No null check needed - if ResolvePageInstance returns, values are guaranteed valid

                var page = pageInstance;

                if (page.IsClosed)
                {
                    WriteWarning($"Page '{actualPageName}' is already closed.");
                    CleanupPageFromSession(pageId);
                    WriteObject($"🧹 Cleaned up closed page '{actualPageName}' from session.");
                    return;
                }

                // Confirm close unless Force is specified
                if (!Force.IsPresent)
                {
                    var pageTitle = GetPageTitle(page);
                    if (!ShouldProcess($"Page '{actualPageName}' ({pageTitle})", "Close"))
                    {
                        WriteObject("Close cancelled.");
                        return;
                    }
                }

                WriteObject($"🗑️ Closing page '{actualPageName}'...");

                // Close the page
                var closeTask = page.CloseAsync();
                closeTask.GetAwaiter().GetResult();

                // Remove from session store
                CleanupPageFromSession(pageId);

                WriteObject($"✅ Page '{actualPageName}' closed successfully!");
            }
            catch (PowerBrowserException ex)
            {
                // Handle custom PowerBrowser exceptions with their built-in error information
                WriteError(new ErrorRecord(ex, ex.ErrorId, ex.Category, null));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RemoveBrowserPageFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private (IPage page, string pageId, string pageName) ResolvePageInstance()
        {
            var sessionStore = SessionState.PSVariable;
            var pageInstances = sessionStore.GetValue("PowerBrowserPages") as Dictionary<string, IPage>;
            var powerPageInstances = sessionStore.GetValue("PowerBrowserPageObjects") as Dictionary<string, PowerBrowserPage>;

            if (pageInstances == null)
            {
                throw new ResourceUnavailableException("No pages are currently open.");
            }

            // Handle PowerShell PSObject wrapping
            var actualPage = Page;
            if (Page is PSObject psObj && psObj.BaseObject is PowerBrowserPage)
            {
                actualPage = psObj.BaseObject;
            }

            // Check if Page parameter contains a PowerBrowserPage object
            if (actualPage is PowerBrowserPage powerPage)
            {
                return (powerPage.Page, powerPage.PageId, powerPage.PageName);
            }

            // Check if Page parameter is a string (page ID/name)
            var pageIdentifier = Page?.ToString() ?? PageId;
            
            if (string.IsNullOrEmpty(pageIdentifier))
            {
                throw new RequiredParameterException("Page identifier is required to remove a specific page.");
            }

            // Try to find the page by exact ID first, then by partial match (page name)
            var matchingPageId = FindMatchingPageId(pageInstances, pageIdentifier);
            if (string.IsNullOrEmpty(matchingPageId))
            {
                throw new ResourceNotFoundException($"Page '{pageIdentifier}' not found.");
            }

            var page = pageInstances[matchingPageId];
            var parts = matchingPageId.Split('_');
            var pageName = parts.Length > 1 ? string.Join("_", parts.Skip(1)) : matchingPageId;

            return (page, matchingPageId, pageName);
        }

        private void CleanupPageFromSession(string pageId)
        {
            var sessionStore = SessionState.PSVariable;
            var pageInstances = sessionStore.GetValue("PowerBrowserPages") as Dictionary<string, IPage>;
            var powerPageInstances = sessionStore.GetValue("PowerBrowserPageObjects") as Dictionary<string, PowerBrowserPage>;

            if (pageInstances != null) pageInstances.Remove(pageId);
            if (powerPageInstances != null) powerPageInstances.Remove(pageId);

            sessionStore.Set("PowerBrowserPages", pageInstances);
            sessionStore.Set("PowerBrowserPageObjects", powerPageInstances);
        }

        private string FindMatchingPageId(Dictionary<string, IPage> pageInstances, string searchTerm)
        {
            // First, try exact match with page ID
            if (pageInstances.ContainsKey(searchTerm))
            {
                return searchTerm;
            }

            // Then try to find by page name (part after browser name)
            var matchingPages = pageInstances.Keys.Where(pageId => 
            {
                var parts = pageId.Split('_');
                if (parts.Length > 1)
                {
                    var pageName = string.Join("_", parts.Skip(1));
                    return pageName.Equals(searchTerm, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }).ToList();

            return matchingPages.FirstOrDefault();
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