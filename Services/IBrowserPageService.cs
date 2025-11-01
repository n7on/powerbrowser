using System;
using System.Collections.Generic;
using PowerBrowser.Transport;

namespace PowerBrowser.Services
{
    public interface IBrowserPageService
    {
        PBrowserPage CreateBrowserPage(PBrowser pBrowser, string name, int width, int height, string url, bool waitForLoad);
        List<PBrowserPage> GetBrowserPages();
        List<PBrowserPage> GetBrowserPagesByBrowser(PBrowser pBrowser);

        void RemoveBrowserPage(PBrowserPage browserPage);
    }

}