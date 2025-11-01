using System;
using PuppeteerSharp;
using System.IO;
using System.Linq;
using PowerBrowser.Transport;
using System.Management.Automation;
using System.Collections.Generic;
using PowerBrowser.Common;

namespace PowerBrowser.Services
{

    public class BrowserService : IBrowserService
    {
        private const string RunningBrowsersKey = "RunningBrowsers";

        public bool IsBrowserTypeInstalled(SupportedPBrowser browserType)
        {
            var path = GetBrowserTypeInstallPath(browserType);
            return Directory.Exists(path);
        }
        private readonly SessionStateService<PBrowser> _sessionStateService;

        public BrowserService(SessionState sessionState)
        {
            _sessionStateService = new SessionStateService<PBrowser>(sessionState, RunningBrowsersKey);
        }

        private string GetBrowserTypeInstallPath(SupportedPBrowser supportedBrowser)
        {
            var storagePath = GetBrowserInstallPath();
            return Path.Combine(storagePath, supportedBrowser.ToString());
        }

        private string GetBrowserInstallPath()
        {
            var browserPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PowerBrowser",
                "Browsers"
            );

            return browserPath;
        }

        public static string[] GetInstalledBrowserTypes()
        {
            var storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PowerBrowser", "Browsers"
            );

            if (!Directory.Exists(storagePath))
            {
                return Array.Empty<string>();
            }
            var supportedBrowserNames = Enum.GetNames(typeof(SupportedPBrowser));
            return Directory.GetDirectories(storagePath)
                .Where(dir => supportedBrowserNames.Contains(Path.GetFileName(dir)))
                .Select(dir => Path.GetFileName(dir))
                .ToArray();
        }
        public PBrowser GetPBrowser(SupportedPBrowser browserType)
        {
            return GetBrowsers().FirstOrDefault(b => b.BrowserType == browserType);
        }
        public List<PBrowser> GetBrowsers()
        {
            var storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PowerBrowser", "Browsers"
            );

            if (!Directory.Exists(storagePath))
            {
                return new List<PBrowser>();
            }
            var runningBrowsers = _sessionStateService.GetAll();
            var supportedBrowserNames = Enum.GetNames(typeof(SupportedPBrowser));
            var browsers = Directory.GetDirectories(storagePath)
                .Where(dir => supportedBrowserNames.Contains(Path.GetFileName(dir)))
                .Select(dir => {
                    var key = Path.GetFileName(dir);
                    runningBrowsers.TryGetValue(key, out var browser);
                    return browser ?? new PBrowser(Path.GetFileName(dir).ToSupportedPBrowser(), dir);
                }).ToList();
            
            return browsers;
        }

        public bool RemoveBrowser(PBrowser browser)
        {
            if (Directory.Exists(browser.Path))
            {
                Directory.Delete(browser.Path, true);
                return true;
            }
            return false;
        }


        public void DownloadBrowser(SupportedPBrowser browserType)
        {
            var namedBrowserPath = GetBrowserTypeInstallPath(browserType);
            Directory.CreateDirectory(namedBrowserPath);

            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = namedBrowserPath,
                Browser = (SupportedBrowser)Enum.Parse(typeof(SupportedBrowser), browserType.ToString())
            });

            browserFetcher.DownloadAsync().GetAwaiter().GetResult();
        }

        public bool StopBrowser(PBrowser browser)
        {

            if (!browser.Running)
            {
                return false;
            }

            browser.Browser.CloseAsync().GetAwaiter().GetResult();

            _sessionStateService.Remove(browser.BrowserType.ToString());

            return true;
        }
        public PBrowser StartBrowser(SupportedPBrowser browserType, bool headless, int width, int height)
        {
            var path = GetBrowserTypeInstallPath(browserType);
            var browserTypeName = browserType.ToString();
            var launchOptions = new LaunchOptions
            {
                Headless = headless,
                DefaultViewport = new ViewPortOptions
                {
                    Width = width,
                    Height = height
                }
            };

            // Set the executable path using BrowserFetcher
            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = path,
                Browser = (SupportedBrowser)Enum.Parse(typeof(SupportedBrowser), browserTypeName)
            });

            var installedBrowsers = browserFetcher.GetInstalledBrowsers().ToArray();
            if (installedBrowsers.Length == 0)
            {
                throw new InvalidOperationException($"No browser installations found in {path}");
            }

            var browserInfo = installedBrowsers[0]; // Use the first (and should be only) installation
            launchOptions.ExecutablePath = browserInfo.GetExecutablePath();

            var browser = Puppeteer.LaunchAsync(launchOptions).GetAwaiter().GetResult();

            //default page close
            var defaultPage = browser.PagesAsync().GetAwaiter().GetResult().FirstOrDefault();
            if (defaultPage != null && defaultPage.Url == "about:blank")
                defaultPage.CloseAsync().GetAwaiter().GetResult();

            var pbrowser = new PBrowser(
                browser,
                headless,
                $"{width}x{height}",
                path
            );

            _sessionStateService.Save(browserTypeName, pbrowser);

            return pbrowser;
        }

    }
}