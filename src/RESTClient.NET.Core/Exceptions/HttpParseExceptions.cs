using System;

namespace RESTClient.NET.Core.Exceptions
{
    /// <summary>
    /// Base exception for HTTP file parsing errors
    /// </summary>
    public class HttpParseException : Exception
    {
        /// <summary>
        /// Gets the line number where the parsing error occurred
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the column number where the parsing error occurred
        /// </summary>
        public int ColumnNumber { get; }

        /// <summary>
        /// Gets the content that was being parsed when the error occurred
        /// </summary>
        public string? ParsedContent { get; }

        /// <summary>
        /// Initializes a new instance of the HttpParseException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="lineNumber">The line number where the error occurred</param>
        /// <param name="columnNumber">The column number where the error occurred</param>
        /// <param name="parsedContent">The content being parsed</param>
        /// <param name="innerException">The inner exception</param>
        public HttpParseException(
            string message,
            int lineNumber = 0,
            int columnNumber = 0,
            string? parsedContent = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
            ParsedContent = parsedContent;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var location = LineNumber > 0 ? $" at line {LineNumber}" : string.Empty;
            if (ColumnNumber > 0)
            {
                location += $", column {ColumnNumber}";
            }

            return $"{Message}{location}";
        }
    }

    /// <summary>
    /// Exception thrown when duplicate request names are found
    /// </summary>
    public class DuplicateRequestNameException : HttpParseException
    {
        /// <summary>
        /// Gets the duplicate request name
        /// </summary>
        public string DuplicateRequestName { get; }

        /// <summary>
        /// Gets the line number where the request name was first defined
        /// </summary>
        public int FirstOccurrenceLineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the DuplicateRequestNameException class
        /// </summary>
        /// <param name="requestName">The duplicate request name</param>
        /// <param name="currentLineNumber">The line number of the duplicate occurrence</param>
        /// <param name="firstOccurrenceLineNumber">The line number of the first occurrence</param>
        public DuplicateRequestNameException(
            string requestName,
            int currentLineNumber,
            int firstOccurrenceLineNumber)
            : base($"Duplicate request name '{requestName}' found. First defined at line {firstOccurrenceLineNumber}", currentLineNumber)
        {
            DuplicateRequestName = requestName;
            FirstOccurrenceLineNumber = firstOccurrenceLineNumber;
        }
    }

    /// <summary>
    /// Exception thrown when duplicate request IDs are found
    /// </summary>
    public class DuplicateRequestIdException : HttpParseException
    {
        /// <summary>
        /// Gets the duplicate request ID
        /// </summary>
        public string DuplicateRequestId { get; }

        /// <summary>
        /// Gets the line number where the request ID was first defined
        /// </summary>
        public int FirstOccurrenceLineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the DuplicateRequestIdException class
        /// </summary>
        /// <param name="requestId">The duplicate request ID</param>
        /// <param name="currentLineNumber">The line number of the duplicate occurrence</param>
        /// <param name="firstOccurrenceLineNumber">The line number of the first occurrence</param>
        public DuplicateRequestIdException(
            string requestId,
            int currentLineNumber,
            int firstOccurrenceLineNumber)
            : base($"Duplicate request ID '{requestId}' found. First defined at line {firstOccurrenceLineNumber}", currentLineNumber)
        {
            DuplicateRequestId = requestId;
            FirstOccurrenceLineNumber = firstOccurrenceLineNumber;
        }
    }

    /// <summary>
    /// Exception thrown when a request is missing a required request ID
    /// </summary>
    public class MissingRequestIdException : HttpParseException
    {
        /// <summary>
        /// Gets the line number where the request starts
        /// </summary>
        public int RequestStartLineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the MissingRequestIdException class
        /// </summary>
        /// <param name="requestStartLineNumber">The line number where the request starts</param>
        public MissingRequestIdException(int requestStartLineNumber)
            : base($"Request at line {requestStartLineNumber} is missing a required request ID", requestStartLineNumber)
        {
            RequestStartLineNumber = requestStartLineNumber;
        }
    }

    /// <summary>
    /// Exception thrown when a request is missing a required request name
    /// </summary>
    public class MissingRequestNameException : HttpParseException
    {
        /// <summary>
        /// Gets the line number where the request starts
        /// </summary>
        public int RequestStartLineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the MissingRequestNameException class
        /// </summary>
        /// <param name="requestStartLineNumber">The line number where the request starts</param>
        public MissingRequestNameException(int requestStartLineNumber)
            : base($"Request at line {requestStartLineNumber} is missing a required request name", requestStartLineNumber)
        {
            RequestStartLineNumber = requestStartLineNumber;
        }
    }

    /// <summary>
    /// Exception thrown when an invalid request name format is encountered
    /// </summary>
    public class InvalidRequestNameException : HttpParseException
    {
        /// <summary>
        /// Gets the invalid request name
        /// </summary>
        public string InvalidRequestName { get; }

        /// <summary>
        /// Initializes a new instance of the InvalidRequestNameException class
        /// </summary>
        /// <param name="invalidRequestName">The invalid request name</param>
        /// <param name="lineNumber">The line number where the invalid request name was found</param>
        public InvalidRequestNameException(string invalidRequestName, int lineNumber)
            : base($"Invalid request name '{invalidRequestName}'. Request names must contain only alphanumeric characters, hyphens, and underscores", lineNumber)
        {
            InvalidRequestName = invalidRequestName;
        }
    }

    /// <summary>
    /// Exception thrown when an invalid request ID format is encountered
    /// </summary>
    public class InvalidRequestIdException : HttpParseException
    {
        /// <summary>
        /// Gets the invalid request ID
        /// </summary>
        public string InvalidRequestId { get; }

        /// <summary>
        /// Initializes a new instance of the InvalidRequestIdException class
        /// </summary>
        /// <param name="invalidRequestId">The invalid request ID</param>
        /// <param name="lineNumber">The line number where the invalid request ID was found</param>
        public InvalidRequestIdException(string invalidRequestId, int lineNumber)
            : base($"Invalid request ID '{invalidRequestId}'. Request IDs must contain only alphanumeric characters, hyphens, and underscores", lineNumber)
        {
            InvalidRequestId = invalidRequestId;
        }
    }
}
