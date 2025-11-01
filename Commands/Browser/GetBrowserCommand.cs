using System;
using System.Management.Automation;
using PowerBrowser.Transport;
using PowerBrowser.Services;

namespace PowerBrowser.Commands.Browser
{
    [Cmdlet(VerbsCommon.Get, "Browser")]
    [OutputType(typeof(PBrowser))]
    public class GetBrowserCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                var browsers = ServiceFactory.CreateBrowserService(SessionState).GetBrowsers();

                if (browsers.Count == 0)
                {
                    WriteWarning("No browsers found.");
                    return;
                }

                foreach (var browser in browsers)
                {
                    WriteObject(browser);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}
