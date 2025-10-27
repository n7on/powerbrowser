using System;
using System.Collections.Generic;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsLifecycle.Stop, "Browser")]
    [OutputType(typeof(string))]
    public class StopBrowserCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "PowerBrowserInstance object from Start-Browser or browser name")]
        public object Browser { get; set; }

        [Parameter(
            HelpMessage = "Name of the browser to stop (used when Browser parameter is not provided)")]
        public string Name { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Force stop without confirmation")]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve browser instance and name from various input sources
                var (browserInstance, actualBrowserName) = ResolveBrowserInstance();
                
                // No null check needed - if ResolveBrowserInstance returns, values are guaranteed valid

                var browser = browserInstance;

                // Check if browser is still connected
                if (!browser.IsConnected)
                {    
                    WriteWarning($"Browser '{actualBrowserName}' appears to be already disconnected.");
                    CleanupBrowserFromSession(actualBrowserName);
                    WriteObject($"🧹 Cleaned up disconnected browser '{actualBrowserName}' from session.");
                    return;
                }

                // Confirm stop unless Force is specified
                if (!Force.IsPresent)
                {
                    if (!ShouldProcess($"Browser '{actualBrowserName}' (PID: {browser.Process?.Id ?? -1})", "Stop"))
                    {
                        WriteObject("Stop cancelled.");
                        return;
                    }
                }

                WriteObject($"⏹️ Stopping browser '{actualBrowserName}'...");

                // Close the browser using PuppeteerSharp
                var closeTask = browser.CloseAsync();
                closeTask.GetAwaiter().GetResult();

                // Remove from session store
                CleanupBrowserFromSession(actualBrowserName);

                WriteObject($"✅ Browser '{actualBrowserName}' stopped successfully!");
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

        private (IBrowser browser, string browserName) ResolveBrowserInstance()
        {
            var browserInstances = PowerBrowserInstance.Instances;

            if (browserInstances == null || browserInstances.Count == 0)
            {
                throw new ResourceUnavailableException("No browsers are currently running.");
            }

            // Handle PowerShell PSObject wrapping
            var actualBrowser = Browser;
            if (Browser is PSObject psObj && psObj.BaseObject is PowerBrowserInstance)
            {
                actualBrowser = psObj.BaseObject;
            }

            // Check if Browser parameter contains a PowerBrowserInstance object
            if (actualBrowser is PowerBrowserInstance powerBrowser)
            {
                return (powerBrowser.Browser, powerBrowser.Name);
            }

            // Check if Browser parameter is a string (browser name)
            var browserName = Browser?.ToString() ?? Name;
            
            if (string.IsNullOrEmpty(browserName))
            {
                throw new RequiredParameterException("Browser name is required when stopping a specific browser instance.");
            }

            if (!browserInstances.ContainsKey(browserName))
            {
                throw new ResourceNotFoundException($"Browser instance '{browserName}' is not currently running.");
            }

            // Adjusted the return statement to extract the IBrowser instance from PowerBrowserInstance.
            return (browserInstances[browserName].Browser, browserName);
        }

        private void CleanupBrowserFromSession(string browserName)
        {
            var browserInstances = PowerBrowserInstance.Instances;
            
            if (browserInstances != null) browserInstances.Remove(browserName);
        }
    }
}