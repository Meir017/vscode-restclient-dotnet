using System;
using System.Collections.Generic;

namespace RESTClient.NET.Testing.Models
{
    /// <summary>
    /// Represents expected response information for HTTP test assertions
    /// </summary>
    public class HttpExpectedResponse
    {
        /// <summary>
        /// Gets or sets the expected HTTP status code
        /// </summary>
        public int? ExpectedStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the expected HTTP headers
        /// </summary>
        public IReadOnlyDictionary<string, string> ExpectedHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the expected content that should be contained in the response body
        /// </summary>
        public string? ExpectedBodyContains { get; set; }

        /// <summary>
        /// Gets or sets the JSONPath expression to validate against the response body
        /// </summary>
        public string? ExpectedBodyPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the JSON schema file for response validation
        /// </summary>
        public string? ExpectedSchemaPath { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed response time
        /// </summary>
        public TimeSpan? MaxResponseTime { get; set; }

        /// <summary>
        /// Gets or sets additional custom expectations
        /// </summary>
        public IDictionary<string, object> CustomExpectations { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets a value indicating whether this response has any expectations defined
        /// </summary>
        public bool HasExpectations => ExpectedStatusCode.HasValue ||
                                       ExpectedHeaders.Count > 0 ||
                                       !string.IsNullOrEmpty(ExpectedBodyContains) ||
                                       !string.IsNullOrEmpty(ExpectedBodyPath) ||
                                       !string.IsNullOrEmpty(ExpectedSchemaPath) ||
                                       MaxResponseTime.HasValue ||
                                       CustomExpectations.Count > 0;
    }
}
