using System;
using System.Collections.Generic;
using PuppeteerSharp;

namespace PowerBrowser.Models
{
    /// <summary>
    /// PowerShell-friendly wrapper for IBrowser with additional metadata
    /// </summary>
    public class PowerBrowserInstance
    {
        public string Name { get; set; }
        public IBrowser Browser { get; set; }
        public DateTime StartTime { get; set; }
        public bool Headless { get; set; }
        public string WindowSize { get; set; }
        public List<PowerBrowserPage> Pages { get; set; }
        
        // Additional properties for Get-Browser display
        public string Size { get; set; }
        public string Path { get; set; }

        // Added a static property to manage instances of PowerBrowserInstance.
        private static readonly Dictionary<string, PowerBrowserInstance> _instances = new Dictionary<string, PowerBrowserInstance>();
        public static Dictionary<string, PowerBrowserInstance> Instances => _instances;

        public PowerBrowserInstance(string name, IBrowser browser, bool headless, string windowSize)
        {
            Name = name;
            Browser = browser;
            StartTime = DateTime.Now;
            Headless = headless;
            WindowSize = windowSize;
            Pages = new List<PowerBrowserPage>();
        }

        // Properties for PowerShell display
        public int ProcessId => Browser?.Process?.Id ?? -1;
        public string WebSocketEndpoint => Browser?.WebSocketEndpoint ?? "Unknown";
        public bool Running => Browser?.IsConnected ?? false;  // User-friendly alias
        public int PageCount => Pages.Count;

        public override string ToString()
        {
            return $"{Name} (PID: {ProcessId}, Pages: {PageCount}, Running: {Running})";
        }
    }
}