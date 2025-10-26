using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PuppeteerSharp;
using PowerBrowser.Models;

namespace PowerBrowser.Cmdlets
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

        protected override void ProcessRecord()
        {
            try
            {
                var namedBrowserPath = GetNamedBrowserPath(Name);
                
                if (!Directory.Exists(namedBrowserPath))
                {
                    WriteError(new ErrorRecord(
                        new DirectoryNotFoundException($"Browser '{Name}' not found. Use Get-Browser to see installed browsers."),
                        "BrowserNotFound", ErrorCategory.ObjectNotFound, Name));
                    return;
                }

                WriteVerbose($"🚀 Starting {Name} using PuppeteerSharp...");

                var browser = LaunchBrowserAsync().GetAwaiter().GetResult();
                
                // Create PowerBrowser wrapper object
                var powerBrowser = new PowerBrowserInstance(
                    Name, 
                    browser, 
                    Headless.IsPresent, 
                    $"{Width}x{Height}"
                );
                
                // Store browser instance for backward compatibility and cross-command access
                var sessionStore = SessionState.PSVariable;
                var browserInstances = sessionStore.GetValue("PowerBrowserInstances") as Dictionary<string, IBrowser> 
                    ?? new Dictionary<string, IBrowser>();
                var powerBrowserInstances = sessionStore.GetValue("PowerBrowserObjects") as Dictionary<string, PowerBrowserInstance>
                    ?? new Dictionary<string, PowerBrowserInstance>();
                
                browserInstances[Name] = browser;
                powerBrowserInstances[Name] = powerBrowser;
                sessionStore.Set("PowerBrowserInstances", browserInstances);
                sessionStore.Set("PowerBrowserObjects", powerBrowserInstances);

                WriteVerbose($"✅ Browser '{Name}' started successfully!");
                WriteVerbose($"📊 Process ID: {powerBrowser.ProcessId}");
                WriteVerbose($"🔗 WebSocket: {powerBrowser.WebSocketEndpoint}");
                
                // Return the PowerBrowser object for pipeline chaining
                WriteObject(powerBrowser);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "StartBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }

        private async Task<IBrowser> LaunchBrowserAsync()
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

            return await Puppeteer.LaunchAsync(launchOptions);
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