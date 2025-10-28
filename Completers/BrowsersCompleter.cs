using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PowerBrowser.Helpers;
using System.Linq;
using System.Management.Automation.Language;
using System;

namespace PowerBrowser.Completers
{
    public class InstalledBrowserCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            return BrowserHelper.GetBrowsers()
                .Where(browser => string.IsNullOrEmpty(wordToComplete) || browser.BrowserType.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                .Select(browser => new CompletionResult(browser.BrowserType))
                .ToList();
        }
    }
}