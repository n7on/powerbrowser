using System;
using System.Management.Automation;
using PowerBrowser.Transport;

namespace PowerBrowser.Commands.BrowserPage
{
    [Cmdlet(VerbsCommon.Get, "BrowserPage")]
    [OutputType(typeof(PBrowserPage))]
    public class GetBrowserPageCommand : PSCmdlet
    {

        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public PBrowser Browser { get; set; }
        protected override void ProcessRecord()
        {
            try
            {
                var pageService = ServiceFactory.CreateBrowserPageService(SessionState);
                var pages = Browser != null ? pageService.GetBrowserPagesByBrowser(Browser) : pageService.GetBrowserPages();

                if (pages.Count == 0)
                {
                    WriteWarning("No browser pages found.");
                    return;
                }

                foreach (var page in pages)
                {
                    WriteObject(page);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetBrowserPageFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}