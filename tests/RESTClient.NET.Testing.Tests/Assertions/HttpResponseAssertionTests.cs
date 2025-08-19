using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwesomeAssertions;
using RESTClient.NET.Testing.Assertions;
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
    }
}
