using System;
using PuppeteerSharp;
using System.IO;
using System.Linq;
using PowerBrowser.Models;
using System.Management.Automation;
using System.Collections.Generic;

namespace PowerBrowser.Helpers
{

    public static class BrowserHelper
    {
        public static SupportedBrowser ParseBrowserType(string browserTypeStr)
        {
            if (Enum.TryParse<SupportedBrowser>(browserTypeStr, true, out var browserType))
            {
                return browserType;
            }
            throw new ArgumentException($"Invalid browser type: {browserTypeStr}");
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

            var supportedBrowserNames = Enum.GetNames(typeof(SupportedBrowser));
            return Directory.GetDirectories(storagePath)
                .Where(dir => supportedBrowserNames.Contains(Path.GetFileName(dir)))
                .Select(dir => Path.GetFileName(dir))
                .ToArray();
        }
        public static List<PowerBrowserInstance> GetBrowsers(SessionState sessionState)
        {
            var storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PowerBrowser", "Browsers"
            );

            if (!Directory.Exists(storagePath))
            {
                return new List<PowerBrowserInstance>();
            }
            var browsers = SessionStateHelper.GetRunningBrowsers(sessionState);
            var supportedBrowserNames = Enum.GetNames(typeof(SupportedBrowser));
            return Directory.GetDirectories(storagePath)
                .Where(dir => supportedBrowserNames.Contains(Path.GetFileName(dir)))
                .Select(dir => {
                    var key = Path.GetFileName(dir);
                    browsers.TryGetValue(key, out var browser);
                    return new PowerBrowserInstance(dir, browser);
                }).ToList();
        }
        public static string GetBrowserStoragePath()
        {
            var browserPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PowerBrowser",
                "Browsers"
            );

            return browserPath;
        }

        public static bool RemoveBrowser(SupportedBrowser browserType)
        {
            var namedBrowserPath = GetBrowserInstancePath(browserType);
            if (Directory.Exists(namedBrowserPath))
            {
                Directory.Delete(namedBrowserPath, true);
                return true;
            }
            return false;
        }

        public static string GetBrowserInstancePath(SupportedBrowser supportedBrowser)
        {
            var storagePath = GetBrowserStoragePath();
            return Path.Combine(storagePath, supportedBrowser.ToString());
        }

        public static void DownloadBrowser(SupportedBrowser BrowserType)
        {
            var namedBrowserPath = GetBrowserInstancePath(BrowserType);
            Directory.CreateDirectory(namedBrowserPath);

            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = namedBrowserPath,
                Browser = BrowserType
            });

            browserFetcher.DownloadAsync().GetAwaiter().GetResult();
        }

        public static bool StopBrowser(PowerBrowserInstance browser, SessionState sessionState)
        {

            if (!browser.Running)
            {
                return false;
            }

            browser.Browser.CloseAsync().GetAwaiter().GetResult();

            SessionStateHelper.RemoveBrowserInstance(browser.BrowserType, sessionState);
            return true;
        }
        public static PowerBrowserInstance StartBrowser(SupportedBrowser browserType, bool headless, int width, int height, SessionState sessionState)
        {
            var path = GetBrowserInstancePath(browserType);

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
                Browser = browserType
            });

            var installedBrowsers = browserFetcher.GetInstalledBrowsers().ToArray();
            if (installedBrowsers.Length == 0)
            {
                throw new InvalidOperationException($"No browser installations found in {path}");
            }

            var browserInfo = installedBrowsers[0]; // Use the first (and should be only) installation
            launchOptions.ExecutablePath = browserInfo.GetExecutablePath();

            var browser = Puppeteer.LaunchAsync(launchOptions).GetAwaiter().GetResult();

            var powerBrowserInstance = new PowerBrowserInstance(
                browser,
                headless,
                $"{width}x{height}",
                path
            );

            SessionStateHelper.SaveBrowserInstance(powerBrowserInstance.BrowserType, browser, sessionState);

            return powerBrowserInstance;
        }
    }
}