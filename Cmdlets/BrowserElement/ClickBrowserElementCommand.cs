using System;
using System.Collections.Generic;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets.BrowserElement
{
    [Cmdlet(VerbsLifecycle.Invoke, "BrowserElementClick")]
    [OutputType(typeof(PowerBrowserElement))]
    public class ClickBrowserElementCommand : BrowserElementBase
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserElement object from Find-BrowserElement or element ID")]
        public PowerBrowserElement Element { get; set; }

        [Parameter(
            HelpMessage = "Element ID (used when Element parameter is not provided)")]
        public string ElementId { get; set; } = string.Empty;

        [Parameter(
            HelpMessage = "Wait for navigation after click (useful for links and form submissions)")]
        public SwitchParameter WaitForNavigation { get; set; }

        [Parameter(
            HelpMessage = "Timeout in milliseconds for navigation wait (default: 5000)")]
        public int NavigationTimeout { get; set; } = 5000;

        [Parameter(
            HelpMessage = "Click type: Left (default), Right, or Middle")]
        public ClickType ClickType { get; set; } = ClickType.Left;

        [Parameter(
            HelpMessage = "Number of clicks (default: 1, use 2 for double-click)")]
        public int ClickCount { get; set; } = 1;

        [Parameter(
            HelpMessage = "Delay in milliseconds between clicks for multi-click (default: 0)")]
        public int ClickDelay { get; set; } = 0;

        protected override void ProcessRecord()
        {
            try
            {

                WriteVerbose($"Clicking element '{Element}' on page '{Element.PageName}'...");

                PerformClickSync(
                    Element,
                    ClickDelay,
                    NavigationTimeout,
                    WaitForNavigation,
                    ClickType,
                    ClickCount
                );

                WriteVerbose($"Successfully clicked element '{Element.TagName}' on page '{Element.PageName}'");

                // Return the same element for continued chaining
                WriteObject(Element);
            }
            catch (PowerBrowserException ex)
            {
                // Handle custom PowerBrowser exceptions with their built-in error information
                WriteError(new ErrorRecord(ex, ex.ErrorId, ex.Category, null));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ClickBrowserElementFailed", ErrorCategory.OperationStopped, null));
            }
        }

  
    }


}