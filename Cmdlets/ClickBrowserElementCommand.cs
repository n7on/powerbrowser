using System;
using System.Collections.Generic;
using System.Management.Automation;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Invoke, "BrowserElementClick")]
    [OutputType(typeof(PowerBrowserElement))]
    public class ClickBrowserElementCommand : BrowserCmdletBase
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
                // Resolve element instance from various input sources
                var powerElement = ResolveElementInstance();
                
                if (powerElement == null)
                {
                    return; // Error already written in ResolveElementInstance
                }

                WriteVerbose($"🖱️ Clicking element '{powerElement}' on page '{powerElement.PageName}'...");

                PerformClickSync(powerElement);

                WriteVerbose($"✅ Successfully clicked element '{powerElement.TagName}' on page '{powerElement.PageName}'");

                // Return the same element for continued chaining
                WriteObject(powerElement);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ClickBrowserElementFailed", ErrorCategory.OperationStopped, null));
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

        private void PerformClickSync(PowerBrowserElement powerElement)
        {
            try
            {
                var clickOptions = new PuppeteerSharp.Input.ClickOptions
                {
                    Button = ConvertClickType(ClickType),
                    Delay = ClickDelay
                };

                if (WaitForNavigation.IsPresent && powerElement.Page != null)
                {
                    // Click and wait for navigation
                    var clickTask = powerElement.Element.ClickAsync(clickOptions);
                    var navigationTask = powerElement.Page.WaitForNavigationAsync(new NavigationOptions
                    {
                        Timeout = NavigationTimeout
                    });

                    // Execute click
                    clickTask.GetAwaiter().GetResult();
                    
                    // Wait for navigation to complete
                    navigationTask.GetAwaiter().GetResult();
                }
                else
                {
                    // Simple click without waiting for navigation
                    if (ClickCount > 1)
                    {
                        // Handle multiple clicks manually
                        for (int i = 0; i < ClickCount; i++)
                        {
                            powerElement.Element.ClickAsync(clickOptions).GetAwaiter().GetResult();
                            if (i < ClickCount - 1 && ClickDelay > 0)
                            {
                                System.Threading.Thread.Sleep(ClickDelay);
                            }
                        }
                    }
                    else
                    {
                        powerElement.Element.ClickAsync(clickOptions).GetAwaiter().GetResult();
                    }
                }
            }
            catch (TimeoutException)
            {
                WriteWarning($"Navigation timeout after {NavigationTimeout}ms - element was clicked but navigation didn't complete");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ClickOperationFailed", ErrorCategory.OperationStopped, powerElement.ElementId));
            }
        }

        private PuppeteerSharp.Input.MouseButton ConvertClickType(ClickType clickType)
        {
            return clickType switch
            {
                ClickType.Left => PuppeteerSharp.Input.MouseButton.Left,
                ClickType.Right => PuppeteerSharp.Input.MouseButton.Right,
                ClickType.Middle => PuppeteerSharp.Input.MouseButton.Middle,
                _ => PuppeteerSharp.Input.MouseButton.Left
            };
        }
    }

    public enum ClickType
    {
        Left,
        Right, 
        Middle
    }
}