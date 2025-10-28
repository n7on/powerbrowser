using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using PowerBrowser.Helpers;
using PuppeteerSharp;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsLifecycle.Uninstall, "Browser")]
    [OutputType(typeof(string))]
    public class UninstallBrowserCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "Name of the browser installation to uninstall")]
        public SupportedBrowser BrowserType { get; set; } = SupportedBrowser.Chrome;

        [Parameter(HelpMessage = "Remove without confirmation")]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                BrowserHelper.RemoveBrowser(BrowserType);
                WriteVerbose($"Removed browser: {BrowserType}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "UninstallBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}
