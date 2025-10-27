using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;
using PowerBrowser.Cmdlets.Browser;

namespace PowerBrowser.Cmdlets.BrowserElement
{
    [Cmdlet(VerbsCommon.Find, "BrowserElement")]
    [OutputType(typeof(PowerBrowserElement))]
    public class FindBrowserElementCommand : BrowserElementBase
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserPage object from New-BrowserPage")]
        public PowerBrowserPage Page { get; set; }

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
                WriteVerbose($"Finding elements with selector '{Selector}' on page '{Page.PageName}'...");

                var elements = FindElementsSync(Page.Page, Selector, Timeout, WaitForVisible.IsPresent);

                if (elements == null || elements.Length == 0)
                {
                    WriteWarning($"No elements found matching selector '{Selector}' on page '{Page.PageName}'");
                    return;
                }

                WriteVerbose($"Found {elements.Length} element(s) matching '{Selector}'");

                // Create PowerBrowserElement wrapper objects
                var results = new List<PowerBrowserElement>();
                var elementsToProcess = First.IsPresent ? elements.Take(1) : elements;

                foreach (var elementWithIndex in elementsToProcess.Select((e, i) => new { Element = e, Index = i }))
                {
                    var element = elementWithIndex.Element;
                    var index = elementWithIndex.Index;
                    var elementId = $"{Page.PageName}_Element_{DateTime.Now.Ticks}_{index}";

                    var powerElement = new PowerBrowserElement(
                        elementId,
                        Page.PageName,
                        element,
                        Selector,
                        index,
                        Page.Page
                    );

                    results.Add(powerElement);
                }

                // Return the elements for pipeline chaining
                foreach (var element in results)
                {
                    WriteObject(element);
                }
            }
            catch (PowerBrowserException ex)
            {
                // Handle custom PowerBrowser exceptions with their built-in error information
                WriteError(new ErrorRecord(ex, ex.ErrorId, ex.Category, null));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FindBrowserElementFailed", ErrorCategory.OperationStopped, null));
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