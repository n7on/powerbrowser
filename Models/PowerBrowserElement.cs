using System;
using System.Collections.Generic;
using PuppeteerSharp;

namespace PowerBrowser.Models
{
    /// <summary>
    /// PowerShell-friendly wrapper for IElementHandle with additional metadata
    /// </summary>
    public class PowerBrowserElement
    {
        public string ElementId { get; set; }
        public string PageName { get; set; }
        public IElementHandle Element { get; set; }
        public IPage Page { get; set; }
        public string Selector { get; set; }
        public int Index { get; set; }
        public DateTime FoundTime { get; set; }

        // Added a static property and method to manage and retrieve all PowerBrowserElement instances.
        private static readonly Dictionary<string, PowerBrowserElement> _elements = new Dictionary<string, PowerBrowserElement>();
        public static Dictionary<string, PowerBrowserElement> GetAllElements() => _elements;

        public PowerBrowserElement(string elementId, string pageName, IElementHandle element, string selector, int index, IPage page = null)
        {
            ElementId = elementId;
            PageName = pageName;
            Element = element;
            Selector = selector;
            Index = index;
            FoundTime = DateTime.Now;
            Page = page;

            // Add instance to the static dictionary
            _elements[elementId] = this;
        }

        // Properties for PowerShell display
        public string TagName
        {
            get
            {
                try
                {
                    return Element?.EvaluateFunctionAsync<string>("el => el.tagName").GetAwaiter().GetResult()?.ToLower() ?? "unknown";
                }
                catch
                {
                    return "unknown";
                }
            }
        }

        public string InnerText
        {
            get
            {
                try
                {
                    return Element?.EvaluateFunctionAsync<string>("el => el.innerText").GetAwaiter().GetResult() ?? "";
                }
                catch
                {
                    return "";
                }
            }
        }

        public string InnerHTML
        {
            get
            {
                try
                {
                    return Element?.EvaluateFunctionAsync<string>("el => el.innerHTML").GetAwaiter().GetResult() ?? "";
                }
                catch
                {
                    return "";
                }
            }
        }

        public string Id
        {
            get
            {
                try
                {
                    return Element?.EvaluateFunctionAsync<string>("el => el.id").GetAwaiter().GetResult() ?? "";
                }
                catch
                {
                    return "";
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                try
                {
                    return Element?.IsIntersectingViewportAsync().GetAwaiter().GetResult() ?? false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public override string ToString()
        {
            var text = InnerText;
            var displayText = string.IsNullOrEmpty(text) ? "" : $" '{text.Substring(0, Math.Min(30, text.Length))}'";
            if (text.Length > 30) displayText += "...";
            
            return $"{TagName}[{Index}]{displayText} ({Selector})";
        }
    }
}