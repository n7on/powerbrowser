using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using PowerBrowser.Services;
using System.Management.Automation.Language;
using System;

namespace PowerBrowser.Completers
{
    public class InstalledBrowserCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            foreach (var browserType in BrowserService.GetInstalledBrowserTypes())
            {
                if (browserType.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new CompletionResult(browserType);
                }
            }
        }
    }
}