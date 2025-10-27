using System;
using System.Management.Automation;
using PuppeteerSharp;
using PowerBrowser.Models;
using PowerBrowser.Exceptions;
using System.Collections.Generic;

namespace PowerBrowser.Cmdlets.BrowserPage
{
    public abstract class BrowserPageCmdletBase : PSCmdlet
    {
        [Parameter(
            HelpMessage = "Timeout in milliseconds for navigation (default: 30000)")]
        public int Timeout { get; set; } = 30000;

        [Parameter(
            HelpMessage = "What to wait for during navigation")]
        public WaitUntilNavigation[] WaitUntil { get; set; } = new[] { WaitUntilNavigation.Load };

        [Parameter(
            HelpMessage = "Referer header to send with the navigation request")]
        public string Referer { get; set; } = string.Empty;

        protected void NavigateToUrl(PowerBrowserPage powerPage, string url, SwitchParameter waitForLoad)
        {
            try
            {
                var navigationOptions = new NavigationOptions
                {
                    Timeout = Timeout
                };

                // Set referer if provided
                if (!string.IsNullOrEmpty(Referer))
                {
                    navigationOptions.Referer = Referer;
                }

                // Configure what to wait for
                if (waitForLoad.IsPresent || WaitUntil.Length > 0)
                {
                    navigationOptions.WaitUntil = WaitUntil.Length > 0 ? WaitUntil : new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded };
                }

                WriteVerbose($"🔄 Navigating with timeout: {Timeout}ms, WaitUntil: {string.Join(", ", navigationOptions.WaitUntil)}");

                // Perform the navigation
                var navigationTask = powerPage.Page.GoToAsync(url, navigationOptions);
                var response = navigationTask.GetAwaiter().GetResult();

                if (response == null)
                {
                    WriteWarning($"Navigation to '{url}' completed but no response received. This may indicate a client-side navigation or redirect.");
                }
                else
                {
                    WriteVerbose($"📄 Navigation response: {response.Status} {response.StatusText}");

                    // Check for navigation errors
                    if ((int)response.Status >= 400)
                    {
                        WriteWarning($"Navigation resulted in HTTP {response.Status} {response.StatusText}");
                    }
                }
            }
            catch (TimeoutException)
            {
                throw new ResourceUnavailableException($"Navigation to '{url}' timed out after {Timeout}ms. The page may be taking too long to load.");
            }
            catch (NavigationException navEx)
            {
                throw new ResourceUnavailableException($"Navigation to '{url}' failed: {navEx.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("net::"))
            {
                throw new ResourceUnavailableException($"Network error during navigation to '{url}': {ex.Message}");
            }
        }
    }
    public class RunningBrowserCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName,
            string wordToComplete, System.Management.Automation.Language.CommandAst commandAst,
            System.Collections.IDictionary fakeBoundParameters)
        {
            var results = new List<CompletionResult>();

            try
            {
                // This is a simplified completer - in a real session, we'd access the session state
                // For now, we'll return common browser names
                var commonBrowsers = new[] { "Chrome", "Firefox", "ChromeHeadlessShell", "Chromium" };

                foreach (var browserName in commonBrowsers)
                {
                    if (browserName.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new CompletionResult(browserName, browserName, CompletionResultType.ParameterValue, browserName));
                    }
                }
            }
            catch
            {
                // Ignore errors during completion
            }

            return results;
        }
    }
}