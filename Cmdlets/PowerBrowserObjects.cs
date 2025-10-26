using System;
using System.Collections.Generic;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets
{
    /// <summary>
    /// PowerShell-friendly wrapper for IBrowser with additional metadata
    /// </summary>
    public class PowerBrowserInstance
    {
        public string Name { get; set; }
        public IBrowser Browser { get; set; }
        public DateTime StartTime { get; set; }
        public bool Headless { get; set; }
        public string WindowSize { get; set; }
        public List<PowerBrowserPage> Pages { get; set; }

        public PowerBrowserInstance(string name, IBrowser browser, bool headless, string windowSize)
        {
            Name = name;
            Browser = browser;
            StartTime = DateTime.Now;
            Headless = headless;
            WindowSize = windowSize;
            Pages = new List<PowerBrowserPage>();
        }

        // Properties for PowerShell display
        public int ProcessId => Browser?.Process?.Id ?? -1;
        public string WebSocketEndpoint => Browser?.WebSocketEndpoint ?? "Unknown";
        public bool IsConnected => Browser?.IsConnected ?? false;
        public int PageCount => Pages.Count;

        public override string ToString()
        {
            return $"{Name} (PID: {ProcessId}, Pages: {PageCount}, Connected: {IsConnected})";
        }
    }

    /// <summary>
    /// PowerShell-friendly wrapper for IPage with additional metadata
    /// </summary>
    public class PowerBrowserPage
    {
        public string PageId { get; set; }
        public string PageName { get; set; }
        public PowerBrowserInstance Browser { get; set; }
        public IPage Page { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ViewportWidth { get; set; }
        public int ViewportHeight { get; set; }

        public PowerBrowserPage(string pageId, string pageName, PowerBrowserInstance browser, IPage page, int width, int height)
        {
            PageId = pageId;
            PageName = pageName;
            Browser = browser;
            Page = page;
            CreatedTime = DateTime.Now;
            ViewportWidth = width;
            ViewportHeight = height;
        }

        // Properties for PowerShell display
        public string BrowserName => Browser?.Name ?? "Unknown";
        public string Url => Page?.Url ?? "Unknown";
        public bool IsClosed => Page?.IsClosed ?? true;
        public string ViewportSize => $"{ViewportWidth}x{ViewportHeight}";

        public string Title
        {
            get
            {
                try
                {
                    return Page?.GetTitleAsync()?.GetAwaiter().GetResult() ?? "Unknown";
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        public override string ToString()
        {
            return $"{PageName} ({Url}) - {ViewportSize}";
        }
    }

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

        public PowerBrowserElement(string elementId, string pageName, IElementHandle element, string selector, int index, IPage page = null)
        {
            ElementId = elementId;
            PageName = pageName;
            Element = element;
            Selector = selector;
            Index = index;
            FoundTime = DateTime.Now;
            Page = page;
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

        public string ClassName
        {
            get
            {
                try
                {
                    return Element?.EvaluateFunctionAsync<string>("el => el.className").GetAwaiter().GetResult() ?? "";
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