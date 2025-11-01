using System;
using System.Management.Automation;
using PowerBrowser.Transport;
using PowerBrowser.Exceptions;
using PowerBrowser.Services;
using PowerBrowser.Completers;
using PowerBrowser.Common;

namespace PowerBrowser.Commands.Browser
{
    [Cmdlet(VerbsLifecycle.Stop, "Browser")]
    [OutputType(typeof(string))]
    public class StopBrowserCommand : BrowserBaseCommand
    {

        [Parameter(HelpMessage = "Force stop without confirmation")]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var browser = ResolveBrowserOrThrow(Browser, BrowserType);
                BrowserService.StopBrowser(browser);

                WriteVerbose($"Browser '{browser.BrowserType}' stopped successfully!");
            }
            catch (PowerBrowserException ex)
            {
                // Handle custom PowerBrowser exceptions with their built-in error information
                WriteError(new ErrorRecord(ex, ex.ErrorId, ex.Category, null));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "StopBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}