using System;
using System.Collections.Generic;
using PuppeteerSharp;
using System.Management.Automation;

namespace PowerBrowser.Models
{
    /// <summary>
    /// PowerShell-friendly wrapper for IBrowser with additional metadata
    /// </summary>
    public class PowerBrowserInstance
    {
    
        [Hidden]
        public IBrowser Browser { get; set; }
        public string BrowserType { get; set; }
        public DateTime StartTime { get; set; }
        public bool Headless { get; set; }
        public string WindowSize { get; set; }
        public List<PowerBrowserPage> Pages { get; set; }
        
        // Additional properties for Get-Browser display
        public string Size { get; set; }
        public string Path { get; set; }

        public PowerBrowserInstance(IBrowser browser, bool headless, string windowSize, string path)
        {
            Browser = browser;
            BrowserType = browser?.BrowserType.ToString() ?? "Unknown";
            StartTime = DateTime.Now;
            Headless = headless;
            WindowSize = windowSize;
            Pages = new List<PowerBrowserPage>();
            Path = path;
        }

        public PowerBrowserInstance(string path, IBrowser browser)
        {
            Browser = browser;
            BrowserType = System.IO.Path.GetFileName(path);
            StartTime = DateTime.Now;
            Headless = false;
            WindowSize = "Unknown"; // Default size
            Path = path;
            Pages = new List<PowerBrowserPage>();
            Size = "Unknown"; // Default size
        }

        // Properties for PowerShell display
        public int ProcessId => Browser?.Process?.Id ?? -1;
        public string WebSocketEndpoint => Browser?.WebSocketEndpoint ?? "Unknown";
        public bool Running => Browser?.IsConnected ?? false;  // User-friendly alias
        public int PageCount => Pages.Count;

        public override string ToString()
        {
            return $"(PID: {ProcessId}, Pages: {PageCount}, Running: {Running})";
        }
    }
}