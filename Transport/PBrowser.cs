using System;
using System.Collections.Generic;
using PuppeteerSharp;
using System.Management.Automation;
using PowerBrowser.Common;

namespace PowerBrowser.Transport
{
    /// <summary>
    /// PowerShell-friendly wrapper for IBrowser with additional metadata
    /// </summary>
    public class PBrowser
    {
    
        [Hidden]
        public IBrowser Browser { get; set; }
        public SupportedPBrowser BrowserType { get; set; }
        public DateTime StartTime { get; set; }
        public bool Headless { get; set; }
        public string WindowSize { get; set; }

        // Additional properties for Get-Browser display
        public string Size { get; set; }
        public string Path { get; set; }

        public PBrowser(IBrowser browser, bool headless, string windowSize, string path)
        {
            Browser = browser;
            BrowserType = browser?.BrowserType.ToSupportedPBrowser() ?? SupportedPBrowser.Chrome;   
            StartTime = DateTime.Now;
            Headless = headless;
            WindowSize = windowSize;
            Path = path;
        }

        public PBrowser(SupportedPBrowser browserType, string path)
        {
            Browser = null;
            BrowserType = browserType;
            StartTime = DateTime.MinValue;
            Headless = false;
            WindowSize = "Unknown"; // Default size
            Path = path;
            Size = "Unknown"; // Default size
        }

        // Properties for PowerShell display
        public int ProcessId => Browser?.Process?.Id ?? -1;
        public string WebSocketEndpoint => Browser?.WebSocketEndpoint ?? "Unknown";
        public bool Running => Browser?.IsConnected ?? false;  // User-friendly alias
        public int PageCount => Browser?.PagesAsync().GetAwaiter().GetResult().Length ?? 0;

        public override string ToString()
        {
            return $"(PID: {ProcessId}, Pages: {PageCount}, Running: {Running})";
        }
    }
}