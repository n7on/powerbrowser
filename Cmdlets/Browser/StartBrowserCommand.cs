using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Helpers;
using PowerBrowser.Completers;
using System.Collections;
using System.Management.Automation.Language;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsLifecycle.Start, "Browser")]
    [OutputType(typeof(PowerBrowserInstance))]
    public class StartBrowserCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "Name of the browser to start")]
        [ArgumentCompleter(typeof(InstalledBrowserCompleter))]
        public string BrowserType { get; set; }

        [Parameter(HelpMessage = "Run browser in headless mode (no GUI)")]
        public SwitchParameter Headless { get; set; }

        [Parameter(HelpMessage = "Additional arguments to pass to the browser")]
        public string[] Arguments { get; set; }

        [Parameter(HelpMessage = "Window width (default: 1280)")]
        public int Width { get; set; } = 1280;

        [Parameter(HelpMessage = "Window height (default: 720)")]
        public int Height { get; set; } = 720;



        protected override void ProcessRecord()
        {
            try
            {
                var powerBrowserInstance = BrowserHelper.StartBrowser(
                    BrowserHelper.ParseBrowserType(BrowserType),
                    Headless.IsPresent,
                    Width,
                    Height,
                    SessionState
                );
                WriteObject(powerBrowserInstance);
            }
            catch (PipelineStoppedException)
            {
                // Swallow the exception as it indicates the pipeline was stopped intentionally
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "StartBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }
}