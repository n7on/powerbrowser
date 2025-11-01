using System;
using System.Management.Automation;
using PowerBrowser.Transport;
using PowerBrowser.Services;
using PowerBrowser.Common;
using PowerBrowser.Completers;
public abstract class BrowserBaseCommand : PSCmdlet
{
    [Parameter(
        Position = 0,
        Mandatory = false,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    public PBrowser Browser { get; set; }

    [Parameter(
        HelpMessage = "Name of the browser to stop (used when Browser parameter is not provided)",
        Mandatory = false)]
    [ArgumentCompleter(typeof(InstalledBrowserCompleter))]
    public string BrowserType { get; set; }
    protected IBrowserService BrowserService => ServiceFactory.CreateBrowserService(SessionState);
    protected PBrowser ResolveBrowserOrThrow(PBrowser browser, string browserType)
    {
        if (browser == null && string.IsNullOrEmpty(browserType))
        {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentException("Either Browser or BrowserType parameter must be provided."),
                "InvalidParameters",
                ErrorCategory.InvalidArgument,
                null));
        }

        if (browser == null)
        {
            BrowserTypeValidator.Validate(browserType);
            browser = BrowserService.GetPBrowser(browserType.ToSupportedPBrowser());
        }

        return browser;
    }
}
