using System;
using System.Management.Automation;
using PowerBrowser.Transport;

namespace PowerBrowser.Commands.BrowserPage
{
    [Cmdlet(VerbsCommon.Remove, "BrowserPage")]
    [OutputType(typeof(PBrowserPage))]
    public class RemoveBrowserPageCommand : BrowserPageBaseCommand
    {
        protected override void ProcessRecord()
        {
            try
            {
                BrowserPage = ResolveBrowserPageOrThrow(BrowserPage, PageId);
                BrowserPageService.RemoveBrowserPage(BrowserPage);

                WriteVerbose($"Removed browser page: {BrowserPage.PageName} (ID: {BrowserPage.PageId})");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RemoveBrowserPageFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}