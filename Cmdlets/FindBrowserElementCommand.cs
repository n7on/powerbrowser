using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.Find, "BrowserElement")]
    [OutputType(typeof(PowerBrowserElement))]
    public class FindBrowserElementCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserPage object from New-BrowserPage or page name")]
        public object Page { get; set; }

        [Parameter(
            HelpMessage = "Page name (used when Page parameter is not provided)")]
        [ArgumentCompleter(typeof(RunningPageCompleter))]
        public string PageName { get; set; } = string.Empty;

        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = "CSS selector to find elements (e.g., 'h1', '.class', '#id', '[attribute]')")]
        public string Selector { get; set; } = string.Empty;

        [Parameter(
            HelpMessage = "Return only the first matching element instead of all matches")]
        public SwitchParameter First { get; set; }

        [Parameter(
            HelpMessage = "Timeout in milliseconds to wait for element to appear (default: 5000)")]
        public int Timeout { get; set; } = 5000;

        [Parameter(
            HelpMessage = "Wait for element to be visible (not just present in DOM)")]
        public SwitchParameter WaitForVisible { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve page instance from various input sources
                var (pageInstance, actualPageName) = ResolvePageInstance();
                
                if (pageInstance == null)
                {
                    return; // Error already written in ResolvePageInstance
                }

                WriteVerbose($"🔍 Finding elements with selector '{Selector}' on page '{actualPageName}'...");

                var elements = FindElementsSync(pageInstance);

                if (elements == null || elements.Length == 0)
                {
                    WriteWarning($"No elements found matching selector '{Selector}' on page '{actualPageName}'");
                    return;
                }

                WriteVerbose($"✅ Found {elements.Length} element(s) matching '{Selector}'");

                // Create PowerBrowserElement wrapper objects
                var sessionStore = SessionState.PSVariable;
                var elementInstances = sessionStore.GetValue("PowerBrowserElements") as Dictionary<string, PowerBrowserElement>
                    ?? new Dictionary<string, PowerBrowserElement>();

                var results = new List<PowerBrowserElement>();
                var elementsToProcess = First.IsPresent ? elements.Take(1) : elements;

                foreach (var (element, index) in elementsToProcess.Select((e, i) => (e, i)))
                {
                    var elementId = $"{actualPageName}_Element_{DateTime.Now.Ticks}_{index}";
                    
                    var powerElement = new PowerBrowserElement(
                        elementId,
                        actualPageName,
                        element,
                        Selector,
                        index,
                        pageInstance
                    );

                    elementInstances[elementId] = powerElement;
                    results.Add(powerElement);
                }

                sessionStore.Set("PowerBrowserElements", elementInstances);

                // Return the elements for pipeline chaining
                foreach (var element in results)
                {
                    WriteObject(element);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FindBrowserElementFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private (IPage page, string pageName) ResolvePageInstance()
        {
            var sessionStore = SessionState.PSVariable;
            var pageInstances = sessionStore.GetValue("PowerBrowserPages") as Dictionary<string, IPage>;
            var powerPageInstances = sessionStore.GetValue("PowerBrowserPageObjects") as Dictionary<string, PowerBrowserPage>;

            if (pageInstances == null)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("No browser pages are available. Use New-BrowserPage first."),
                    "NoPagesAvailable", ErrorCategory.ObjectNotFound, null));
                return (null, null);
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
                return (powerPage.Page, powerPage.PageName);
            }

            // Check if Page parameter is a string (page name/ID)
            var pageName = Page?.ToString() ?? PageName;
            
            if (string.IsNullOrEmpty(pageName))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Page name must be specified either through -Page or -PageName parameter, or by piping a PowerBrowserPage object."),
                    "PageNameRequired", ErrorCategory.InvalidArgument, null));
                return (null, null);
            }

            // Try to find page by exact ID first
            if (pageInstances.ContainsKey(pageName))
            {
                return (pageInstances[pageName], pageName);
            }

            // Try to find page by partial name (e.g., "MyPage" -> "Chrome_MyPage")
            var matchingPage = pageInstances.FirstOrDefault(kvp => 
                kvp.Key.EndsWith($"_{pageName}") || kvp.Key == pageName);

            if (matchingPage.Key != null)
            {
                return (matchingPage.Value, matchingPage.Key);
            }

            WriteError(new ErrorRecord(
                new InvalidOperationException($"Page '{pageName}' not found. Use Get-BrowserPage to see available pages."),
                "PageNotFound", ErrorCategory.ObjectNotFound, pageName));
            return (null, null);
        }

        private IElementHandle[] FindElementsSync(IPage page)
        {
            try
            {
                if (WaitForVisible.IsPresent)
                {
                    // Wait for element to be visible
                    var waitTask = page.WaitForSelectorAsync(Selector, new WaitForSelectorOptions
                    {
                        Timeout = Timeout,
                        Visible = true
                    });
                    waitTask.GetAwaiter().GetResult();
                }
                else if (Timeout > 0)
                {
                    // Wait for element to appear in DOM
                    var waitTask = page.WaitForSelectorAsync(Selector, new WaitForSelectorOptions
                    {
                        Timeout = Timeout
                    });
                    waitTask.GetAwaiter().GetResult();
                }

                // Find all matching elements
                var elements = page.QuerySelectorAllAsync(Selector).GetAwaiter().GetResult();
                return elements;
            }
            catch (TimeoutException)
            {
                WriteWarning($"Timeout waiting for selector '{Selector}' to appear within {Timeout}ms");
                return new IElementHandle[0];
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ElementSearchFailed", ErrorCategory.OperationStopped, Selector));
                return null;
            }
        }
    }

    /// <summary>
    /// Argument completer for running browser page names
    /// </summary>
    public class RunningPageCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName,
            string wordToComplete, System.Management.Automation.Language.CommandAst commandAst,
            System.Collections.IDictionary fakeBoundParameters)
        {
            var results = new List<CompletionResult>();

            try
            {
                // This is a simplified completer - in a real session, we'd access the session state
                // For now, we'll return common page patterns
                var commonPages = new[] { "Page1", "Page2", "MainPage", "TestPage", "HomePage" };

                foreach (var pageName in commonPages)
                {
                    if (pageName.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new CompletionResult(pageName, pageName, CompletionResultType.ParameterValue, pageName));
                    }
                }
            }
            catch
            {
                // Ignore errors during completion
            }

            return results;
        }
    }
}