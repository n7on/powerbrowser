using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using PowerBrowser.Models;
using PuppeteerSharp;

namespace PowerBrowser.Helpers
{
    public static class SessionStateHelper
    {
        private const string RunningBrowsersKey = "PowerBrowser_RunningBrowsers";

        public static void SaveBrowserInstance(string name, IBrowser browser, SessionState sessionState)
        {
            if (sessionState == null)
            {
                throw new ArgumentNullException(nameof(sessionState));
            }

            if (!(sessionState.PSVariable.GetValue(RunningBrowsersKey) is Dictionary<string, IBrowser> runningBrowsers))
            {
                runningBrowsers = new Dictionary<string, IBrowser>();
                sessionState.PSVariable.Set(RunningBrowsersKey, runningBrowsers);
            }

            runningBrowsers[name] = browser;
        }

        public static Dictionary<string, IBrowser> GetRunningBrowsers(SessionState sessionState)
        {
            if (sessionState == null)
            {
                throw new ArgumentNullException(nameof(sessionState));
            }

            if (sessionState.PSVariable.GetValue(RunningBrowsersKey) is Dictionary<string, IBrowser> runningBrowsers)
            {
                return runningBrowsers;
            }

            return new Dictionary<string, IBrowser>();
        }

        public static IBrowser GetBrowserInstance(string name, SessionState sessionState)
        {
            var runningBrowsers = GetRunningBrowsers(sessionState);
            if (runningBrowsers.ContainsKey(name))
            {
                return runningBrowsers[name];
            }

            return null;
        }   
        public static void RemoveBrowserInstance(string name, SessionState sessionState)
        {

            var runningBrowsers = GetRunningBrowsers(sessionState);
            if (runningBrowsers.ContainsKey(name))
            {
                runningBrowsers.Remove(name);
                sessionState.PSVariable.Set(RunningBrowsersKey, runningBrowsers);
            }
        }
    }
}