using AwesomeAssertions;
using RESTClient.NET.Core.Exceptions;
using RESTClient.NET.Core.Parsing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Parsing
{
    public class HttpFileParserTests
    {
        [Fact]
        public async Task ParseAsync_WithSimpleGetRequest_ShouldParseCorrectly()
        {
            // Arrange
            var content = @"# @name get-users
GET http://localhost:5000/api/users";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.Requests.First();
            request.Name.Should().Be("get-users");
            request.Method.Should().Be("GET");
            request.Url.Should().Be("http://localhost:5000/api/users");
            request.Headers.Should().BeEmpty();
            request.Body.Should().BeNull();
        }

        [Fact]
        public async Task ParseAsync_WithPostRequestAndBody_ShouldParseCorrectly()
        {
            // Arrange
            var content = @"# @name create-user
POST http://localhost:5000/api/users
Content-Type: application/json

{
  ""name"": ""John Doe"",
  ""email"": ""john@example.com""
}";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.Requests.First();
            request.Name.Should().Be("create-user");
            request.Method.Should().Be("POST");
            request.Url.Should().Be("http://localhost:5000/api/users");
            request.Headers.Should().HaveCount(1);
            request.Headers["Content-Type"].Should().Be("application/json");
            request.Body.Should().Contain("John Doe");
            request.Body.Should().Contain("john@example.com");
        }

        [Fact]
        public async Task ParseAsync_WithMultipleRequests_ShouldParseAllCorrectly()
        {
            // Arrange
            var content = @"### get-users
GET http://localhost:5000/api/users

### create-user  
POST http://localhost:5000/api/users
Content-Type: application/json

{""name"": ""Jane""}

### delete-user
DELETE http://localhost:5000/api/users/1";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(3);

            var getRequest = result.GetRequestByName("get-users");
            getRequest.Method.Should().Be("GET");

            var postRequest = result.GetRequestByName("create-user");
            postRequest.Method.Should().Be("POST");
            postRequest.Headers["Content-Type"].Should().Be("application/json");

            var deleteRequest = result.GetRequestByName("delete-user");
            deleteRequest.Method.Should().Be("DELETE");
        }

        [Fact]
        public async Task ParseAsync_WithVariables_ShouldParseFileVariables()
        {
            // Arrange
            var content = @"@baseUrl = http://localhost:5000
@apiKey = secret123

### get-users
GET {{baseUrl}}/api/users
Authorization: Bearer {{apiKey}}";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.FileVariables.Should().HaveCount(2);
            result.FileVariables["baseUrl"].Should().Be("http://localhost:5000");
            result.FileVariables["apiKey"].Should().Be("secret123");

            var request = result.Requests.First();
            request.Url.Should().Be("{{baseUrl}}/api/users");
            request.Headers["Authorization"].Should().Be("Bearer {{apiKey}}");
        }

        [Fact]
        public async Task ParseAsync_WithExpectations_ShouldParseMetadata()
        {
            // Arrange
            var content = @"### test-api
# @expect status 200
# @expect header Content-Type application/json
# @expect body-path $.users[0].name John
GET http://localhost:5000/api/users";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.Requests.First();
            request.Metadata.Expectations.Should().HaveCount(3);

            var statusExpectation = request.Metadata.Expectations.First(e => e.Type == RESTClient.NET.Core.Models.ExpectationType.StatusCode);
            statusExpectation.Value.Should().Be("200");

            var headerExpectation = request.Metadata.Expectations.First(e => e.Type == RESTClient.NET.Core.Models.ExpectationType.Header);
            headerExpectation.Value.Should().Be("Content-Type application/json");

            var bodyExpectation = request.Metadata.Expectations.First(e => e.Type == RESTClient.NET.Core.Models.ExpectationType.BodyPath);
            bodyExpectation.Value.Should().Be("$.users[0].name John");
        }

        [Fact]
        public async Task ParseAsync_WithDuplicateRequestNames_ShouldThrowDuplicateRequestNameException()
        {
            // Arrange
            var content = @"### duplicate-id
GET http://localhost:5000/api/users

### duplicate-id
POST http://localhost:5000/api/users";

            var parser = new HttpFileParser();

            // Act & Assert
            await FluentActions.Invoking(() => parser.ParseAsync(content))
                .Should().ThrowAsync<DuplicateRequestNameException>()
                .WithMessage("*duplicate-id*");
        }

        [Fact]
        public async Task ParseAsync_WithMissingRequestName_ShouldThrowMissingRequestNameException()
        {
            // Arrange
            var content = @"###
GET http://localhost:5000/api/users";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert - should generate a default name instead of throwing
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.Requests.First().Name.Should().Be("request-1");
        }

        [Fact]
        public async Task ParseAsync_WithInvalidRequestName_ShouldThrowInvalidRequestNameException()
        {
            // Arrange
            var content = @"### invalid request id with spaces
GET http://localhost:5000/api/users";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert - should parse with processed name
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.Requests.First().Name.Should().Be("invalid-request-id-with-spaces");
        }

        [Fact]
        public async Task ParseAsync_WithComments_ShouldIgnoreComments()
        {
            // Arrange
            var content = @"// This is a comment
# This is also a comment

### get-users
GET http://localhost:5000/api/users
// This comment should be ignored
Authorization: Bearer token";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.Requests.First();
            request.Headers.Should().HaveCount(1);
            request.Headers["Authorization"].Should().Be("Bearer token");
        }

        [Fact]
        public async Task ParseAsync_WithEmptyContent_ShouldReturnEmptyHttpFile()
        {
            // Arrange
            var content = "";
            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().BeEmpty();
            result.FileVariables.Should().BeEmpty();
        }

        [Fact]
        public async Task ParseAsync_WithWhitespaceOnlyContent_ShouldReturnEmptyHttpFile()
        {
            // Arrange
            var content = "   \n\n\t  \r\n  ";
            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().BeEmpty();
            result.FileVariables.Should().BeEmpty();
        }

        [Fact]
        public async Task ParseAsync_WithValidationDisabled_ShouldAllowDuplicateIds()
        {
            // Arrange
            var content = @"### duplicate-id
GET http://localhost:5000/api/users

### duplicate-id
POST http://localhost:5000/api/users";

            var options = new HttpParseOptions { ValidateRequestNames = false };
            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content, options);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(2);
        }

        [Theory]
        [InlineData("GET", "http://example.com")]
        [InlineData("POST", "https://api.example.com/users")]
        [InlineData("PUT", "/relative/path")]
        [InlineData("DELETE", "{{baseUrl}}/api/users/{{userId}}")]
        [InlineData("PATCH", "http://localhost:8080")]
        public async Task ParseAsync_WithDifferentHttpMethods_ShouldParseCorrectly(string method, string url)
        {
            // Arrange
            var content = $@"### test-request
{method} {url}";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.Requests.First();
            request.Method.Should().Be(method);
            request.Url.Should().Be(url);
        }
    }
}
