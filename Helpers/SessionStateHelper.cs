using System;
using System.Collections.Generic;
using System.Management.Automation;
using PowerBrowser.Models;

namespace PowerBrowser.Helpers
{
    public static class SessionStateHelper
    {
        private const string RunningBrowsersKey = "PowerBrowser_RunningBrowsers";

        public static void SaveBrowserInstance(PowerBrowserInstance browserInstance, SessionState sessionState)
        {
            if (sessionState == null)
            {
                throw new ArgumentNullException(nameof(sessionState));
            }

            if (!(sessionState.PSVariable.GetValue(RunningBrowsersKey) is Dictionary<string, PowerBrowserInstance> runningBrowsers))
            {
                runningBrowsers = new Dictionary<string, PowerBrowserInstance>();
                sessionState.PSVariable.Set(RunningBrowsersKey, runningBrowsers);
            }

            runningBrowsers[browserInstance.Name] = browserInstance;
        }

        public static Dictionary<string, PowerBrowserInstance> GetRunningBrowsers(SessionState sessionState)
        {
            if (sessionState == null)
            {
                throw new ArgumentNullException(nameof(sessionState));
            }

            if (sessionState.PSVariable.GetValue(RunningBrowsersKey) is Dictionary<string, PowerBrowserInstance> runningBrowsers)
            {
                return runningBrowsers;
            }

            return new Dictionary<string, PowerBrowserInstance>();
        }

        public static void RemoveBrowserInstance(string processId, SessionState sessionState)
        {
            if (sessionState == null)
            {
                throw new ArgumentNullException(nameof(sessionState));
            }

            var runningBrowsers = GetRunningBrowsers(sessionState);
            if (runningBrowsers.ContainsKey(processId))
            {
                runningBrowsers.Remove(processId);
                sessionState.PSVariable.Set(RunningBrowsersKey, runningBrowsers);
            }
        }
    }
}