using System.Collections.Generic;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents metadata and settings for an HTTP request
    /// </summary>
    public class HttpRequestMetadata
    {
        /// <summary>
        /// Gets or sets the human-readable name of the request
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a note or description for the request
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to follow redirects
        /// </summary>
        public bool NoRedirect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to save cookies
        /// </summary>
        public bool NoCookieJar { get; set; }

        /// <summary>
        /// Gets custom metadata properties
        /// </summary>
        public IDictionary<string, string> CustomMetadata { get; }

        /// <summary>
        /// Gets the test expectations for this request
        /// </summary>
        public IList<TestExpectation> Expectations { get; }

        /// <summary>
        /// Initializes a new instance of the HttpRequestMetadata class
        /// </summary>
        public HttpRequestMetadata()
        {
            CustomMetadata = new Dictionary<string, string>();
            Expectations = [];
        }

        /// <summary>
        /// Adds a test expectation to this request
        /// </summary>
        /// <param name="expectation">The test expectation to add</param>
        public void AddExpectation(TestExpectation expectation)
        {
            Expectations.Add(expectation);
        }

        /// <summary>
        /// Gets expectations of a specific type
        /// </summary>
        /// <param name="type">The expectation type to filter by</param>
        /// <returns>A collection of expectations of the specified type</returns>
        public IEnumerable<TestExpectation> GetExpectations(ExpectationType type)
        {
            foreach (TestExpectation expectation in Expectations)
            {
                if (expectation.Type == type)
                {
                    yield return expectation;
                }
            }
        }

        /// <summary>
        /// Checks if the request has any expectations
        /// </summary>
        /// <returns>true if the request has expectations; otherwise, false</returns>
        public bool HasExpectations()
        {
            return Expectations.Count > 0;
        }
    }

    /// <summary>
    /// Represents a test expectation for an HTTP request
    /// </summary>
    public class TestExpectation
    {
        /// <summary>
        /// Gets or sets the type of expectation
        /// </summary>
        public ExpectationType Type { get; set; }

        /// <summary>
        /// Gets or sets the expected value or pattern
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional context or parameters for the expectation
        /// </summary>
        public string? Context { get; set; }

        /// <summary>
        /// Initializes a new instance of the TestExpectation class
        /// </summary>
        /// <param name="type">The expectation type</param>
        /// <param name="value">The expected value</param>
        /// <param name="context">Additional context</param>
        public TestExpectation(ExpectationType type, string value, string? context = null)
        {
            Type = type;
            Value = value;
            Context = context;
        }
    }

    /// <summary>
    /// Defines the types of test expectations
    /// </summary>
    public enum ExpectationType
    {
        /// <summary>
        /// Expect a specific HTTP status code
        /// </summary>
        StatusCode,

        /// <summary>
        /// Expect a specific header value
        /// </summary>
        Header,

        /// <summary>
        /// Expect the response body to contain specific text
        /// </summary>
        BodyContains,

        /// <summary>
        /// Expect a specific value at a JSON path
        /// </summary>
        BodyPath,

        /// <summary>
        /// Expect the response to match a JSON schema
        /// </summary>
        Schema,

        /// <summary>
        /// Expect the response time to be below a threshold
        /// </summary>
        MaxTime
    }
}
