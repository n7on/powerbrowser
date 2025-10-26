using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerBrowser.Cmdlets
{
    public abstract class BrowserCmdletBase : PSCmdlet
    {
        /// <summary>
        /// Gets the browser storage path where all browsers are installed
        /// </summary>
        protected string GetBrowserStoragePath()
        {
            var userDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var browserPath = Path.Combine(userDataPath, "PowerBrowser", "Browsers");
            
            WriteVerbose($"Browser storage path: {browserPath}");
            return browserPath;
        }

        /// <summary>
        /// Ensures the browser storage directory exists
        /// </summary>
        protected void EnsureBrowserStorageExists()
        {
            var browserPath = GetBrowserStoragePath();
            if (!Directory.Exists(browserPath))
            {
                WriteVerbose($"Creating browser storage directory: {browserPath}");
                Directory.CreateDirectory(browserPath);
            }
        }



        /// <summary>
        /// Gets the path for a specific named browser installation
        /// </summary>
        protected string GetNamedBrowserPath(string browserName)
        {
            var storagePath = GetBrowserStoragePath();
            return Path.Combine(storagePath, browserName);
        }

        /// <summary>
        /// Gets all installed browser names
        /// </summary>
        protected string[] GetInstalledBrowserNames()
        {
            var storagePath = GetBrowserStoragePath();
            if (!Directory.Exists(storagePath))
                return Array.Empty<string>();

            return Directory.GetDirectories(storagePath)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Cast<string>()
                .OrderBy(name => name)
                .ToArray();
        }
    }
}
