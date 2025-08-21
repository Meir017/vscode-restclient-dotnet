using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RESTClient.NET.Testing.Models;
using RESTClient.NET.Testing.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace RESTClient.NET.Testing.Tests.Playground;

/// <summary>
/// Integration tests for HttpFileTestBase using the playground minimal API
/// These tests verify that HttpFileTestBase can successfully execute HTTP file tests
/// against a real ASP.NET Core minimal API
/// </summary>
public class HttpFileTestBasePlaygroundTests : HttpFileTestBase<Program>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _output;
    private static readonly Lazy<IEnumerable<object[]>> _staticTestData = new(GetStaticTestDataInternal);

    public HttpFileTestBasePlaygroundTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
    }

    protected override string GetHttpFilePath()
    {
        // Get the solution root directory by walking up from the test assembly location
        string assemblyLocation = GetType().Assembly.Location;
        var directory = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation)!);

        // Walk up until we find the solution file
        while (directory != null && directory.GetFiles("*.slnx").Length == 0)
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Could not find solution root directory");
        }

        string httpFilePath = Path.Combine(directory.FullName, "playground", "MinimalWebApi", "playground-api-tests.http");
        return httpFilePath;
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Factory.Should().NotBeNull();
        OriginalFactory.Should().NotBeNull();
        HttpFile.Should().NotBeNull();
        HttpFile.Requests.Should().NotBeEmpty();

        _output.WriteLine($"Loaded {HttpFile.Requests.Count} requests from HTTP file");
    }

    /// <summary>
    /// Gets test data for all requests in the HTTP file for use with xUnit [MemberData]
    /// </summary>
    /// <returns>Test data for xUnit [MemberData]</returns>
    public static IEnumerable<object[]> GetHttpFileTestDataStatic()
    {
        // Use cached static test data to avoid resource leaks and inefficiency
        return _staticTestData.Value;
    }

    /// <summary>
    /// Internal method to generate static test data once using a temporary instance
    /// </summary>
    private static List<object[]> GetStaticTestDataInternal()
    {
        // This is a workaround since xUnit requires static member data
        // We'll create a temporary instance to get the test data (done only once)
        using var factory = new WebApplicationFactory<Program>();
        using var tempInstance = new HttpFileTestBasePlaygroundTests(factory, new SimpleTestOutputHelper());
        return tempInstance.GetHttpFileTestData().ToList(); // ToList() to materialize the data before disposal
    }

    [Theory]
    [MemberData(nameof(GetHttpFileTestDataStatic))]
    public async Task ExecuteHttpFileTest_ShouldPassBasicValidation(HttpTestCase testCase)
    {
        // Arrange
        _output.WriteLine($"Testing: {testCase.Name} - {testCase.Method} {testCase.Url}");

        using HttpClient client = Factory.CreateClient();

        // Get the base URL from the test server and process the request with environment variables
        string baseUrl = client.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost";
        var environmentVariables = new Dictionary<string, string>
        {
            ["baseUrl"] = baseUrl,
            ["contentType"] = "application/json"
        };

        // Get the processed request with environment variables applied
        Core.Models.HttpRequest? processedRequest = GetProcessedRequest(testCase.Name, environmentVariables);
        processedRequest.Should().NotBeNull();

        // Create HttpRequestMessage from processed request
        using var request = new HttpRequestMessage(new HttpMethod(processedRequest!.Method), processedRequest.Url);

        // Add headers
        foreach (KeyValuePair<string, string> header in processedRequest.Headers)
        {
            // Handle content headers separately
            if (TestHttpUtils.IsContentHeader(header.Key))
            {
                continue; // Will be added when we set content
            }
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Add body content if present
        if (!string.IsNullOrEmpty(processedRequest.Body))
        {
            // Get the content type from headers for proper StringContent creation
            string contentType = processedRequest.Headers.TryGetValue("Content-Type", out string? ctValue)
                ? ctValue
                : "text/plain";

            // Create StringContent with proper content type to avoid HTTP 415 errors
            request.Content = TestHttpUtils.CreateHttpContent(processedRequest.Body, contentType);

            // Add other content headers (excluding Content-Type which is already set)
            foreach (KeyValuePair<string, string> header in processedRequest.Headers)
            {
                if (TestHttpUtils.IsContentHeader(header.Key) && !header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);

        // Log response for debugging
        string responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response: {(int)response.StatusCode} {response.StatusCode}");
        _output.WriteLine($"Content: {responseContent}");

        // Assert - Basic status code validation
        if (testCase.ExpectedResponse?.ExpectedStatusCode != null)
        {
            ((int)response.StatusCode).Should().Be(testCase.ExpectedResponse.ExpectedStatusCode.Value);
        }

        // Assert - Content validation if expected
        if (!string.IsNullOrEmpty(testCase.ExpectedResponse?.ExpectedBodyContains))
        {
            responseContent.Should().Contain(testCase.ExpectedResponse.ExpectedBodyContains);
        }
    }

    [Fact]
    public async Task ExecutePingTest_ShouldReturnPong()
    {
        // Arrange
        HttpTestCase testCase = GetTestCase("ping-test");
        using HttpClient client = Factory.CreateClient();

        // Get the base URL from the test server and process the request with environment variables
        string baseUrl = client.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost";
        var environmentVariables = new Dictionary<string, string>
        {
            ["baseUrl"] = baseUrl,
            ["contentType"] = "application/json"
        };

        // Get the processed request with environment variables applied
        Core.Models.HttpRequest? processedRequest = GetProcessedRequest("ping-test", environmentVariables);
        processedRequest.Should().NotBeNull();

        // Create HttpRequestMessage from processed request
        using var request = new HttpRequestMessage(new HttpMethod(processedRequest!.Method), processedRequest.Url);

        // Add headers
        foreach (KeyValuePair<string, string> header in processedRequest.Headers)
        {
            // Handle content headers separately
            if (TestHttpUtils.IsContentHeader(header.Key))
            {
                continue; // Will be added when we set content
            }
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Add body content if present
        if (!string.IsNullOrEmpty(processedRequest.Body))
        {
            request.Content = TestHttpUtils.CreateHttpContent(processedRequest.Body);

            // Add content headers
            foreach (KeyValuePair<string, string> header in processedRequest.Headers)
            {
                if (TestHttpUtils.IsContentHeader(header.Key))
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("pong");
    }

    [Fact]
    public async Task ExecuteGetTest_ShouldReturnExpectedContent()
    {
        // Arrange
        HttpTestCase testCase = GetTestCase("get-test");
        using HttpClient client = Factory.CreateClient();

        // Get the base URL from the test server and process the request with environment variables
        string baseUrl = client.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost";
        var environmentVariables = new Dictionary<string, string>
        {
            ["baseUrl"] = baseUrl,
            ["contentType"] = "application/json"
        };

        // Get the processed request with environment variables applied
        Core.Models.HttpRequest? processedRequest = GetProcessedRequest("get-test", environmentVariables);
        processedRequest.Should().NotBeNull();

        // Create HttpRequestMessage from processed request
        using var request = new HttpRequestMessage(new HttpMethod(processedRequest!.Method), processedRequest.Url);

        // Add headers
        foreach (KeyValuePair<string, string> header in processedRequest.Headers)
        {
            // Handle content headers separately
            if (TestHttpUtils.IsContentHeader(header.Key))
            {
                continue; // Will be added when we set content
            }
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Add body content if present
        if (!string.IsNullOrEmpty(processedRequest.Body))
        {
            request.Content = TestHttpUtils.CreateHttpContent(processedRequest.Body);

            // Add content headers
            foreach (KeyValuePair<string, string> header in processedRequest.Headers)
            {
                if (TestHttpUtils.IsContentHeader(header.Key))
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Hello, World!");
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task ExecuteHeadersTest_ShouldReturnCustomHeaders()
    {
        // Arrange
        HttpTestCase testCase = GetTestCase("headers-test");
        using HttpClient client = Factory.CreateClient();

        // Get the base URL from the test server and process the request with environment variables
        string baseUrl = client.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost";
        var environmentVariables = new Dictionary<string, string>
        {
            ["baseUrl"] = baseUrl,
            ["contentType"] = "application/json"
        };

        // Get the processed request with environment variables applied
        Core.Models.HttpRequest? processedRequest = GetProcessedRequest("headers-test", environmentVariables);
        processedRequest.Should().NotBeNull();

        // Create HttpRequestMessage from processed request
        using var request = new HttpRequestMessage(new HttpMethod(processedRequest!.Method), processedRequest.Url);

        // Add headers
        foreach (KeyValuePair<string, string> header in processedRequest.Headers)
        {
            // Handle content headers separately
            if (TestHttpUtils.IsContentHeader(header.Key))
            {
                continue; // Will be added when we set content
            }
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Add body content if present
        if (!string.IsNullOrEmpty(processedRequest.Body))
        {
            request.Content = TestHttpUtils.CreateHttpContent(processedRequest.Body);

            // Add content headers
            foreach (KeyValuePair<string, string> header in processedRequest.Headers)
            {
                if (TestHttpUtils.IsContentHeader(header.Key))
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Act
        using HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Headers test");
        content.Should().Contain("Custom-Header"); // Should contain our custom header in the response
    }

    [Fact]
    public void GetFilteredTestData_WithNamePattern_ShouldFilterCorrectly()
    {
        // Arrange & Act
        IEnumerable<object[]> filteredData = GetFilteredTestData(namePattern: "get-*");

        // Assert
        filteredData.Should().NotBeEmpty();

        // All filtered test cases should start with "get-"
        foreach (object[] item in filteredData)
        {
            var testCase = (HttpTestCase)item[0];
            testCase.Name.Should().StartWith("get-");
        }
    }

    [Fact]
    public void GetProcessedRequest_WithEnvironmentVariables_ShouldApplyVariables()
    {
        // Arrange
        var environmentVariables = new Dictionary<string, string>
        {
            ["baseUrl"] = "https://custom.example.com"
        };

        // Act
        Core.Models.HttpRequest? processedRequest = GetProcessedRequest("get-test", environmentVariables);

        // Assert
        processedRequest.Should().NotBeNull();
        processedRequest!.Url.Should().Contain("custom.example.com");
    }

    [Fact]
    public void TryGetTestCase_WithValidName_ShouldReturnTrue()
    {
        // Act
        bool found = TryGetTestCase("ping-test", out HttpTestCase? testCase);

        // Assert
        found.Should().BeTrue();
        testCase.Should().NotBeNull();
        testCase.Name.Should().Be("ping-test");
    }

    [Fact]
    public void TryGetTestCase_WithInvalidName_ShouldReturnFalse()
    {
        // Act
        bool found = TryGetTestCase("nonexistent-test", out HttpTestCase? testCase);

        // Assert
        found.Should().BeFalse();
        testCase.Should().BeNull();
    }
}

/// <summary>
/// Simple test output helper for use in static contexts
/// </summary>
internal sealed class SimpleTestOutputHelper : ITestOutputHelper
{
    public void WriteLine(string message) { }
    public void WriteLine(string format, params object[] args) { }
}
