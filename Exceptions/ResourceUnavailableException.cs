using System;
using System.Management.Automation;

namespace PowerBrowser.Exceptions
{
    /// <summary>
    /// Exception thrown when a required resource is not available (e.g., no browsers running, no pages open)
    /// </summary>
        public class ResourceUnavailableException : PowerBrowserException
        {
        public ResourceUnavailableException(string message)
            : base(message, "ResourceUnavailable", ErrorCategory.ResourceUnavailable)
        {
        }

        public ResourceUnavailableException(string message, Exception innerException)
            : base(message, "ResourceUnavailable", ErrorCategory.ResourceUnavailable, innerException)
        {
        }
    }
}