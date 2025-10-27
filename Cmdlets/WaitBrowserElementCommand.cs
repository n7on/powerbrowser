using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Wait, "BrowserElement")]
    [OutputType(typeof(PowerBrowserElement))]
    public class WaitBrowserElementCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserPage object from New-BrowserPage or Get-BrowserPage")]
        public PowerBrowserPage Page { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "CSS selector for the element to wait for.")]
        public string Selector { get; set; } = string.Empty;

        [Parameter(
            Mandatory = false,
            HelpMessage = "Wait condition: Visible, Hidden, Enabled, Disabled, TextContains, AttributeEquals")]
        public string Condition { get; set; } = "Visible";

        [Parameter(
            Mandatory = false,
            HelpMessage = "Text to match for TextContains or value for AttributeEquals")]
        public string Value { get; set; } = string.Empty;

        [Parameter(
            Mandatory = false,
            HelpMessage = "Attribute name for AttributeEquals condition")]
        public string Attribute { get; set; } = string.Empty;

        [Parameter(
            HelpMessage = "Timeout in milliseconds (default: 10000)")]
        public int Timeout { get; set; } = 10000;

        [Parameter(
            HelpMessage = "Polling interval in milliseconds (default: 250)")]
        public int PollingInterval { get; set; } = 250;

        protected override void ProcessRecord()
        {
            try
            {
                var element = WaitForElementAsync(Page, Selector, Condition, Value, Attribute, Timeout, PollingInterval).GetAwaiter().GetResult();
                if (element == null)
                {
                    throw new ResourceUnavailableException($"Element '{Selector}' did not meet condition '{Condition}' within {Timeout}ms.");
                }
                WriteObject(element);
            }
            catch (PowerBrowserException ex)
            {
                WriteError(new ErrorRecord(ex, ex.FullyQualifiedErrorId, ex.Category, null));
            }
            catch (ParameterBindingException ex)
            {
                WriteError(new ErrorRecord(ex, "ParameterArgumentValidationErrorEmptyStringNotAllowed", ErrorCategory.InvalidArgument, null));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "WaitBrowserElementFailed", ErrorCategory.OperationStopped, null));
            }
        }

    private async Task<PowerBrowserElement> WaitForElementAsync(PowerBrowserPage page, string selector, string condition, string value, string attribute, int timeout, int pollingInterval)
        {
            var endTime = DateTime.UtcNow.AddMilliseconds(timeout);
            while (DateTime.UtcNow < endTime)
            {
                var element = await page.Page.QuerySelectorAsync(selector);
                if (element != null)
                {
                    string elementId = Guid.NewGuid().ToString();
                    string pageName = page.PageName;
                    int index = 0;
                    switch (condition.ToLowerInvariant())
                    {
                        case "visible":
                            if (await element.IsIntersectingViewportAsync())
                                return new PowerBrowserElement(elementId, pageName, element, selector, index, page.Page);
                            break;
                        case "hidden":
                            if (!await element.IsIntersectingViewportAsync())
                                return new PowerBrowserElement(elementId, pageName, element, selector, index, page.Page);
                            break;
                        case "enabled":
                            if (!await element.EvaluateFunctionAsync<bool>("el => el.disabled"))
                                return new PowerBrowserElement(elementId, pageName, element, selector, index, page.Page);
                            break;
                        case "disabled":
                            if (await element.EvaluateFunctionAsync<bool>("el => el.disabled"))
                                return new PowerBrowserElement(elementId, pageName, element, selector, index, page.Page);
                            break;
                        case "textcontains":
                            var text = await element.EvaluateFunctionAsync<string>("el => el.textContent");
                            if (!string.IsNullOrEmpty(value) && text != null && text.Contains(value))
                                return new PowerBrowserElement(elementId, pageName, element, selector, index, page.Page);
                            break;
                        case "attributeequals":
                            if (!string.IsNullOrEmpty(attribute))
                            {
                                var attrVal = await element.EvaluateFunctionAsync<string>($"el => el.getAttribute('{attribute}')");
                                if (attrVal == value)
                                    return new PowerBrowserElement(elementId, pageName, element, selector, index, page.Page);
                            }
                            break;
                        default:
                            throw new RequiredParameterException($"Unknown wait condition: '{condition}'.");
                    }
                }
                await Task.Delay(pollingInterval);
            }
            return null;
        }
    }
}
