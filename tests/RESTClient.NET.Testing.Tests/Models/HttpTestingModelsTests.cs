using System;
using System.Collections.Generic;
using AwesomeAssertions;
using RESTClient.NET.Testing.Models;
using Xunit;

namespace RESTClient.NET.Testing.Tests.Models
{
    public class HttpTestCaseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeCorrectly()
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
            testCase.LineNumber.Should().Be(0);
        }

        [Fact]
        public void Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var testCase = new HttpTestCase();
            var headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } };
            var metadata = new Dictionary<string, object> { { "Note", "Test note" } };
            var expectedResponse = new HttpExpectedResponse { ExpectedStatusCode = 200 };

            // Act
            testCase.Name = "test-name";
            testCase.Method = "POST";
            testCase.Url = "http://localhost:5000/api/test";
            testCase.Headers = headers;
            testCase.Body = @"{""test"": true}";
            testCase.ExpectedResponse = expectedResponse;
            testCase.Metadata = metadata;
            testCase.LineNumber = 42;

            // Assert
            testCase.Name.Should().Be("test-name");
            testCase.Method.Should().Be("POST");
            testCase.Url.Should().Be("http://localhost:5000/api/test");
            testCase.Headers.Should().BeSameAs(headers);
            testCase.Body.Should().Be(@"{""test"": true}");
            testCase.ExpectedResponse.Should().BeSameAs(expectedResponse);
            testCase.Metadata.Should().BeSameAs(metadata);
            testCase.LineNumber.Should().Be(42);
        }
    }

    public class HttpExpectedResponseTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var response = new HttpExpectedResponse();

            // Assert
            response.ExpectedStatusCode.Should().BeNull();
            response.ExpectedHeaders.Should().NotBeNull();
            response.ExpectedHeaders.Should().BeEmpty();
            response.ExpectedBodyContains.Should().BeNull();
            response.ExpectedBodyPath.Should().BeNull();
            response.ExpectedSchemaPath.Should().BeNull();
            response.MaxResponseTime.Should().BeNull();
            response.CustomExpectations.Should().NotBeNull();
            response.CustomExpectations.Should().BeEmpty();
        }

        [Fact]
        public void HasExpectations_WithNoExpectations_ShouldReturnFalse()
        {
            // Arrange
            var response = new HttpExpectedResponse();

            // Act & Assert
            response.HasExpectations.Should().BeFalse();
        }

        [Fact]
        public void HasExpectations_WithStatusCode_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse { ExpectedStatusCode = 200 };

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }

        [Fact]
        public void HasExpectations_WithHeaders_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse
            {
                ExpectedHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }

        [Fact]
        public void HasExpectations_WithBodyContains_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse { ExpectedBodyContains = "success" };

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }

        [Fact]
        public void HasExpectations_WithBodyPath_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse { ExpectedBodyPath = "$.status" };

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }

        [Fact]
        public void HasExpectations_WithSchemaPath_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse { ExpectedSchemaPath = "/schemas/user.json" };

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }

        [Fact]
        public void HasExpectations_WithMaxResponseTime_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse { MaxResponseTime = TimeSpan.FromSeconds(5) };

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }

        [Fact]
        public void HasExpectations_WithCustomExpectations_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse();
            response.CustomExpectations["custom"] = "value";

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }

        [Fact]
        public void Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var response = new HttpExpectedResponse();
            var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
            var customExpectations = new Dictionary<string, object> { { "custom", "value" } };
            var maxTime = TimeSpan.FromSeconds(10);

            // Act
            response.ExpectedStatusCode = 201;
            response.ExpectedHeaders = headers;
            response.ExpectedBodyContains = "created";
            response.ExpectedBodyPath = "$.id";
            response.ExpectedSchemaPath = "/schemas/created.json";
            response.MaxResponseTime = maxTime;
            response.CustomExpectations = customExpectations;

            // Assert
            response.ExpectedStatusCode.Should().Be(201);
            response.ExpectedHeaders.Should().BeSameAs(headers);
            response.ExpectedBodyContains.Should().Be("created");
            response.ExpectedBodyPath.Should().Be("$.id");
            response.ExpectedSchemaPath.Should().Be("/schemas/created.json");
            response.MaxResponseTime.Should().Be(maxTime);
            response.CustomExpectations.Should().BeSameAs(customExpectations);
        }

        [Fact]
        public void HasExpectations_WithMultipleExpectations_ShouldReturnTrue()
        {
            // Arrange
            var response = new HttpExpectedResponse
            {
                ExpectedStatusCode = 200,
                ExpectedBodyContains = "success",
                MaxResponseTime = TimeSpan.FromSeconds(5),
                ExpectedHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                CustomExpectations = new Dictionary<string, object> { { "priority", "high" } }
            };

            // Act & Assert
            response.HasExpectations.Should().BeTrue();
        }
    }
}
