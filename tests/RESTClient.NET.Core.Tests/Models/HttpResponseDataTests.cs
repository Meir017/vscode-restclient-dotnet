using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class HttpResponseDataTests
    {
        [Fact]
        public void FromHttpResponse_WithValidResponse_CreatesCorrectly()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Custom-Header", "test-value");
            var bodyContent = """{"id": 123, "name": "Test"}""";

            // Act
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent, 150.5);

            // Assert
            responseData.Should().NotBeNull();
            responseData.StatusCode.Should().Be(HttpStatusCode.OK);
            responseData.BodyContent.Should().Be(bodyContent);
            responseData.ResponseTimeMs.Should().Be(150.5);
            responseData.ParsedBody.Should().NotBeNull();
            responseData.GetHeaderValue("Custom-Header").Should().Be("test-value");
        }

        [Fact]
        public void FromHttpResponse_WithNullResponse_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => HttpResponseData.FromHttpResponse(null!, "content");
            act.Should().Throw<ArgumentNullException>().WithParameterName("response");
        }

        [Fact]
        public void FromHttpResponse_WithNonJsonContent_DoesNotParseBody()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var bodyContent = "Plain text content";

            // Act
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent);

            // Assert
            responseData.BodyContent.Should().Be(bodyContent);
            responseData.ParsedBody.Should().BeNull();
        }

        [Fact]
        public void FromHttpResponse_WithInvalidJson_DoesNotParseBody()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json");
            var bodyContent = "invalid json";

            // Act
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent);

            // Assert
            responseData.BodyContent.Should().Be(bodyContent);
            responseData.ParsedBody.Should().BeNull();
        }

        [Fact]
        public void GetJsonPathValue_WithValidPath_ReturnsValue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var bodyContent = """{"user": {"id": 42, "name": "John"}, "token": "abc123"}""";
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent);

            // Act
            var userId = responseData.GetJsonPathValue("$.user.id");
            var token = responseData.GetJsonPathValue("$.token");
            var name = responseData.GetJsonPathValue("$.user.name");

            // Assert
            userId.Should().Be("42");
            token.Should().Be("abc123");
            name.Should().Be("John");
        }

        [Fact]
        public void GetJsonPathValue_WithInvalidPath_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var bodyContent = """{"user": {"id": 42}}""";
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent);

            // Act
            var result = responseData.GetJsonPathValue("$.nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetJsonPathValue_WithNullOrEmptyPath_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var bodyContent = """{"user": {"id": 42}}""";
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent);

            // Act
            var nullResult = responseData.GetJsonPathValue(null!);
            var emptyResult = responseData.GetJsonPathValue("");

            // Assert
            nullResult.Should().BeNull();
            emptyResult.Should().BeNull();
        }

        [Fact]
        public void GetJsonPathValue_WithNoParsedBody_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var bodyContent = "Plain text";
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent);

            // Act
            var result = responseData.GetJsonPathValue("$.anything");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetHeaderValue_WithExistingHeader_ReturnsValue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Authorization", "Bearer token123");
            response.Headers.Add("Server", "TestServer/1.0");
            var responseData = HttpResponseData.FromHttpResponse(response, "content");

            // Act
            var auth = responseData.GetHeaderValue("Authorization");
            var server = responseData.GetHeaderValue("Server");

            // Assert
            auth.Should().Be("Bearer token123");
            server.Should().Be("TestServer/1.0");
        }

        [Fact]
        public void GetHeaderValue_WithCaseInsensitiveName_ReturnsValue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent("content", System.Text.Encoding.UTF8, "application/json");
            var responseData = HttpResponseData.FromHttpResponse(response, "content");

            // Act
            var lowerCase = responseData.GetHeaderValue("content-type");
            var upperCase = responseData.GetHeaderValue("CONTENT-TYPE");
            var mixedCase = responseData.GetHeaderValue("Content-Type");

            // Assert
            lowerCase.Should().Be("application/json; charset=utf-8");
            upperCase.Should().Be("application/json; charset=utf-8");
            mixedCase.Should().Be("application/json; charset=utf-8");
        }

        [Fact]
        public void GetHeaderValue_WithNonExistentHeader_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var responseData = HttpResponseData.FromHttpResponse(response, "content");

            // Act
            var result = responseData.GetHeaderValue("Non-Existent-Header");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetHeaderValue_WithNullOrEmptyName_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var responseData = HttpResponseData.FromHttpResponse(response, "content");

            // Act
            var nullResult = responseData.GetHeaderValue(null!);
            var emptyResult = responseData.GetHeaderValue("");

            // Assert
            nullResult.Should().BeNull();
            emptyResult.Should().BeNull();
        }

        [Fact]
        public void Properties_SetCorrectly()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Created);
            response.Content = new StringContent("content", System.Text.Encoding.UTF8, "application/json");
            var bodyContent = """{"result": "success"}""";
            var responseTimeMs = 250.75;

            // Act
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent, responseTimeMs);

            // Assert
            responseData.StatusCode.Should().Be(HttpStatusCode.Created);
            responseData.BodyContent.Should().Be(bodyContent);
            responseData.ResponseTimeMs.Should().Be(responseTimeMs);
            responseData.ContentType.Should().Be("application/json");
            responseData.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Headers_IncludesBothResponseAndContentHeaders()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Server", "TestServer/1.0");
            response.Headers.Add("Cache-Control", "no-cache");
            response.Content = new StringContent("content", System.Text.Encoding.UTF8, "application/json");

            // Act
            var responseData = HttpResponseData.FromHttpResponse(response, "content");

            // Assert
            responseData.Headers.Should().ContainKey("Server");
            responseData.Headers.Should().ContainKey("Cache-Control");
            responseData.Headers.Should().ContainKey("Content-Type");
            responseData.Headers["Server"].Should().Be("TestServer/1.0");
            responseData.Headers["Cache-Control"].Should().Be("no-cache");
        }

        [Fact]
        public void ParsedBody_WithJsonArray_ParsesCorrectly()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var bodyContent = """[{"id": 1, "name": "First"}, {"id": 2, "name": "Second"}]""";

            // Act
            var responseData = HttpResponseData.FromHttpResponse(response, bodyContent);

            // Assert
            responseData.ParsedBody.Should().NotBeNull();
            responseData.GetJsonPathValue("$[0].id").Should().Be("1");
            responseData.GetJsonPathValue("$[1].name").Should().Be("Second");
        }
    }
}
