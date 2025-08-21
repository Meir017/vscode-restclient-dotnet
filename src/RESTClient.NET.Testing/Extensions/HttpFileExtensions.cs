using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Processing;
using RESTClient.NET.Testing.Models;

namespace RESTClient.NET.Testing.Extensions
{
    /// <summary>
    /// Extension methods for HttpFile to support testing framework integration
    /// </summary>
    public static class HttpFileExtensions
    {
        /// <summary>
        /// Cache for compiled wildcard regex patterns to improve performance
        /// </summary>
        private static readonly ConcurrentDictionary<string, Regex> _wildcardRegexCache = new();
        /// <summary>
        /// Converts HTTP file requests into xUnit theory test data
        /// </summary>
        /// <param name="httpFile">The HTTP file to convert</param>
        /// <returns>Enumerable of object arrays suitable for xUnit [MemberData]</returns>
        public static IEnumerable<object[]> GetTestData(this HttpFile httpFile)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            return httpFile.GetTestCases().Select(testCase => new object[] { testCase });
        }

        /// <summary>
        /// Converts HTTP file requests into test case objects
        /// </summary>
        /// <param name="httpFile">The HTTP file to convert</param>
        /// <returns>Enumerable of HttpTestCase objects</returns>
        public static IEnumerable<HttpTestCase> GetTestCases(this HttpFile httpFile)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            // Use file variables for variable resolution
            IReadOnlyDictionary<string, string> fileVariables = httpFile.FileVariables;

