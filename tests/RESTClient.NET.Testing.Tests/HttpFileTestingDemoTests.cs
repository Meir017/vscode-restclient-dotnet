using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using RESTClient.NET.Core;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Parsing;
using RESTClient.NET.Testing.Extensions;
using RESTClient.NET.Testing.Models;
using RESTClient.NET.Testing.Assertions;
using Xunit;

namespace RESTClient.NET.Testing.Tests;

/// <summary>
/// Demonstrates testing framework capabilities with unit tests.
/// These tests validate the testing framework functionality without requiring a web application.
/// </summary>
public class HttpFileTestingDemoTests
{
    private readonly HttpFileParser _parser;
    private readonly string _httpFileContent;

    public HttpFileTestingDemoTests()
    {
        _parser = new HttpFileParser();
        _httpFileContent = File.ReadAllText("test-requests.http");
    }

    [Fact]
    public void HttpFile_ShouldLoadCorrectly()
    {
        // Arrange & Act
        var httpFile = _parser.Parse(_httpFileContent);

        // Assert
        httpFile.Should().NotBeNull();
        httpFile.Requests.Should().NotBeEmpty();
        httpFile.Requests.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void GetTestCases_ShouldReturnCorrectTestCases()
    {
        // Arrange
        var httpFile = _parser.Parse(_httpFileContent);

        // Act
        var testCases = httpFile.GetTestCases().ToList();

        // Assert
        testCases.Should().NotBeEmpty();
        testCases.Should().OnlyContain(tc => !string.IsNullOrEmpty(tc.Name));
        testCases.Should().OnlyContain(tc => !string.IsNullOrEmpty(tc.Method));
        testCases.Should().OnlyContain(tc => !string.IsNullOrEmpty(tc.Url));
    }

    [Fact]
    public void GetTestData_ShouldReturnXUnitCompatibleData()
    {
        // Arrange
        var httpFile = _parser.Parse(_httpFileContent);

        // Act
        var testData = httpFile.GetTestData().ToList();

        // Assert
        testData.Should().NotBeEmpty();
        testData.Should().OnlyContain(data => data.Length == 1);
        testData.Should().OnlyContain(data => data[0] is HttpTestCase);
    }

    [Fact]
    public void FilterTestCases_ShouldWorkCorrectly()
    {
        // Arrange
        var httpFile = _parser.Parse(_httpFileContent);
        var allTestCases = httpFile.GetTestCases().ToList();

        // Act
        var getRequests = allTestCases.Where(tc => tc.Method == "GET").ToList();
        var postRequests = allTestCases.Where(tc => tc.Method == "POST").ToList();

        // Assert
        getRequests.Should().NotBeEmpty();
        postRequests.Should().NotBeEmpty();
        (getRequests.Count + postRequests.Count).Should().Be(allTestCases.Count);
    }

    [Fact]
    public void FilterTestCases_ByExpectations_ShouldWorkCorrectly()
    {
        // Arrange
        var httpFile = _parser.Parse(_httpFileContent);
        var testCases = httpFile.GetTestCases().ToList();

        // Act
        var casesWithExpectations = testCases
            .Where(tc => tc.ExpectedResponse != null)
            .ToList();

        // Assert
        casesWithExpectations.Should().NotBeEmpty();
        casesWithExpectations.Should().OnlyContain(tc => tc.ExpectedResponse != null);
    }

    [Theory]
    [MemberData(nameof(GetHttpFileTestData))]
    public void TestHttpFileRequests_ShouldHaveValidStructure(HttpTestCase testCase)
    {
        // Arrange & Act & Assert
        testCase.Should().NotBeNull();
        testCase.Name.Should().NotBeNullOrEmpty();
        testCase.Method.Should().NotBeNullOrEmpty();
        testCase.Url.Should().NotBeNullOrEmpty();
        testCase.Headers.Should().NotBeNull();
    }

    [Fact]
    public void ToHttpRequestMessage_ShouldCreateValidRequest()
    {
        // Arrange
        var httpFile = _parser.Parse(_httpFileContent);
        var testCase = httpFile.GetTestCases().First();

        // Act
        var requestMessage = testCase.ToHttpRequestMessage();

        // Assert
        requestMessage.Should().NotBeNull();
        requestMessage.Method.ToString().Should().Be(testCase.Method);
        requestMessage.RequestUri.Should().NotBeNull();
    }

    public static IEnumerable<object[]> GetHttpFileTestData()
    {
        var parser = new HttpFileParser();
        var content = File.ReadAllText("test-requests.http");
        var httpFile = parser.Parse(content);
        return httpFile.GetTestData();
    }
}

/// <summary>
/// Unit tests for the testing framework components
/// </summary>
public class TestingFrameworkUnitTests
{
    [Fact]
    public void HttpTestCase_DefaultConstructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var testCase = new HttpTestCase();

        // Assert
        testCase.Name.Should().BeEmpty();
        testCase.Method.Should().BeEmpty();
        testCase.Url.Should().BeEmpty();
        testCase.Headers.Should().NotBeNull();
        testCase.Headers.Should().BeEmpty();
        testCase.Body.Should().BeNull();
        testCase.ExpectedResponse.Should().BeNull();
        testCase.Metadata.Should().NotBeNull();
        testCase.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void HttpExpectedResponse_HasExpectations_ShouldReturnCorrectValue()
    {
        // Arrange
        var emptyResponse = new HttpExpectedResponse();
        var responseWithStatus = new HttpExpectedResponse { ExpectedStatusCode = 200 };

        // Act & Assert
        emptyResponse.HasExpectations.Should().BeFalse();
        responseWithStatus.HasExpectations.Should().BeTrue();
    }

    [Fact]
    public void AssertStatusCode_WithMatchingStatus_ShouldNotThrow()
    {
        // Arrange
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        // Act & Assert
        var action = () => HttpResponseAssertion.AssertStatusCode(response, 200);
        action.Should().NotThrow();
    }

    [Fact]
    public void AssertStatusCode_WithMismatchedStatus_ShouldThrow()
    {
        // Arrange
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);

        // Act & Assert
        var action = () => HttpResponseAssertion.AssertStatusCode(response, 200);
        action.Should().Throw<AssertionException>()
            .WithMessage("Expected status code 200, but got 404 (NotFound)");
    }

    [Fact]
    public async Task AssertBodyContains_WithMatchingContent_ShouldNotThrow()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            Content = new StringContent("Hello, World!")
        };

        // Act & Assert
        var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "World");
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AssertBodyContains_WithMismatchedContent_ShouldThrow()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            Content = new StringContent("Hello, World!")
        };

        // Act & Assert
        var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "Universe");
        await action.Should().ThrowAsync<AssertionException>()
            .Where(ex => ex.Message.Contains("Expected response body to contain 'Universe'"));
    }
}
