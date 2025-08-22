using System.Net;
using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Processing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Processing
{
    public class ResponseVariableProcessorTests
    {
        [Fact]
        public void ResolveResponseVariables_WithNull_ReturnsNull()
        {
            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(null, new ResponseContext());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ResolveResponseVariables_WithEmpty_ReturnsEmpty()
        {
            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(string.Empty, new ResponseContext());

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ResolveResponseVariables_WithNullResponseContext_ReturnsOriginal()
        {
            // Arrange
            string content = "Bearer {{login.response.body.$.token}}";

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, null);

            // Assert
            result.Should().Be(content);
        }

        [Fact]
        public void ResolveResponseVariables_WithNoVariables_ReturnsOriginal()
        {
            // Arrange
            string content = "This is plain text without response variables";
            var responseContext = new ResponseContext();

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be(content);
        }

        [Fact]
        public void ResolveResponseVariables_WithJsonPathVariable_ReturnsResolvedValue()
        {
            // Arrange
            string content = "Bearer {{login.response.body.$.token}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateJsonResponseData("""{"token": "abc123", "userId": 42}""");
            responseContext.StoreResponse("login", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("Bearer abc123");
        }

        [Fact]
        public void ResolveResponseVariables_WithNestedJsonPath_ReturnsResolvedValue()
        {
            // Arrange
            string content = "User ID: {{login.response.body.$.user.id}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateJsonResponseData("""{"user": {"id": 42, "name": "John"}, "token": "xyz789"}""");
            responseContext.StoreResponse("login", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("User ID: 42");
        }

        [Fact]
        public void ResolveResponseVariables_WithFullBodyVariable_ReturnsFullBody()
        {
            // Arrange
            string content = "Response: {{test.response.body}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateJsonResponseData("""{"message": "Hello World"}""");
            responseContext.StoreResponse("test", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("""Response: {"message": "Hello World"}""");
        }

        [Fact]
        public void ResolveResponseVariables_WithHeaderVariable_ReturnsHeaderValue()
        {
            // Arrange
            string content = "Content-Type: {{api.response.header.Content-Type}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateResponseDataWithHeaders(new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Cache-Control", "no-cache" }
            });
            responseContext.StoreResponse("api", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("Content-Type: application/json");
        }

        [Fact]
        public void ResolveResponseVariables_WithStatusVariable_ReturnsStatusCode()
        {
            // Arrange
            string content = "Status: {{api.response.status}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateResponseDataWithStatus(HttpStatusCode.Created);
            responseContext.StoreResponse("api", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("Status: 201");
        }

        [Fact]
        public void ResolveResponseVariables_WithContentTypeVariable_ReturnsContentType()
        {
            // Arrange
            string content = "Type: {{api.response.contentType}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateResponseDataWithContentType("application/xml");
            responseContext.StoreResponse("api", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("Type: application/xml");
        }

        [Fact]
        public void ResolveResponseVariables_WithResponseTimeVariable_ReturnsResponseTime()
        {
            // Arrange
            string content = "Time: {{api.response.responseTime}}ms";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateResponseDataWithTime(125.75);
            responseContext.StoreResponse("api", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("Time: 125.75ms");
        }

        [Fact]
        public void ResolveResponseVariables_WithMultipleVariables_ResolvesAll()
        {
            // Arrange
            string content = "Authorization: Bearer {{login.response.body.$.token}}, Status: {{login.response.status}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateJsonResponseData("""{"token": "secret123"}""");
            responseData.StatusCode = HttpStatusCode.OK;
            responseContext.StoreResponse("login", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be("Authorization: Bearer secret123, Status: 200");
        }

        [Fact]
        public void ResolveResponseVariables_WithUnknownRequest_KeepsOriginal()
        {
            // Arrange
            string content = "Bearer {{unknown.response.body.$.token}}";
            var responseContext = new ResponseContext();

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be(content);
        }

        [Fact]
        public void ResolveResponseVariables_WithInvalidJsonPath_KeepsOriginal()
        {
            // Arrange
            string content = "Value: {{test.response.body.$.nonexistent}}";
            var responseContext = new ResponseContext();

            HttpResponseData responseData = CreateJsonResponseData("""{"other": "value"}""");
            responseContext.StoreResponse("test", responseData);

            // Act
            string? result = ResponseVariableProcessor.ResolveResponseVariables(content, responseContext);

            // Assert
            result.Should().Be(content);
        }

        [Fact]
        public void ContainsResponseVariables_WithResponseVariables_ReturnsTrue()
        {
            // Arrange
            string content = "Bearer {{login.response.body.$.token}}";

            // Act
            bool result = ResponseVariableProcessor.ContainsResponseVariables(content);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsResponseVariables_WithoutResponseVariables_ReturnsFalse()
        {
            // Arrange
            string content = "Bearer {{token}}";

            // Act
            bool result = ResponseVariableProcessor.ContainsResponseVariables(content);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ExtractReferencedRequests_WithMultipleRequests_ReturnsAllRequests()
        {
            // Arrange
            string content = """
                Authorization: Bearer {{login.response.body.$.token}}
                User-ID: {{profile.response.body.$.id}}
                Status: {{login.response.status}}
                """;

            // Act
            string[] result = ResponseVariableProcessor.ExtractReferencedRequests(content);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("login");
            result.Should().Contain("profile");
        }

        [Fact]
        public void ExtractReferencedRequests_WithNoResponseVariables_ReturnsEmpty()
        {
            // Arrange
            string content = "Authorization: Bearer {{token}}";

            // Act
            string[] result = ResponseVariableProcessor.ExtractReferencedRequests(content);

            // Assert
            result.Should().BeEmpty();
        }

        // Helper methods
        private static HttpResponseData CreateJsonResponseData(string jsonBody)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return HttpResponseData.FromHttpResponse(response, jsonBody);
        }

        private static HttpResponseData CreateResponseDataWithHeaders(Dictionary<string, string> headers)
        {
            return new HttpResponseData
            {
                Headers = headers,
                StatusCode = HttpStatusCode.OK
            };
        }

        private static HttpResponseData CreateResponseDataWithStatus(HttpStatusCode statusCode)
        {
            return new HttpResponseData
            {
                StatusCode = statusCode
            };
        }

        private static HttpResponseData CreateResponseDataWithContentType(string contentType)
        {
            return new HttpResponseData
            {
                ContentType = contentType,
                StatusCode = HttpStatusCode.OK
            };
        }

        private static HttpResponseData CreateResponseDataWithTime(double responseTimeMs)
        {
            return new HttpResponseData
            {
                ResponseTimeMs = responseTimeMs,
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
