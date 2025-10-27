using System;
using System.Management.Automation;

namespace PowerBrowser.Exceptions
{
    /// <summary>
    /// Exception thrown when a required parameter is missing or invalid
    /// </summary>
        public class RequiredParameterException : PowerBrowserException
        {
        public RequiredParameterException(string message)
            : base(message, "RequiredParameter", ErrorCategory.InvalidArgument)
        {
        }

        public RequiredParameterException(string message, Exception innerException)
            : base(message, "RequiredParameter", ErrorCategory.InvalidArgument, innerException)
        {
        }
    }
}