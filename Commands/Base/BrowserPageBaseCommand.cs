using System;
using System.Management.Automation;
using PowerBrowser.Transport;
using PowerBrowser.Services;
using PowerBrowser.Common;
using PowerBrowser.Completers;
using PuppeteerSharp;
public abstract class BrowserPageBaseCommand : PSCmdlet
{
    [Parameter(
        Position = 0,
        Mandatory = false,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    public PBrowserPage BrowserPage { get; set; }

    [Parameter(
        Position = 1,
        Mandatory = false)]
    public string PageId { get; set; }
    protected IBrowserPageService BrowserPageService => ServiceFactory.CreateBrowserPageService(SessionState);
    protected PBrowserPage ResolveBrowserPageOrThrow(PBrowserPage browserPage, string pageId)
    {
        if(BrowserPage == null && string.IsNullOrEmpty(PageId))
        {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentException(
                    "Either BrowserPage or PageId parameter must be provided."),
                    "InvalidParameters",
                    ErrorCategory.InvalidArgument,
                    null
                )
            );
        }
        else if(BrowserPage == null)
        {
            BrowserPage = BrowserPageService.GetBrowserPages()
                .Find(page => page.PageId.Equals(PageId, StringComparison.OrdinalIgnoreCase));

            if(BrowserPage == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException(
                        $"No browser page found with PageId: {PageId}"),
                        "BrowserPageNotFound",
                        ErrorCategory.ObjectNotFound,
                        null
                    )
                );
            } 
        }
        return BrowserPage;
    }
}