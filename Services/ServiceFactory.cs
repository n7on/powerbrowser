using System.Management.Automation;
using PowerBrowser.Services;
using PowerBrowser.Transport;

public static class ServiceFactory
{
    public static IBrowserService CreateBrowserService(SessionState sessionState)
    {
        var browserService = new BrowserService(sessionState);
        return browserService;
    }

    public static IBrowserPageService CreateBrowserPageService(SessionState sessionState)
    {
        var browserPageService = new BrowserPageService(sessionState);
        return browserPageService;
    }
}