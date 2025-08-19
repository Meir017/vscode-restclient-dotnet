using System;
using System.Collections.Generic;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents a single HTTP request with comprehensive metadata and enhanced expectation support.
    /// Provides the core model for individual HTTP requests parsed from VS Code REST Client files.
    /// </summary>
    /// <remarks>
    /// <para>HttpRequest encapsulates all aspects of an HTTP request:</para>
    /// <list type="bullet">
    /// <item>Standard HTTP components: method, URL, headers, body</item>
    /// <item>Enhanced metadata: expectations, custom settings, line numbers</item>
    /// <item>Name-based identification for programmatic access</item>
    /// <item>Case-insensitive header handling for HTTP standard compliance</item>
    /// </list>
    /// <para>Request names are extracted from <c># @name</c> comments and must be unique within a file.</para>
    /// <para>Metadata includes expectation comments like <c># @expect-status</c> for automated testing.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a request programmatically
    /// var request = new HttpRequest
    /// {
    ///     Name = "create-user",
    ///     Method = "POST",
    ///     Url = "https://api.example.com/users",
    ///     Body = @"{""name"": ""John Doe"", ""email"": ""john@example.com""}"
    /// };
    /// 
    /// // Add headers (case-insensitive)
    /// request.Headers["Content-Type"] = "application/json";
    /// request.Headers["Authorization"] = "Bearer token123";
    /// 
    /// // Set expectations for testing
    /// request.Metadata.ExpectedStatusCode = 201;
    /// request.Metadata.ExpectedHeaders["Location"] = "/users/123";
    /// 
    /// // Access request properties
    /// Console.WriteLine($"Request: {request.Method} {request.Url}");
    /// Console.WriteLine($"Expected status: {request.Metadata.ExpectedStatusCode}");
    /// Console.WriteLine($"Headers count: {request.Headers.Count}");
    /// </code>
    /// </example>
    public class HttpRequest
    {
        /// <summary>
        /// Gets or sets the request name (from @name comment)
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
        /// Gets or sets the file body reference for external body content
        /// </summary>
        /// <remarks>
        /// When this property is set, the Body property should be null.
        /// File body references allow loading request body content from external files
        /// with optional variable processing and custom encoding support.
        /// </remarks>
        public FileBodyReference? FileBodyReference { get; set; }

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
