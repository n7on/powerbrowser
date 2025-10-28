using System;
using System.Collections.Generic;
using PuppeteerSharp;

namespace PowerBrowser.Models
{
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
        public string BrowserType => Browser?.BrowserType ?? "Unknown";
        public string ViewportSize => $"{ViewportWidth}x{ViewportHeight}";
        public bool IsClosed => Page?.IsClosed ?? true;

        public string Url
        {
            get
            {
                try
                {
                    return Page?.Url ?? "about:blank";
                }
                catch
                {
                    return "about:blank";
                }
            }
        }

        public string Title
        {
            get
            {
                try
                {
                    return Page?.GetTitleAsync().GetAwaiter().GetResult() ?? "";
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
}