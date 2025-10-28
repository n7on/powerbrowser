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
    public class GetBrowserCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                // todo: it should always just show the installed browsers. 
                // And the IBrowser object should be the one that is saved in SessionState.
                var runningBrowsers = SessionStateHelper.GetRunningBrowsers(SessionState);

                if (runningBrowsers.Count == 0)
                {
                    WriteWarning("No running browsers found.");
                    return;
                }

                foreach (var browser in runningBrowsers.Values)
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
