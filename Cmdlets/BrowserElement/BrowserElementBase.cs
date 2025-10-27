using System.Management.Automation;
using PowerBrowser.Models;
using System;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace PowerBrowser.Cmdlets.BrowserElement
{
    public abstract class BrowserElementBase : PSCmdlet
    {

        protected IElementHandle[] FindElementsSync(IPage page, string selector, int timeout, bool waitForVisible)
        {
            try
            {
                if (waitForVisible)
                {
                    // Wait for element to be visible
                    var waitTask = page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
                    {
                        Timeout = timeout,
                        Visible = true
                    });
                    waitTask.GetAwaiter().GetResult();
                }
                else if (timeout > 0)
                {
                    // Wait for element to appear in DOM
                    var waitTask = page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
                    {
                        Timeout = timeout
                    });
                    waitTask.GetAwaiter().GetResult();
                }

                // Find all matching elements
                var elements = page.QuerySelectorAllAsync(selector).GetAwaiter().GetResult();
                return elements;
            }
            catch (TimeoutException)
            {
                WriteWarning($"Timeout waiting for selector '{selector}' to appear within {timeout}ms");
                return new IElementHandle[0];
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ElementSearchFailed", ErrorCategory.OperationStopped, selector));
                return null;
            }
        }
        protected void PerformClickSync(
            PowerBrowserElement powerElement,
            int ClickDelay = 0,
            int NavigationTimeout = 5000,
            SwitchParameter WaitForNavigation = default,
            ClickType ClickType = ClickType.Left,
            int ClickCount = 1
        )
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

        private MouseButton ConvertClickType(ClickType clickType)
        {
            return clickType switch
            {
                ClickType.Left      => MouseButton.Left,
                ClickType.Right     => MouseButton.Right,
                ClickType.Middle    => MouseButton.Middle,
                _                   => MouseButton.Left
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