            foreach (HttpRequest request in httpFile.Requests)
            {
                // Skip requests with empty URLs or auto-generated names from separators
                if (string.IsNullOrWhiteSpace(request.Url) ||
                    (request.Name.StartsWith("request-") && request.Name.Length > 8 &&
                     char.IsDigit(request.Name[8]) && !HasNameMetadata(request)))
                {
                    continue;
                }

                // Resolve variables in the URL
                string? resolvedUrl = VariableProcessor.ResolveVariables(request.Url, fileVariables);

                yield return new HttpTestCase
                {
                    Name = request.Name,
                    Method = request.Method,
                    Url = resolvedUrl ?? request.Url,
                    Headers = request.Headers,
                    Body = request.Body,
                    LineNumber = request.LineNumber,
                    ExpectedResponse = ConvertExpectationsToResponse(request),
                    Metadata = ConvertRequestMetadata(request)
                };
            }
        }

        /// <summary>
        /// Checks if a request has explicit name metadata (not auto-generated)
        /// </summary>
        private static bool HasNameMetadata(HttpRequest request)
        {
            return request.Metadata.CustomMetadata.ContainsKey("name") ||
                   !string.IsNullOrEmpty(request.Metadata.Name);
        }

        /// <summary>
        /// Filters test cases based on criteria
        /// </summary>
        /// <param name="testCases">The test cases to filter</param>
        /// <param name="namePattern">Optional name pattern to match (supports * wildcard)</param>
        /// <param name="methods">Optional HTTP methods to include</param>
        /// <param name="hasExpectations">Optional filter for test cases with expectations</param>
        /// <returns>Filtered test cases</returns>
        public static IEnumerable<HttpTestCase> Filter(
            this IEnumerable<HttpTestCase> testCases,
            string? namePattern = null,
            IEnumerable<string>? methods = null,
            bool? hasExpectations = null)
        {
            ArgumentNullException.ThrowIfNull(testCases);

            IEnumerable<HttpTestCase> filtered = testCases.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(namePattern))
            {
                filtered = filtered.Where(tc => MatchesPattern(tc.Name, namePattern));
            }

            if (methods != null)
            {
                var methodList = methods.ToList();
                if (methodList.Count > 0)
                {
                    filtered = filtered.Where(tc => methodList.Contains(tc.Method, StringComparer.OrdinalIgnoreCase));
                }
            }

            if (hasExpectations.HasValue)
            {
                filtered = filtered.Where(tc => (tc.ExpectedResponse?.HasExpectations ?? false) == hasExpectations.Value);
            }

            return filtered;
        }

        /// <summary>
        /// Matches a name against a pattern that may contain wildcards
        /// </summary>
        /// <param name="name">The name to match</param>
        /// <param name="pattern">The pattern with optional * wildcards</param>
        /// <returns>True if the name matches the pattern</returns>
        private static bool MatchesPattern(string name, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            if (!pattern.Contains('*'))
            {
                // No wildcards, use substring matching (case insensitive)
                return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
            }

            // Handle wildcard patterns with cached regex for efficiency
            string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            Regex regex = _wildcardRegexCache.GetOrAdd(regexPattern,
                pat => new Regex(pat, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            return regex.IsMatch(name);
        }

        /// <summary>
        /// Converts an HttpTestCase to an HttpRequestMessage
        /// </summary>
        /// <param name="testCase">The test case to convert</param>
        /// <param name="fileVariables">Optional file variables for variable resolution</param>
        /// <returns>HttpRequestMessage ready for sending</returns>
        public static HttpRequestMessage ToHttpRequestMessage(this HttpTestCase testCase, IReadOnlyDictionary<string, string>? fileVariables = null)
        {
            ArgumentNullException.ThrowIfNull(testCase);

            var request = new HttpRequestMessage(new HttpMethod(testCase.Method), testCase.Url);

            // Add headers
            foreach (KeyValuePair<string, string> header in testCase.Headers)
            {
                // Resolve variables in header value
                string resolvedValue = fileVariables != null
                    ? VariableProcessor.ResolveVariables(header.Value, fileVariables) ?? header.Value
                    : header.Value;

                // Handle content headers separately
                if (IsContentHeader(header.Key))
                {
                    // Will be added when we set content
                    continue;
                }

                request.Headers.TryAddWithoutValidation(header.Key, resolvedValue);
            }

            // Add body content if present
            if (!string.IsNullOrEmpty(testCase.Body))
            {
                // Resolve variables in body
                string resolvedBody = fileVariables != null
                    ? VariableProcessor.ResolveVariables(testCase.Body, fileVariables) ?? testCase.Body
                    : testCase.Body;

                request.Content = new StringContent(resolvedBody);

                // Add content headers
                foreach (KeyValuePair<string, string> header in testCase.Headers)
                {
                    if (IsContentHeader(header.Key))
                    {
                        // Resolve variables in header value
                        string resolvedValue = fileVariables != null
                            ? VariableProcessor.ResolveVariables(header.Value, fileVariables) ?? header.Value
                            : header.Value;

                        request.Content.Headers.TryAddWithoutValidation(header.Key, resolvedValue);
                    }
                }
            }

            return request;
        }

        private static HttpExpectedResponse? ConvertExpectationsToResponse(HttpRequest request)
        {
            if (request.Metadata?.Expectations == null || !request.Metadata.Expectations.Any())
            {
                return null;
            }

            var response = new HttpExpectedResponse();
            var headers = new Dictionary<string, string>();

            foreach (TestExpectation expectation in request.Metadata.Expectations)
            {
                switch (expectation.Type)
                {
                    case ExpectationType.StatusCode:
                        if (int.TryParse(expectation.Value, out int statusCode))
                        {
                            response.ExpectedStatusCode = statusCode;
                        }

                        break;

                    case ExpectationType.Header:
                        // Parse "HeaderName HeaderValue" format
                        string[] headerParts = expectation.Value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (headerParts.Length == 2)
                        {
                            headers[headerParts[0]] = headerParts[1];
                        }

                        break;

                    case ExpectationType.BodyContains:
                        response.ExpectedBodyContains = expectation.Value;
                        break;

                    case ExpectationType.BodyPath:
                        response.ExpectedBodyPath = expectation.Value;
                        break;

                    case ExpectationType.Schema:
                        response.ExpectedSchemaPath = expectation.Value;
                        break;

                    case ExpectationType.MaxTime:
                        if (TryParseTimeSpan(expectation.Value, out TimeSpan timeSpan))
                        {
                            response.MaxResponseTime = timeSpan;
                        }

                        break;

                    default:
                        response.CustomExpectations[expectation.Type.ToString()] = expectation.Value;
                        break;
                }
            }

            response.ExpectedHeaders = headers;
            return response;
        }

        private static IDictionary<string, object> ConvertRequestMetadata(HttpRequest request)
        {
            var metadata = new Dictionary<string, object>();

            if (request.Metadata != null)
            {
                if (!string.IsNullOrEmpty(request.Metadata.Note))
                {
                    metadata["Note"] = request.Metadata.Note;
                }

                if (request.Metadata.NoRedirect)
                {
                    metadata["NoRedirect"] = true;
                }

                if (request.Metadata.NoCookieJar)
                {
                    metadata["NoCookieJar"] = true;
                }

                if (request.Metadata.CustomMetadata != null)
                {
                    foreach (KeyValuePair<string, string> item in request.Metadata.CustomMetadata)
                    {
                        metadata[item.Key] = item.Value;
                    }
                }
            }

            return metadata;
        }

        private static bool IsContentHeader(string headerName)
        {
            return headerName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Language", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Location", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-MD5", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Range", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Expires", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Last-Modified", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseTimeSpan(string value, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // Handle "5000ms" format
            if (value.EndsWith("ms", StringComparison.OrdinalIgnoreCase))
            {
                string msString = value.Substring(0, value.Length - 2);
                if (int.TryParse(msString, out int ms))
                {
                    timeSpan = TimeSpan.FromMilliseconds(ms);
                    return true;
                }
            }

            // Handle "5s" format
            if (value.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                string sString = value.Substring(0, value.Length - 1);
                if (int.TryParse(sString, out int s))
                {
                    timeSpan = TimeSpan.FromSeconds(s);
                    return true;
                }
            }

            // Try direct TimeSpan parsing
            return TimeSpan.TryParse(value, out timeSpan);
        }
    }
}
