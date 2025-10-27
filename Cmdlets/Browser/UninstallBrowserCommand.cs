using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerBrowser.Cmdlets.Browser
{
    [Cmdlet(VerbsLifecycle.Uninstall, "Browser")]
    [OutputType(typeof(string))]
    public class UninstallBrowserCommand : BrowserCmdletBase
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "Name of the browser installation to uninstall")]
        [ArgumentCompleter(typeof(BrowserNameCompleter))]
        public string Name { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Remove without confirmation")]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var namedBrowserPath = GetNamedBrowserPath(Name);
                
                if (!Directory.Exists(namedBrowserPath))
                {
                    WriteError(new ErrorRecord(
                        new DirectoryNotFoundException($"Browser '{Name}' not found."),
                        "BrowserNotFound", ErrorCategory.ObjectNotFound, Name));
                    return;
                }

                // Confirm removal unless Force is specified
                if (!Force.IsPresent)
                {
                    if (!ShouldProcess($"Browser installation '{Name}'", "Uninstall"))
                    {
                        WriteObject("Uninstall cancelled.");
                        return;
                    }
                }

                WriteObject($"🗑️ Removing '{Name}'...");

                try
                {
                    Directory.Delete(namedBrowserPath, true);
                    WriteVerbose($"Removed directory: {namedBrowserPath}");
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "BrowserRemovalFailed", 
                        ErrorCategory.OperationStopped, Name));
                    return;
                }
                
                WriteObject($"✅ Browser '{Name}' uninstalled successfully!");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "UninstallBrowserFailed", ErrorCategory.OperationStopped, null));
            }
        }
    }

    public class BrowserNameCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, 
            string wordToComplete, System.Management.Automation.Language.CommandAst commandAst, 
            System.Collections.IDictionary fakeBoundParameters)
        {
            var results = new List<CompletionResult>();
            
            try
            {
                var userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var browserStoragePath = Path.Combine(userDataPath, "PowerBrowser", "Browsers");
                
                if (!Directory.Exists(browserStoragePath))
                    return results;

                var installedBrowsers = Directory.GetDirectories(browserStoragePath)
                    .Select(Path.GetFileName)
                    .Where(name => !string.IsNullOrEmpty(name) && 
                                   name.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(name => name);

                foreach (var browserName in installedBrowsers)
                {
                    results.Add(new CompletionResult(browserName, browserName, CompletionResultType.ParameterValue, browserName));
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
