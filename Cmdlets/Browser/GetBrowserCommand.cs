using System;
using System.Linq;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Helpers;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsCommon.Get, "Browser")]
    [OutputType(typeof(PowerBrowserInstance))]
    public class GetBrowserCommand : BrowserCmdletBase
    {
        protected override void ProcessRecord()
        {
            try
            {
                WriteVerbose("Retrieving running browsers from session state.");

                var runningBrowsers = SessionStateHelper.GetRunningBrowsers(this.SessionState);

                if (runningBrowsers.Count == 0)
                {
                    WriteWarning("No running browsers found.");
                    return;
                }

                foreach (var browserInstance in runningBrowsers.Values)
                {
                    WriteObject(browserInstance);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}
