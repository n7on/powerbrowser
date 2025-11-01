using System;
using PuppeteerSharp;
using PowerBrowser.Transport;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;

namespace PowerBrowser.Services
{

    public class BrowserPageService : IBrowserPageService
    {
        private const string RunningBrowserPagesKey = "RunningBrowserPages";

        private readonly SessionStateService<PBrowserPage> _sessionStateService;

        public BrowserPageService(SessionState sessionState)
        {
            _sessionStateService = new SessionStateService<PBrowserPage>(sessionState, RunningBrowserPagesKey);
        }

        public List<PBrowserPage> GetBrowserPagesByBrowser(PBrowser pBrowser)
        {
            return _sessionStateService.GetAll()
                .Where(kv => kv.Value.Browser == pBrowser)
                .Select(kv => kv.Value)
                .ToList();
        }
        public List<PBrowserPage> GetBrowserPages()
        {
            return _sessionStateService.GetAll().Select(kv => kv.Value).ToList();
        }

        public PBrowserPage CreateBrowserPage(PBrowser pBrowser, string name, int width, int height, string url, bool waitForLoad)
        {
            var pages = pBrowser.Browser.PagesAsync().GetAwaiter().GetResult();
            string pageName = string.IsNullOrEmpty(name) ? $"Page{pages.Length + 1}" : name;

            var page = pBrowser.Browser.NewPageAsync().GetAwaiter().GetResult();

            // Set viewport size
            page.SetViewportAsync(new ViewPortOptions
            {
                Width = width,
                Height = height
            }).GetAwaiter().GetResult();

            // Navigate to URL if specified
            if (!string.IsNullOrEmpty(url) && url != "about:blank")
            {
                if (waitForLoad)
                {
                    page.GoToAsync(url, new NavigationOptions
                    {
                        WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded }
                    }).GetAwaiter().GetResult();
                }
                else
                {
                    page.GoToAsync(url).GetAwaiter().GetResult();
                }
            }

            var browserPage = new PBrowserPage(
                pBrowser,
                page,
                pageName,
                width,
                height
            );

            _sessionStateService.Save(browserPage.PageId, browserPage);
            return browserPage;
        }
        public void RemoveBrowserPage(PBrowserPage browserPage)
        {
            browserPage.Page.CloseAsync().GetAwaiter().GetResult();
            _sessionStateService.Remove(browserPage.PageId);
        }
    }
}