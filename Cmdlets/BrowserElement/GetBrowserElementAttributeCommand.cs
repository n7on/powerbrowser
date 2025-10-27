using System;
using System.Collections.Generic;
using System.Management.Automation;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets.BrowserElement
{
    [Cmdlet(VerbsCommon.Get, "BrowserElementAttribute")]
    [OutputType(typeof(PSObject))]
    public class GetBrowserElementAttributeCommand : BrowserElementBase
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
            HelpMessage = "Specific attribute name to get (e.g., 'href', 'src', 'class'). If not specified, gets all attributes")]
        public string AttributeName { get; set; } = string.Empty;

        [Parameter(
            HelpMessage = "Get element properties instead of attributes (e.g., 'value', 'checked', 'disabled')")]
        public SwitchParameter Properties { get; set; }

        [Parameter(
            HelpMessage = "Get computed CSS styles for the element")]
        public SwitchParameter ComputedStyle { get; set; }

        [Parameter(
            HelpMessage = "Get element's bounding rectangle (position and size)")]
        public SwitchParameter BoundingBox { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve element instance from various input sources
                var powerElement = ResolveElementInstance();
                
                // No null check needed - if ResolveElementInstance returns, value is guaranteed valid

                WriteVerbose($"📋 Getting attributes/properties for element '{powerElement}' on page '{powerElement.PageName}'...");

                var result = GetElementInfoSync(powerElement);

                WriteVerbose($"Successfully retrieved element information for '{powerElement.TagName}' element");

                // Return the information object
                WriteObject(result);
            }
            catch (PowerBrowserException ex)
            {
                // Handle custom PowerBrowser exceptions with their built-in error information
                WriteError(new ErrorRecord(ex, ex.ErrorId, ex.Category, null));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetBrowserElementAttributeFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private PowerBrowserElement ResolveElementInstance()
        {
            // Updated the ResolveElementInstance method to use PowerBrowserElement.GetAllElements.
            var elementInstances = PowerBrowserElement.GetAllElements();

            if (elementInstances == null)
            {
                throw new ResourceUnavailableException("No elements are currently available.");
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
                throw new RequiredParameterException("Element ID is required to get element attributes.");
            }

            if (!elementInstances.ContainsKey(elementId))
            {
                throw new ResourceNotFoundException($"Element '{elementId}' not found.");
            }

            return elementInstances[elementId];
        }

        private PSObject GetElementInfoSync(PowerBrowserElement powerElement)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("ElementId", powerElement.ElementId));
            result.Properties.Add(new PSNoteProperty("PageName", powerElement.PageName));
            result.Properties.Add(new PSNoteProperty("TagName", powerElement.TagName));
            result.Properties.Add(new PSNoteProperty("Selector", powerElement.Selector));

            try
            {
                if (BoundingBox.IsPresent)
                {
                    var boundingBox = powerElement.Element.BoundingBoxAsync().GetAwaiter().GetResult();
                    if (boundingBox != null)
                    {
                        var boxInfo = new PSObject();
                        boxInfo.Properties.Add(new PSNoteProperty("X", boundingBox.X));
                        boxInfo.Properties.Add(new PSNoteProperty("Y", boundingBox.Y));
                        boxInfo.Properties.Add(new PSNoteProperty("Width", boundingBox.Width));
                        boxInfo.Properties.Add(new PSNoteProperty("Height", boundingBox.Height));
                        result.Properties.Add(new PSNoteProperty("BoundingBox", boxInfo));
                    }
                }

                if (ComputedStyle.IsPresent)
                {
                    var styles = powerElement.Element.EvaluateFunctionAsync<Dictionary<string, object>>(@"
                        el => {
                            const computed = window.getComputedStyle(el);
                            const styles = {};
                            for (let i = 0; i < computed.length; i++) {
                                const prop = computed[i];
                                styles[prop] = computed.getPropertyValue(prop);
                            }
                            return styles;
                        }
                    ").GetAwaiter().GetResult();

                    result.Properties.Add(new PSNoteProperty("ComputedStyle", styles));
                }

                if (Properties.IsPresent)
                {
                    if (!string.IsNullOrEmpty(AttributeName))
                    {
                        // Get specific property
                        var propertyValue = powerElement.Element.EvaluateFunctionAsync<object>($"el => el.{AttributeName}").GetAwaiter().GetResult();
                        result.Properties.Add(new PSNoteProperty("PropertyValue", propertyValue));
                        result.Properties.Add(new PSNoteProperty("PropertyName", AttributeName));
                    }
                    else
                    {
                        // Get common properties
                        var properties = powerElement.Element.EvaluateFunctionAsync<Dictionary<string, object>>(@"
                            el => ({
                                value: el.value,
                                checked: el.checked,
                                disabled: el.disabled,
                                selected: el.selected,
                                readOnly: el.readOnly,
                                hidden: el.hidden,
                                offsetWidth: el.offsetWidth,
                                offsetHeight: el.offsetHeight,
                                scrollTop: el.scrollTop,
                                scrollLeft: el.scrollLeft,
                                type: el.type,
                                id: el.id,
                                name: el.name,
                                className: el.className,
                                placeholder: el.placeholder,
                                title: el.title,
                                alt: el.alt,
                                src: el.src,
                                href: el.href,
                                tagName: el.tagName.toLowerCase()
                            })
                        ").GetAwaiter().GetResult();

                        // Add properties both as individual properties AND as Properties collection for backward compatibility
                        result.Properties.Add(new PSNoteProperty("Properties", properties));
                        
                        // Add individual properties directly to the result object for easier access
                        foreach (var prop in properties)
                        {
                            if (prop.Key != null && prop.Value != null)
                            {
                                // Use PascalCase for property names to follow PowerShell conventions
                                var propertyName = char.ToUpper(prop.Key[0]) + prop.Key.Substring(1);
                                try
                                {
                                    result.Properties.Add(new PSNoteProperty(propertyName, prop.Value));
                                }
                                catch 
                                {
                                    // Skip if property name conflicts exist
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Get attributes
                    if (!string.IsNullOrEmpty(AttributeName))
                    {
                        // Get specific attribute
                        var attributeValue = powerElement.Element.EvaluateFunctionAsync<string>($"el => el.getAttribute('{AttributeName}')").GetAwaiter().GetResult();
                        result.Properties.Add(new PSNoteProperty("AttributeValue", attributeValue));
                        result.Properties.Add(new PSNoteProperty("AttributeName", AttributeName));
                    }
                    else
                    {
                        // Get all attributes
                        var attributes = powerElement.Element.EvaluateFunctionAsync<Dictionary<string, string>>(@"
                            el => {
                                const attrs = {};
                                for (let i = 0; i < el.attributes.length; i++) {
                                    const attr = el.attributes[i];
                                    attrs[attr.name] = attr.value;
                                }
                                return attrs;
                            }
                        ").GetAwaiter().GetResult();

                        result.Properties.Add(new PSNoteProperty("Attributes", attributes));
                    }
                }

                // Always include basic element info
                result.Properties.Add(new PSNoteProperty("InnerText", powerElement.InnerText));
                result.Properties.Add(new PSNoteProperty("InnerHTML", powerElement.InnerHTML));
                result.Properties.Add(new PSNoteProperty("IsVisible", powerElement.IsVisible));
            }
            catch (Exception ex)
            {
                result.Properties.Add(new PSNoteProperty("Error", ex.Message));
            }

            return result;
        }
    }
}