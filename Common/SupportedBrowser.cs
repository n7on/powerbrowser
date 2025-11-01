using System;
using PuppeteerSharp;

namespace PowerBrowser.Common
{

    public enum SupportedPBrowser
    {
        /// <summary>
        /// Chrome.
        /// </summary>
        Chrome,

        /// <summary>
        /// Firefox.
        /// </summary>
        Firefox,

        /// <summary>
        /// Chromium.
        /// </summary>
        Chromium,

        /// <summary>
        /// Chrome headless shell.
        /// </summary>
        ChromeHeadlessShell,
    }
    public static class SupportedPBrowserExtensions
    {

        public static SupportedPBrowser ToSupportedPBrowser(this SupportedBrowser browserType)
        {
            if (Enum.TryParse<SupportedPBrowser>(browserType.ToString(), out var result))
            {   
                return result;
            }
            throw new ArgumentException($"Invalid browser type: {browserType}");
        }
        public static SupportedPBrowser ToSupportedPBrowser(this string browserType)
        {
            if (Enum.TryParse<SupportedPBrowser>(browserType, true, out var result))
            {
                return result;
            }
            throw new ArgumentException($"Invalid browser type: {browserType}");
        }
        public static string GetFriendlyName(this SupportedPBrowser browser)
        {
            return browser switch
            {
                SupportedPBrowser.Chrome => "Google Chrome",
                SupportedPBrowser.Firefox => "Mozilla Firefox",
                SupportedPBrowser.Chromium => "Chromium",
                SupportedPBrowser.ChromeHeadlessShell => "Chrome Headless Shell",
                _ => "Unknown Browser"
            };
        }
    }
}