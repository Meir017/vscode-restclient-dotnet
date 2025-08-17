using System;
using System.Collections.Generic;

namespace RESTClient.NET.Testing.Models
{
    /// <summary>
    /// Represents a test case generated from an HTTP file request
    /// </summary>
    public class HttpTestCase
    {
        /// <summary>
        /// Gets or sets the name of the test case (from # @name comment)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the request URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP headers for the request
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the request body content
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Gets or sets the expected response information for assertions
        /// </summary>
        public HttpExpectedResponse? ExpectedResponse { get; set; }

        /// <summary>
        /// Gets or sets the line number where this request starts in the HTTP file
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for this test case
        /// </summary>
        public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
