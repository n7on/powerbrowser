using System;
using System.Management.Automation;
using PowerBrowser.Transport;

namespace PowerBrowser.Commands.BrowserPage
{
    [Cmdlet(VerbsCommon.Remove, "BrowserPage")]
    [OutputType(typeof(PBrowserPage))]
    public class RemoveBrowserPageCommand : PSCmdlet

    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public PBrowserPage BrowserPage { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var pageService = ServiceFactory.CreateBrowserPageService(SessionState);
                pageService.RemoveBrowserPage(BrowserPage);

                WriteVerbose($"Removed browser page: {BrowserPage.PageName} (ID: {BrowserPage.PageId})");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RemoveBrowserPageFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}