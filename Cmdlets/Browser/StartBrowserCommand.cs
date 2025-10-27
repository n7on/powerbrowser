using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Helpers;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsLifecycle.Start, "Browser")]
    [OutputType(typeof(PowerBrowserInstance))]
    public class StartBrowserCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "Name of the browser to start")]
        [ArgumentCompleter(typeof(BrowserNameCompleter))]
        public string Name { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Run browser in headless mode (no GUI)")]
        public SwitchParameter Headless { get; set; }

        [Parameter(HelpMessage = "Additional arguments to pass to the browser")]
        public string[] Arguments { get; set; }

        [Parameter(HelpMessage = "Window width (default: 1280)")]
        public int Width { get; set; } = 1280;

        [Parameter(HelpMessage = "Window height (default: 720)")]
        public int Height { get; set; } = 720;

        public static IReadOnlyList<IBrowser> RunningBrowsers => _runningBrowsers;

        private static readonly List<IBrowser> _runningBrowsers = new List<IBrowser>();

        protected override void ProcessRecord()
        {
            try
            {
                WriteVerbose("Starting a new browser instance.");

                var browser = LaunchBrowser();
                var browserName = Guid.NewGuid().ToString(); // Generate a unique name for the browser instance

                var powerBrowserInstance = new PowerBrowserInstance(
                    browserName,
                    browser,
                    false, // Default value for Headless
                    "Unknown" // Replace with actual viewport size if available
                );
                WriteVerbose($"Started browser instance: {browserName}");
                SessionStateHelper.SaveBrowserInstance(powerBrowserInstance, this.SessionState);

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

        private IBrowser LaunchBrowser()
        {
            var namedBrowserPath = GetNamedBrowserPath(Name);
            
            // Determine browser type from name
            var browserType = (SupportedBrowser)Enum.Parse(typeof(SupportedBrowser), Name, true);
            
            var launchOptions = new LaunchOptions
            {
                Headless = Headless.IsPresent,
                DefaultViewport = new ViewPortOptions
                {
                    Width = Width,
                    Height = Height
                },
                Args = BuildBrowserArguments()
            };

            // Set the executable path using BrowserFetcher
            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = namedBrowserPath,
                Browser = browserType
            });

            var installedBrowsers = browserFetcher.GetInstalledBrowsers().ToArray();
            if (installedBrowsers.Length == 0)
            {
                throw new InvalidOperationException($"No browser installations found in {namedBrowserPath}");
            }

            var browserInfo = installedBrowsers[0]; // Use the first (and should be only) installation
            launchOptions.ExecutablePath = browserInfo.GetExecutablePath();

            WriteVerbose($"Launching browser from: {launchOptions.ExecutablePath}");
            WriteVerbose($"Arguments: {string.Join(" ", launchOptions.Args ?? Array.Empty<string>())}");

            return Puppeteer.LaunchAsync(launchOptions).GetAwaiter().GetResult();
        }

        private string[] BuildBrowserArguments()
        {
            var args = new List<string>();

            // Add custom arguments if provided
            if (Arguments != null && Arguments.Length > 0)
            {
                args.AddRange(Arguments);
            }

            // Add some reasonable default arguments
            args.AddRange(new[]
            {
                "--no-first-run",
                "--disable-default-apps",
                "--disable-extensions"
            });

            return args.ToArray();
        }
    }
}