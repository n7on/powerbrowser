using System;
using System.Management.Automation;
using PowerBrowser.Transport;

namespace PowerBrowser.Commands.BrowserPage
{
    [Cmdlet(VerbsCommon.New, "BrowserPage")]
    [OutputType(typeof(PBrowserPage))]
    public class NewBrowserPageCommand : BrowserBaseCommand
    {

        [Parameter(HelpMessage = "Custom name for the page (if not specified, will be Page1, Page2, etc.)")]
        public string Name { get; set; } = string.Empty;

        [Parameter(HelpMessage = "URL to navigate to when creating the page")]
        public string Url { get; set; } = "about:blank";

        [Parameter(HelpMessage = "Wait for page to load completely before returning")]
        public SwitchParameter WaitForLoad { get; set; }

        [Parameter(HelpMessage = "Page width (default: 1280)")]
        public int Width { get; set; } = 1280;

        [Parameter(HelpMessage = "Page height (default: 720)")]
        public int Height { get; set; } = 720;

        protected override void ProcessRecord()
        {
            try
            {
                var browser = ResolveBrowserOrThrow(Browser, BrowserType);
                
                var pageService = ServiceFactory.CreateBrowserPageService(SessionState);
                var browserPage = pageService.CreateBrowserPage(
                    browser,
                    Name,
                    Width,
                    Height,
                    Url,
                    WaitForLoad.IsPresent
                );
                WriteObject(browserPage);
            }            
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "UninstallBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}