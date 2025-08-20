using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents captured response data from an executed HTTP request
    /// Used for request response chaining with variables like {{requestName.response.body.$.token}}
    /// </summary>
    public class HttpResponseData
    {
        /// <summary>
        /// The original HTTP response message
        /// </summary>
        public HttpResponseMessage? Response { get; set; }

        /// <summary>
        /// The response body content as a string
        /// </summary>
        public string? BodyContent { get; set; }

        /// <summary>
        /// Parsed JSON content for JSONPath queries (null if not JSON)
        /// </summary>
        public JToken? ParsedBody { get; set; }

        /// <summary>
        /// Response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Response content type
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Response content length
        /// </summary>
        public long? ContentLength { get; set; }

        /// <summary>
        /// Request execution timestamp
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Response time in milliseconds
        /// </summary>
        public double ResponseTimeMs { get; set; }

        /// <summary>
        /// Creates HttpResponseData from an HttpResponseMessage
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="bodyContent">The response body content</param>
        /// <param name="responseTimeMs">Response time in milliseconds</param>
        /// <returns>HttpResponseData instance</returns>
        public static HttpResponseData FromHttpResponse(HttpResponseMessage response, string bodyContent, double responseTimeMs = 0)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var headers = new Dictionary<string, string>();
            foreach (var header in response.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }

            if (response.Content?.Headers != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            JToken? parsedBody = null;
            try
            {
                if (!string.IsNullOrEmpty(bodyContent))
                {
                    // Try to parse as JSON if content type indicates JSON or if content looks like JSON
                    var contentType = response.Content?.Headers?.ContentType?.MediaType;
                    var looksLikeJson = bodyContent.TrimStart().StartsWith("{") || bodyContent.TrimStart().StartsWith("[");

                    if (IsJsonContent(contentType) || looksLikeJson)
                    {
                        parsedBody = JToken.Parse(bodyContent);
                    }
                }
            }
            catch
            {
                // If JSON parsing fails, leave parsedBody as null
                // This is expected for non-JSON responses
            }

            return new HttpResponseData
            {
                Response = response,
                BodyContent = bodyContent,
                ParsedBody = parsedBody,
                StatusCode = response.StatusCode,
                Headers = headers,
                ContentType = response.Content?.Headers?.ContentType?.MediaType,
                ContentLength = response.Content?.Headers?.ContentLength,
                ResponseTimeMs = responseTimeMs
            };
        }

        /// <summary>
        /// Gets a value from the response using JSONPath syntax
        /// </summary>
        /// <param name="jsonPath">JSONPath expression (e.g., "$.token", "$.user.id")</param>
        /// <returns>The value as string, or null if not found</returns>
        public string? GetJsonPathValue(string jsonPath)
        {
            if (ParsedBody == null || string.IsNullOrEmpty(jsonPath))
                return null;

            try
            {
                var token = ParsedBody.SelectToken(jsonPath);
                return token?.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a header value by name (case-insensitive)
        /// </summary>
        /// <param name="headerName">Header name</param>
        /// <returns>Header value or null if not found</returns>
        public string? GetHeaderValue(string headerName)
        {
            if (string.IsNullOrEmpty(headerName))
                return null;

            foreach (var kvp in Headers)
            {
                if (string.Equals(kvp.Key, headerName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        private static bool IsJsonContent(string? mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
                return false;

            return mediaType!.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   mediaType.IndexOf("text/json", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
