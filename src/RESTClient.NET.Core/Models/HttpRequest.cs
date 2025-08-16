using System;
using System.Collections.Generic;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents a single HTTP request with metadata
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// Gets or sets the request name (from @name comment)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique request identifier
        /// </summary>
        [Obsolete("Use Name property instead. This property will be removed in a future version.")]
        public string RequestId
        {
            get => Name;
            set => Name = value;
        }

        /// <summary>
        /// Gets or sets the HTTP method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        /// Gets or sets the request URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets the HTTP headers for the request
        /// </summary>
        public Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets or sets the request body content
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Gets the request metadata and settings
        /// </summary>
        public HttpRequestMetadata Metadata { get; }

        /// <summary>
        /// Gets or sets the line number where this request starts in the source file
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Initializes a new instance of the HttpRequest class
        /// </summary>
        public HttpRequest()
        {
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Metadata = new HttpRequestMetadata();
        }

        /// <summary>
        /// Gets a header value by name (case-insensitive)
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>The header value if found; otherwise, null</returns>
        public string? GetHeader(string name)
        {
            return Headers.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// Sets a header value for the request
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The header value</param>
        public void SetHeader(string name, string value)
        {
            Headers[name] = value;
        }

        /// <summary>
        /// Checks if the request has a specific header (case-insensitive)
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>true if the header exists; otherwise, false</returns>
        public bool HasHeader(string name)
        {
            return Headers.ContainsKey(name);
        }

        /// <summary>
        /// Removes a header from the request
        /// </summary>
        /// <param name="name">The header name to remove</param>
        /// <returns>true if the header was removed; otherwise, false</returns>
        public bool RemoveHeader(string name)
        {
            return Headers.Remove(name);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}: {Method} {Url}";
        }
    }
}
