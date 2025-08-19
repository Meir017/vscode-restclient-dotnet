using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using RESTClient.NET.Testing.Assertions;
using RESTClient.NET.Testing.Models;
using Xunit;

namespace RESTClient.NET.Testing.Tests.Assertions
{
    public class HttpResponseAssertionTests
    {
        [Fact]
        public void AssertStatusCode_WithMatchingStatus_ShouldNotThrow()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertStatusCode(response, 200);
            action.Should().NotThrow();
        }

        [Fact]
        public void AssertStatusCode_WithMismatchedStatus_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertStatusCode(response, 200);
            action.Should().Throw<AssertionException>()
                .WithMessage("Expected status code 200, but got 404 (NotFound)");
        }

        [Fact]
        public void AssertStatusCode_WithNullResponse_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpResponseMessage response = null!;

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertStatusCode(response, 200);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("response");
        }

        [Theory]
        [InlineData(200, HttpStatusCode.OK)]
        [InlineData(201, HttpStatusCode.Created)]
        [InlineData(400, HttpStatusCode.BadRequest)]
        [InlineData(404, HttpStatusCode.NotFound)]
        [InlineData(500, HttpStatusCode.InternalServerError)]
        public void AssertStatusCode_WithVariousStatusCodes_ShouldWorkCorrectly(int expectedCode, HttpStatusCode actualCode)
        {
            // Arrange
            var response = new HttpResponseMessage(actualCode);

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertStatusCode(response, expectedCode);
            action.Should().NotThrow();
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

        [Fact]
        public async Task AssertBodyContains_WithNullResponse_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpResponseMessage response = null!;

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "test");
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("response");
        }

        [Fact]
        public async Task AssertBodyContains_WithNullExpectedContent_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("Hello, World!")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("expectedContent");
        }

        [Fact]
        public async Task AssertBodyContains_WithEmptyExpectedContent_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("Hello, World!")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "");
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("expectedContent");
        }

        [Fact]
        public async Task AssertBodyContains_WithWhitespaceExpectedContent_ShouldNotThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("Hello,    World!")
            };

            // Act & Assert - whitespace should be searchable
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "   ");
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertBodyContains_WithNoContent_ShouldHandleGracefully()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "test");
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("Expected response body to contain 'test'"));
        }

        [Fact]
        public async Task AssertBodyContains_CaseSensitive_ShouldBeCaseInsensitive()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("Hello, World!")
            };

            // Act & Assert - case mismatch should NOT throw (it's case-insensitive)
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "WORLD");
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertBodyContains_WithJsonContent_ShouldWorkCorrectly()
        {
            // Arrange
            var jsonContent = @"{""name"": ""John Doe"", ""age"": 30, ""city"": ""New York""}";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "John Doe");
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertBodyContains_WithPartialMatch_ShouldWork()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("This is a very long response body with lots of content")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertBodyContains(response, "very long response");
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public void AssertHeader_WithMatchingHeader_ShouldNotThrow()
        {
            // Arrange
            var response = new HttpResponseMessage();
            response.Headers.Add("X-Custom-Header", "custom-value");

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "X-Custom-Header", "custom-value");
            action.Should().NotThrow();
        }

        [Fact]
        public void AssertHeader_WithMismatchedHeaderValue_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage();
            response.Headers.Add("X-Custom-Header", "actual-value");

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "X-Custom-Header", "expected-value");
            action.Should().Throw<AssertionException>()
                .WithMessage("Expected header 'X-Custom-Header' to have value 'expected-value', but got 'actual-value'");
        }

        [Fact]
        public void AssertHeader_WithMissingHeader_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "X-Missing-Header", "expected-value");
            action.Should().Throw<AssertionException>()
                .WithMessage("Header 'X-Missing-Header' was not found in the response");
        }

        [Fact]
        public void AssertHeader_WithContentTypeHeader_ShouldWork()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("test", System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "Content-Type", "application/json; charset=utf-8");
            action.Should().NotThrow();
        }

        [Fact]
        public void AssertHeader_WithMultipleHeaderValues_ShouldHandleFirst()
        {
            // Arrange
            var response = new HttpResponseMessage();
            response.Headers.Add("X-Multi-Header", new[] { "value1", "value2", "value3" });

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "X-Multi-Header", "value1, value2, value3");
            action.Should().NotThrow();
        }

        [Fact]
        public void AssertHeader_WithNullResponse_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpResponseMessage response = null!;

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "X-Header", "value");
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("response");
        }

        [Fact]
        public void AssertHeader_WithNullHeaderName_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, null!, "value");
            action.Should().Throw<ArgumentException>()
                .WithParameterName("headerName");
        }

        [Fact]
        public void AssertHeader_WithEmptyHeaderName_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "", "value");
            action.Should().Throw<ArgumentException>()
                .WithParameterName("headerName");
        }

        [Fact]
        public void AssertHeader_WithWhitespaceHeaderName_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "   ", "value");
            action.Should().Throw<ArgumentException>()
                .WithParameterName("headerName");
        }

        [Fact]
        public void AssertHeader_CaseInsensitive_ShouldWork()
        {
            // Arrange
            var response = new HttpResponseMessage();
            response.Headers.Add("X-Custom-Language", "en-US");

            // Act & Assert - header comparison should be case-insensitive
            var action = () => HttpResponseAssertion.AssertHeader(response, "X-Custom-Language", "EN-US");
            action.Should().NotThrow();
        }

        [Fact]
        public async Task AssertJsonPath_WithValidJsonAndPath_ShouldNotThrow()
        {
            // Arrange
            var jsonContent = @"{""user"": {""name"": ""John"", ""age"": 30}}";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertJsonPath(response, "$.user.name");
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertJsonPath_WithInvalidJson_ShouldThrow()
        {
            // Arrange
            var invalidJson = @"{""user"": {""name"": ""John"", ""age"":}"; // Invalid JSON
            var response = new HttpResponseMessage
            {
                Content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertJsonPath(response, "$.user.name");
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("JSONPath assertion failed"));
        }

        [Fact]
        public async Task AssertJsonPath_WithNullResponse_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpResponseMessage response = null!;

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertJsonPath(response, "$.user.name");
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("response");
        }

        [Fact]
        public async Task AssertJsonPath_WithNullJsonPath_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertJsonPath(response, null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("jsonPath");
        }

        [Fact]
        public async Task AssertJsonPath_WithEmptyJsonPath_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertJsonPath(response, "");
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("jsonPath");
        }

        [Fact]
        public async Task AssertJsonPath_WithNoContent_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertJsonPath(response, "$.user.name");
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("Response has no content") || ex.Message.Contains("JSONPath assertion failed"));
        }

        [Fact]
        public async Task AssertSchema_WithValidJson_ShouldNotThrow()
        {
            // Arrange
            var jsonContent = @"{""name"": ""John"", ""age"": 30}";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertSchema(response, "user-schema.json");
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertSchema_WithInvalidJson_ShouldThrow()
        {
            // Arrange
            var invalidJson = @"{""name"": ""John"", ""age"":}"; // Invalid JSON
            var response = new HttpResponseMessage
            {
                Content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertSchema(response, "user-schema.json");
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("Response content is not valid JSON"));
        }

        [Fact]
        public async Task AssertSchema_WithNullResponse_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpResponseMessage response = null!;

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertSchema(response, "schema.json");
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("response");
        }

        [Fact]
        public async Task AssertSchema_WithNullSchemaPath_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertSchema(response, null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("schemaPath");
        }

        [Fact]
        public async Task AssertSchema_WithEmptySchemaPath_ShouldThrowArgumentException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertSchema(response, "");
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("schemaPath");
        }

        [Fact]
        public async Task AssertSchema_WithNoContent_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage();

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertSchema(response, "schema.json");
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("Response has no content") || ex.Message.Contains("Response content is not valid JSON"));
        }

        [Fact]
        public async Task AssertResponse_WithNullExpected_ShouldNotThrow()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertResponse(response, null);
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertResponse_WithNoExpectations_ShouldNotThrow()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var expected = new HttpExpectedResponse(); // No expectations set

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertResponse(response, expected);
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertResponse_WithCompleteExpectations_ShouldValidateAll()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""message"": ""success""}", System.Text.Encoding.UTF8, "application/json")
            };
            response.Headers.Add("X-Request-Id", "123");

            var expected = new HttpExpectedResponse
            {
                ExpectedStatusCode = 200,
                ExpectedHeaders = new Dictionary<string, string> { { "X-Request-Id", "123" } },
                ExpectedBodyContains = "success"
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertResponse(response, expected);
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertResponse_WithFailingStatusCode_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var expected = new HttpExpectedResponse { ExpectedStatusCode = 200 };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertResponse(response, expected);
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("Expected status code 200, but got 400"));
        }

        [Fact]
        public async Task AssertResponse_WithFailingHeader_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("X-Request-Id", "actual-id");

            var expected = new HttpExpectedResponse
            {
                ExpectedHeaders = new Dictionary<string, string> { { "X-Request-Id", "expected-id" } }
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertResponse(response, expected);
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("Expected header 'X-Request-Id' to have value 'expected-id'"));
        }

        [Fact]
        public async Task AssertResponse_WithFailingBodyContains_ShouldThrow()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("actual content")
            };

            var expected = new HttpExpectedResponse
            {
                ExpectedBodyContains = "expected content"
            };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertResponse(response, expected);
            await action.Should().ThrowAsync<AssertionException>()
                .Where(ex => ex.Message.Contains("Expected response body to contain 'expected content'"));
        }

        [Fact]
        public async Task AssertResponse_WithCustomExpectations_ShouldProcessThem()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""data"": ""test""}", System.Text.Encoding.UTF8, "application/json")
            };

            var expected = new HttpExpectedResponse
            {
                CustomExpectations = new Dictionary<string, object>
                {
                    { "custom-check", "test-value" }
                }
            };

            // Act & Assert - should not throw as custom expectations are handled gracefully
            var action = async () => await HttpResponseAssertion.AssertResponse(response, expected);
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AssertResponse_WithNullResponse_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpResponseMessage response = null!;
            var expected = new HttpExpectedResponse { ExpectedStatusCode = 200 };

            // Act & Assert
            var action = async () => await HttpResponseAssertion.AssertResponse(response, expected);
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("response");
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/html")]
        [InlineData("application/xml")]
        public void AssertHeader_WithContentTypeVariations_ShouldWork(string contentType)
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("test", System.Text.Encoding.UTF8, contentType)
            };

            // Act & Assert
            var action = () => HttpResponseAssertion.AssertHeader(response, "Content-Type", $"{contentType}; charset=utf-8");
            action.Should().NotThrow();
        }
    }
}
