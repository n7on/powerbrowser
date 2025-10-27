using System;
using System.Management.Automation;

namespace PowerBrowser.Exceptions
{
    /// <summary>
    /// Base exception for all PowerBrowser operations
    /// </summary>
    public abstract class PowerBrowserException : Exception
    {
        public string ErrorId { get; }
        public ErrorCategory Category { get; }

        protected PowerBrowserException(string message, string errorId, ErrorCategory category) 
            : base(message)
        {
            ErrorId = errorId;
            Category = category;
        }

        protected PowerBrowserException(string message, string errorId, ErrorCategory category, Exception innerException) 
            : base(message, innerException)
        {
            ErrorId = errorId;
            Category = category;
        }
    }
}