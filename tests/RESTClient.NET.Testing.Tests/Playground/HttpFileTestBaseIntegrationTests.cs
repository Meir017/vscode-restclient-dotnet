using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Testing.Extensions;
using RESTClient.NET.Testing.Models;
using Xunit;
using Xunit.Abstractions;

namespace RESTClient.NET.Testing.Tests.Playground;

/// <summary>
/// Integration tests for HttpFileTestBase using the Playground MinimalWebApi
/// These tests provide actual coverage for the HttpFileTestBase class
/// </summary>
public class HttpFileTestBaseIntegrationTests : HttpFileTestBase<Program>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _output;
    private static readonly string[] _postPutMethods = { "POST", "PUT" };

    public HttpFileTestBaseIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
    }

    protected override string GetHttpFilePath() => "Integration/test-integration.http";

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Factory.Should().NotBeNull();
        OriginalFactory.Should().NotBeNull();
        HttpFile.Should().NotBeNull();
        HttpFile.Requests.Should().NotBeEmpty();
        HttpFile.FileVariables.Should().ContainKey("baseUrl");
        HttpFile.FileVariables.Should().ContainKey("contentType");
    }

    [Fact]
    public void HttpFile_Property_ShouldReturnParsedFile()
    {
        // Act & Assert
        HttpFile.Should().NotBeNull();
        HttpFile.Requests.Should().HaveCountGreaterThan(5); // We have 9 requests in the file
        HttpFile.FileVariables.Should().NotBeEmpty();
    }

    [Fact]
    public void Factory_Property_ShouldReturnConfiguredFactory()
    {
        // Act & Assert
        Factory.Should().NotBeNull();
        Factory.Should().NotBeSameAs(OriginalFactory); // Should be configured, not original
    }

    [Fact]
    public void OriginalFactory_Property_ShouldReturnOriginalFactory()
    {
        // Act & Assert
        OriginalFactory.Should().NotBeNull();
    }

    [Fact]
    public void GetHttpFileTestData_ShouldReturnValidTestData()
    {
        // Act
        var testData = GetHttpFileTestData().ToList();

        // Assert
        testData.Should().NotBeEmpty();
        testData.Should().HaveCountGreaterThan(5);
        testData.Should().OnlyContain(data => data.Length == 1);
        testData.Should().OnlyContain(data => data[0] is HttpTestCase);

        var testCases = testData.Select(data => (HttpTestCase)data[0]).ToList();
        testCases.Should().OnlyContain(tc => !string.IsNullOrEmpty(tc.Name));
        testCases.Should().OnlyContain(tc => !string.IsNullOrEmpty(tc.Method));
        testCases.Should().OnlyContain(tc => !string.IsNullOrEmpty(tc.Url));
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void HttpFileTestData_ShouldHaveValidStructure(HttpTestCase testCase)
    {
        // Assert
        testCase.Should().NotBeNull();
        testCase.Name.Should().NotBeNullOrEmpty();
        testCase.Method.Should().NotBeNullOrEmpty();
        testCase.Url.Should().NotBeNullOrEmpty();
        testCase.Headers.Should().NotBeNull();
        testCase.Metadata.Should().NotBeNull();

        _output.WriteLine($"Test case: {testCase.Name} - {testCase.Method} {testCase.Url}");
    }

    // Static method for MemberData - creates a temporary instance to get test data
    public static IEnumerable<object[]> GetTestCases()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var tempInstance = new HttpFileTestBaseIntegrationTests(factory, new NullTestOutputHelper());
        return tempInstance.GetHttpFileTestData();
    }

    // Simple test output helper for static method
    private sealed class NullTestOutputHelper : ITestOutputHelper
    {
        public void WriteLine(string message) { }
        public void WriteLine(string format, params object[] args) { }
    }

    [Fact]
    public void GetTestCase_WithValidName_ShouldReturnTestCase()
    {
        // Act
        HttpTestCase testCase = GetTestCase("get-test");

        // Assert
        testCase.Should().NotBeNull();
        testCase.Name.Should().Be("get-test");
        testCase.Method.Should().Be("GET");
        testCase.Url.Should().Contain("/api/test");
    }

    [Fact]
    public void GetTestCase_WithInvalidName_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        Func<HttpTestCase> action = () => GetTestCase("non-existent-request");
        action.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void TryGetTestCase_WithValidName_ShouldReturnTrueAndTestCase()
    {
        // Act
        bool result = TryGetTestCase("create-test", out HttpTestCase? testCase);

        // Assert
        result.Should().BeTrue();
        testCase.Should().NotBeNull();
        testCase.Name.Should().Be("create-test");
        testCase.Method.Should().Be("POST");
    }

    [Fact]
    public void TryGetTestCase_WithInvalidName_ShouldReturnFalse()
    {
        // Act
        bool result = TryGetTestCase("non-existent-request", out HttpTestCase? testCase);

        // Assert
        result.Should().BeFalse();
        testCase.Should().BeNull();
    }

    [Fact]
    public void GetFilteredTestData_WithNamePattern_ShouldFilterCorrectly()
    {
        // Act
        var filteredData = GetFilteredTestData(namePattern: "get").ToList();

        // Assert
        filteredData.Should().NotBeEmpty();
        var testCases = filteredData.Select(data => (HttpTestCase)data[0]).ToList();
        testCases.Should().OnlyContain(tc => tc.Name.Contains("get", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetFilteredTestData_WithMethods_ShouldFilterCorrectly()
    {
        // Act
        var filteredData = GetFilteredTestData(methods: _postPutMethods).ToList();

        // Assert
        filteredData.Should().NotBeEmpty();
        var testCases = filteredData.Select(data => (HttpTestCase)data[0]).ToList();
        testCases.Should().OnlyContain(tc => tc.Method == "POST" || tc.Method == "PUT");
    }

    [Fact]
    public void GetFilteredTestData_WithHasExpectations_ShouldFilterCorrectly()
    {
        // Act
        var withExpectations = GetFilteredTestData(hasExpectations: true).ToList();
        var withoutExpectations = GetFilteredTestData(hasExpectations: false).ToList();

        // Assert
        withExpectations.Should().NotBeEmpty();
        withoutExpectations.Should().NotBeEmpty();

        var testCasesWithExpectations = withExpectations.Select(data => (HttpTestCase)data[0]).ToList();
        var testCasesWithoutExpectations = withoutExpectations.Select(data => (HttpTestCase)data[0]).ToList();

        testCasesWithExpectations.Should().OnlyContain(tc => tc.ExpectedResponse != null && tc.ExpectedResponse.HasExpectations);
        testCasesWithoutExpectations.Should().OnlyContain(tc => tc.ExpectedResponse == null || !tc.ExpectedResponse.HasExpectations);
    }

    [Fact]
    public void GetProcessedRequest_WithValidName_ShouldReturnProcessedRequest()
    {
        // Arrange
        var environmentVariables = new Dictionary<string, string>
        {
            { "baseUrl", "https://override.example.com" }
        };

        // Act
        HttpRequest? processedRequest = GetProcessedRequest("get-test", environmentVariables);

        // Assert
        processedRequest.Should().NotBeNull();
        // Environment variables take precedence over file variables
        processedRequest.Url.Should().Contain("https://override.example.com/api/test");
    }

    [Fact]
    public void GetProcessedRequest_WithInvalidName_ShouldReturnNull()
    {
        // Act
        HttpRequest? processedRequest = GetProcessedRequest("non-existent-request");

        // Assert
        processedRequest.Should().BeNull();
    }

    [Fact]
    public void GetProcessedRequest_WithoutEnvironmentVariables_ShouldUseFileVariables()
    {
        // Act
        HttpRequest? processedRequest = GetProcessedRequest("get-test");

        // Assert
        processedRequest.Should().NotBeNull();
        processedRequest.Url.Should().Contain("http://localhost/api/test");
        processedRequest.Headers.Should().ContainKey("Accept");
        processedRequest.Headers["Accept"].Should().Be("application/json");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public void GetFilteredTestData_WithSpecificMethod_ShouldReturnMatchingTests(string method)
    {
        // Act
        var filteredData = GetFilteredTestData(methods: new[] { method }).ToList();

        // Assert
        if (method == "GET")
        {
            filteredData.Should().NotBeEmpty(); // We have GET requests
        }

        var testCases = filteredData.Select(data => (HttpTestCase)data[0]).ToList();
        testCases.Should().OnlyContain(tc => tc.Method == method);
    }

    [Fact]
    public async Task ToHttpRequestMessage_Integration_ShouldCreateValidRequest()
    {
        // Arrange
        HttpTestCase testCase = GetTestCase("get-test");
        using HttpClient client = Factory.CreateClient();

        // Act
        using var request = testCase.ToHttpRequestMessage();
        using HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        request.Should().NotBeNull();
        request.Method.ToString().Should().Be("GET");
        request.RequestUri.Should().NotBeNull();

        response.Should().NotBeNull();
        // The actual response validation would depend on your test server
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // Add test-specific services or replace real services with mocks
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
    }

    protected override void ModifyHttpFile(HttpFile httpFile)
    {
        // Modify HTTP file if needed for testing
        // This method should be called during initialization
        httpFile.Should().NotBeNull();
    }
}
