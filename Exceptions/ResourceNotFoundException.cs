using System;
using System.Management.Automation;

namespace PowerBrowser.Exceptions
{
    /// <summary>
    /// Exception thrown when a specific resource cannot be found (e.g., page not found, element not found)
    /// </summary>
        public class ResourceNotFoundException : PowerBrowserException
        {
        public ResourceNotFoundException(string message)
            : base(message, "ResourceNotFound", ErrorCategory.ObjectNotFound)
        {
        }

        public ResourceNotFoundException(string message, Exception innerException)
            : base(message, "ResourceNotFound", ErrorCategory.ObjectNotFound, innerException)
        {
        }
    }
}