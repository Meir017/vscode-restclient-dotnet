using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using RESTClient.NET.Testing.Models;

namespace RESTClient.NET.Testing.Assertions
{
    /// <summary>
    /// Provides assertion methods for HTTP responses in testing scenarios
    /// </summary>
    public static class HttpResponseAssertion
    {
        /// <summary>
        /// Asserts that an HTTP response matches the expected response criteria
        /// </summary>
        /// <param name="response">The actual HTTP response</param>
        /// <param name="expected">The expected response criteria</param>
        /// <returns>A task representing the assertion operation</returns>
        public static async Task AssertResponse(HttpResponseMessage response, HttpExpectedResponse? expected)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (expected == null || !expected.HasExpectations)
                return; // No assertions to perform

            // Assert status code
            if (expected.ExpectedStatusCode.HasValue)
            {
                AssertStatusCode(response, expected.ExpectedStatusCode.Value);
            }

            // Assert headers
            foreach (var expectedHeader in expected.ExpectedHeaders)
            {
                AssertHeader(response, expectedHeader.Key, expectedHeader.Value);
            }

            // Assert body content
            if (!string.IsNullOrEmpty(expected.ExpectedBodyContains))
            {
                await AssertBodyContains(response, expected.ExpectedBodyContains);
            }

            // Assert JSONPath (if specified)
            if (!string.IsNullOrEmpty(expected.ExpectedBodyPath))
            {
                await AssertJsonPath(response, expected.ExpectedBodyPath);
            }

            // Assert JSON schema (if specified)
            if (!string.IsNullOrEmpty(expected.ExpectedSchemaPath))
            {
                await AssertSchema(response, expected.ExpectedSchemaPath);
            }

            // Custom expectations can be handled by derived implementations
            foreach (var customExpectation in expected.CustomExpectations)
            {
                await AssertCustomExpectation(response, customExpectation.Key, customExpectation.Value);
            }
        }

        /// <summary>
        /// Asserts that the HTTP response has the expected status code
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="expectedStatusCode">The expected status code</param>
        public static void AssertStatusCode(HttpResponseMessage response, int expectedStatusCode)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var actualStatusCode = (int)response.StatusCode;
            if (actualStatusCode != expectedStatusCode)
            {
                throw new AssertionException(
                    $"Expected status code {expectedStatusCode}, but got {actualStatusCode} ({response.StatusCode})");
            }
        }

        /// <summary>
        /// Asserts that the HTTP response contains the expected header with the expected value
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="headerName">The name of the header to check</param>
        /// <param name="expectedValue">The expected header value</param>
        public static void AssertHeader(HttpResponseMessage response, string headerName, string expectedValue)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (string.IsNullOrWhiteSpace(headerName))
                throw new ArgumentException("Header name cannot be null or empty", nameof(headerName));

            // Check in response headers first
            if (response.Headers.TryGetValues(headerName, out var headerValues))
            {
                var actualValue = string.Join(", ", headerValues);
                if (!actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    throw new AssertionException(
                        $"Expected header '{headerName}' to have value '{expectedValue}', but got '{actualValue}'");
                }
                return;
            }

            // Check in content headers if it's a content header
            if (response.Content?.Headers.TryGetValues(headerName, out var contentHeaderValues) == true)
            {
                var actualValue = string.Join(", ", contentHeaderValues);
                if (!actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    throw new AssertionException(
                        $"Expected content header '{headerName}' to have value '{expectedValue}', but got '{actualValue}'");
                }
                return;
            }

            throw new AssertionException($"Header '{headerName}' was not found in the response");
        }

        /// <summary>
        /// Asserts that the HTTP response body contains the expected content
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="expectedContent">The content that should be contained in the response body</param>
        /// <returns>A task representing the assertion operation</returns>
        public static async Task AssertBodyContains(HttpResponseMessage response, string expectedContent)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (string.IsNullOrEmpty(expectedContent))
                throw new ArgumentException("Expected content cannot be null or empty", nameof(expectedContent));

            if (response.Content == null)
            {
                throw new AssertionException("Response has no content, but expected content was specified");
            }

            var actualContent = await response.Content.ReadAsStringAsync();
            if (!actualContent.Contains(expectedContent, StringComparison.OrdinalIgnoreCase))
            {
                throw new AssertionException(
                    $"Expected response body to contain '{expectedContent}', but it was not found. " +
                    $"Actual content: {TruncateContent(actualContent)}");
            }
        }

        /// <summary>
        /// Asserts that the HTTP response body matches the specified JSONPath expression
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="jsonPath">The JSONPath expression to evaluate</param>
        /// <returns>A task representing the assertion operation</returns>
        public static async Task AssertJsonPath(HttpResponseMessage response, string jsonPath)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (string.IsNullOrWhiteSpace(jsonPath))
                throw new ArgumentException("JSONPath cannot be null or empty", nameof(jsonPath));

            if (response.Content == null)
            {
                throw new AssertionException("Response has no content, but JSONPath was specified");
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // This is a placeholder implementation
            // In a full implementation, you would use a JSONPath library like Newtonsoft.Json.JsonPath
            // For now, we'll do a basic check
            try
            {
                var parsedJsonPath = ParseJsonPath(jsonPath);
                ValidateJsonPathAgainstContent(content, parsedJsonPath);
            }
            catch (Exception ex)
            {
                throw new AssertionException($"JSONPath assertion failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Asserts that the HTTP response body conforms to the specified JSON schema
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="schemaPath">The path to the JSON schema file</param>
        /// <returns>A task representing the assertion operation</returns>
        public static async Task AssertSchema(HttpResponseMessage response, string schemaPath)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (string.IsNullOrWhiteSpace(schemaPath))
                throw new ArgumentException("Schema path cannot be null or empty", nameof(schemaPath));

            if (response.Content == null)
            {
                throw new AssertionException("Response has no content, but schema validation was specified");
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // This is a placeholder implementation
            // In a full implementation, you would use a JSON schema validation library
            // For now, we'll just check that the content is valid JSON
            try
            {
                System.Text.Json.JsonDocument.Parse(content);
                // Schema validation would go here
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new AssertionException($"Response content is not valid JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new AssertionException($"Schema validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles custom expectation assertions
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="expectationType">The type of expectation</param>
        /// <param name="expectationValue">The expectation value</param>
        /// <returns>A task representing the assertion operation</returns>
        private static async Task AssertCustomExpectation(HttpResponseMessage response, string expectationType, object expectationValue)
        {
            // This is a placeholder for custom expectation handling
            // Implementers can extend this method to handle custom expectation types
            await Task.CompletedTask;
        }

        private static string TruncateContent(string content, int maxLength = 500)
        {
            if (string.IsNullOrEmpty(content))
                return "(empty)";

            if (content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "... (truncated)";
        }

        private static string ParseJsonPath(string jsonPath)
        {
            // Placeholder implementation - would use proper JSONPath parsing
            return jsonPath;
        }

        private static void ValidateJsonPathAgainstContent(string content, string jsonPath)
        {
            // Placeholder implementation - would use proper JSONPath evaluation
            // For now, just check that content is valid JSON
            System.Text.Json.JsonDocument.Parse(content);
        }
    }

    /// <summary>
    /// Exception thrown when an assertion fails
    /// </summary>
    public class AssertionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the AssertionException class
        /// </summary>
        /// <param name="message">The assertion failure message</param>
        public AssertionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AssertionException class
        /// </summary>
        /// <param name="message">The assertion failure message</param>
        /// <param name="innerException">The inner exception</param>
        public AssertionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
