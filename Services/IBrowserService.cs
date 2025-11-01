

using System.Collections.Generic;
using PowerBrowser.Common;
using PowerBrowser.Transport;

public interface IBrowserService
{

    bool IsBrowserTypeInstalled(SupportedPBrowser browserType);
    PBrowser GetPBrowser(SupportedPBrowser browserType);
    List<PBrowser> GetBrowsers();

    bool RemoveBrowser(PBrowser browser);

    void DownloadBrowser(SupportedPBrowser browserType);

    bool StopBrowser(PBrowser browser);

    PBrowser StartBrowser(SupportedPBrowser browserType, bool headless, int width, int height);

}