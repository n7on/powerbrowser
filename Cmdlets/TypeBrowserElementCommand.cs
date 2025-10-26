using System;
using System.Collections.Generic;
using System.Management.Automation;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsCommon.Set, "BrowserElementText")]
    [OutputType(typeof(PowerBrowserElement))]
    public class TypeBrowserElementCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserElement object from Find-BrowserElement or element ID")]
        public object Element { get; set; }

        [Parameter(
            HelpMessage = "Element ID (used when Element parameter is not provided)")]
        public string ElementId { get; set; } = string.Empty;

        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = "Text to type into the element")]
        public string Text { get; set; } = string.Empty;

        [Parameter(
            HelpMessage = "Clear existing text before typing")]
        public SwitchParameter Clear { get; set; }

        [Parameter(
            HelpMessage = "Delay in milliseconds between keystrokes (default: 50ms for more human-like typing)")]
        public int TypeDelay { get; set; } = 50;

        [Parameter(
            HelpMessage = "Press Enter after typing the text")]
        public SwitchParameter PressEnter { get; set; }

        [Parameter(
            HelpMessage = "Press Tab after typing the text")]
        public SwitchParameter PressTab { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve element instance from various input sources
                var powerElement = ResolveElementInstance();
                
                if (powerElement == null)
                {
                    return; // Error already written in ResolveElementInstance
                }

                WriteVerbose($"⌨️ Typing '{Text}' into element '{powerElement}' on page '{powerElement.PageName}'...");

                PerformTypingSync(powerElement);

                WriteVerbose($"✅ Successfully typed text into '{powerElement.TagName}' element on page '{powerElement.PageName}'");

                // Return the same element for continued chaining
                WriteObject(powerElement);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "TypeBrowserElementFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private PowerBrowserElement ResolveElementInstance()
        {
            var sessionStore = SessionState.PSVariable;
            var elementInstances = sessionStore.GetValue("PowerBrowserElements") as Dictionary<string, PowerBrowserElement>;

            if (elementInstances == null)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("No browser elements are available. Use Find-BrowserElement first."),
                    "NoElementsAvailable", ErrorCategory.ObjectNotFound, null));
                return null;
            }

            // Handle PowerShell PSObject wrapping
            var actualElement = Element;
            if (Element is PSObject psObj && psObj.BaseObject is PowerBrowserElement)
            {
                actualElement = psObj.BaseObject;
            }

            // Check if Element parameter contains a PowerBrowserElement object
            if (actualElement is PowerBrowserElement powerElement)
            {
                return powerElement;
            }

            // Check if Element parameter is a string (element ID)
            var elementId = Element?.ToString() ?? ElementId;
            
            if (string.IsNullOrEmpty(elementId))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("Element ID must be specified either through -Element or -ElementId parameter, or by piping a PowerBrowserElement object."),
                    "ElementIdRequired", ErrorCategory.InvalidArgument, null));
                return null;
            }

            if (!elementInstances.ContainsKey(elementId))
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Element '{elementId}' not found. Use Find-BrowserElement first."),
                    "ElementNotFound", ErrorCategory.ObjectNotFound, elementId));
                return null;
            }

            return elementInstances[elementId];
        }

        private void PerformTypingSync(PowerBrowserElement powerElement)
        {
            try
            {
                // Focus the element first
                powerElement.Element.FocusAsync().GetAwaiter().GetResult();

                // Clear existing content if requested
                if (Clear.IsPresent)
                {
                    // Select all text and delete it
                    powerElement.Element.ClickAsync(new PuppeteerSharp.Input.ClickOptions()).GetAwaiter().GetResult();
                    // Triple-click to select all, then delete
                    powerElement.Element.ClickAsync(new PuppeteerSharp.Input.ClickOptions()).GetAwaiter().GetResult();
                    powerElement.Element.ClickAsync(new PuppeteerSharp.Input.ClickOptions()).GetAwaiter().GetResult();
                    powerElement.Element.PressAsync("Backspace").GetAwaiter().GetResult();
                }

                // Type the text with specified delay
                powerElement.Element.TypeAsync(Text, new PuppeteerSharp.Input.TypeOptions 
                { 
                    Delay = TypeDelay 
                }).GetAwaiter().GetResult();

                // Press additional keys if requested
                if (PressEnter.IsPresent)
                {
                    powerElement.Element.PressAsync("Enter").GetAwaiter().GetResult();
                }

                if (PressTab.IsPresent)
                {
                    powerElement.Element.PressAsync("Tab").GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "TypingOperationFailed", ErrorCategory.OperationStopped, powerElement.ElementId));
            }
        }
    }
}