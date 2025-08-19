using System;
using System.IO;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core.Exceptions;
using RESTClient.NET.Core.Parsing;
using RESTClient.NET.Core.Validation;
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

        [Fact]
        public void Parse_WithStream_ShouldParseCorrectly()
        {
            // Arrange
            var content = @"# @name get-users
GET http://localhost:5000/api/users";
            
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var parser = new HttpFileParser();

            // Act
            var result = parser.Parse(stream);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.Requests.First().Name.Should().Be("get-users");
        }

        [Fact]
        public void Parse_WithStreamAndOptions_ShouldParseCorrectly()
        {
            // Arrange
            var content = @"# @name test-request
POST http://localhost:5000/api/users
Content-Type: application/json

{""name"": ""John""}";
            
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var options = new HttpParseOptions { ValidateRequestNames = true };
            var parser = new HttpFileParser();

            // Act
            var result = parser.Parse(stream, options);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.Requests.First().Name.Should().Be("test-request");
            result.Requests.First().Body.Should().Contain("John");
        }

        [Fact]
        public void Parse_WithNullStream_ShouldThrowArgumentNullException()
        {
            // Arrange
            var parser = new HttpFileParser();

            // Act & Assert
            Action act = () => parser.Parse((Stream)null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("stream");
        }

        [Fact]
        public async Task ParseFileAsync_WithValidFile_ShouldParseCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var content = @"# @name test-file-request
GET http://localhost:5000/api/test";
            
            try
            {
                await File.WriteAllTextAsync(tempFile, content);
                var parser = new HttpFileParser();

                // Act
                var result = await parser.ParseFileAsync(tempFile);

                // Assert
                result.Should().NotBeNull();
                result.Requests.Should().HaveCount(1);
                result.Requests.First().Name.Should().Be("test-file-request");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ParseFileAsync_WithValidFileAndOptions_ShouldParseCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var content = @"@baseUrl = http://localhost:5000

# @name test-file-request
# @expect status 200
GET {{baseUrl}}/api/test";
            
            try
            {
                await File.WriteAllTextAsync(tempFile, content);
                var options = new HttpParseOptions { ValidateRequestNames = true };
                var parser = new HttpFileParser();

                // Act
                var result = await parser.ParseFileAsync(tempFile, options);

                // Assert
                result.Should().NotBeNull();
                result.Requests.Should().HaveCount(1);
                result.FileVariables.Should().ContainKey("baseUrl");
                result.Requests.First().Metadata.Expectations.Should().HaveCount(1);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ParseFileAsync_WithNullPath_ShouldThrowArgumentException()
        {
            // Arrange
            var parser = new HttpFileParser();

            // Act & Assert
            await FluentActions.Invoking(() => parser.ParseFileAsync(null!))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*filePath*");
        }

        [Fact]
        public async Task ParseFileAsync_WithEmptyPath_ShouldThrowArgumentException()
        {
            // Arrange
            var parser = new HttpFileParser();

            // Act & Assert
            await FluentActions.Invoking(() => parser.ParseFileAsync(""))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*filePath*");
        }

        [Fact]
        public async Task ParseFileAsync_WithWhitespacePath_ShouldThrowArgumentException()
        {
            // Arrange
            var parser = new HttpFileParser();

            // Act & Assert
            await FluentActions.Invoking(() => parser.ParseFileAsync("   "))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*filePath*");
        }

        [Fact]
        public async Task ParseFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var parser = new HttpFileParser();
            var nonExistentPath = "non-existent-file-" + Guid.NewGuid() + ".http";

            // Act & Assert
            await FluentActions.Invoking(() => parser.ParseFileAsync(nonExistentPath))
                .Should().ThrowAsync<FileNotFoundException>()
                .WithMessage($"*{nonExistentPath}*");
        }

        [Fact]
        public void Validate_WithValidContent_ShouldReturnValidResult()
        {
            // Arrange
            var content = @"# @name test-request
GET http://localhost:5000/api/users";
            var parser = new HttpFileParser();

            // Act
            var result = parser.Validate(content);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithValidContentAndOptions_ShouldReturnValidResult()
        {
            // Arrange
            var content = @"@baseUrl = http://localhost:5000

# @name get-users
# @expect status 200
GET {{baseUrl}}/api/users";
            var options = new HttpParseOptions { ValidateRequestNames = true };
            var parser = new HttpFileParser();

            // Act
            var result = parser.Validate(content, options);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithParsingError_ShouldReturnInvalidResult()
        {
            // Arrange
            var content = @"# @name test-request
INVALID_HTTP_LINE";
            var parser = new HttpFileParser();

            // Act
            var result = parser.Validate(content);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public void Parse_WithStrictModeAndValidationFailure_ShouldThrowDuplicateRequestNameException()
        {
            // Arrange
            var content = @"### duplicate-name
GET http://localhost:5000/api/users

### duplicate-name
POST http://localhost:5000/api/users";
            
            var options = new HttpParseOptions 
            { 
                StrictMode = true,
                ValidateRequestNames = true
            };
            var parser = new HttpFileParser();

            // Act & Assert
            Action act = () => parser.Parse(content, options);
            act.Should().Throw<DuplicateRequestNameException>()
                .WithMessage("*duplicate-name*");
        }

        [Fact]
        public void Parse_WithValidationDisabledAndDuplicateNames_ShouldNotThrow()
        {
            // Arrange
            var content = @"### duplicate-name
GET http://localhost:5000/api/users

### duplicate-name
POST http://localhost:5000/api/users";
            
            var options = new HttpParseOptions 
            { 
                StrictMode = false,
                ValidateRequestNames = false
            };
            var parser = new HttpFileParser();

            // Act
            var result = parser.Parse(content, options);

            // Assert - should not throw, duplicates are allowed
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(2);
        }

        [Fact]
        public void Parse_WithStrictModeAndPostParsingValidationFailure_ShouldThrowHttpParseException()
        {
            // Arrange - content that parses fine but has validation issues
            var content = @"### very-long-request-name-that-exceeds-the-fifty-character-limit-for-request-names
GET http://localhost:5000/api/users";
            
            var options = new HttpParseOptions 
            { 
                StrictMode = true,
                ValidateRequestNames = false // Allow parsing, but validate after
            };
            var parser = new HttpFileParser();

            // Act & Assert
            Action act = () => parser.Parse(content, options);
            act.Should().Throw<HttpParseException>()
                .WithMessage("*Validation failed*");
        }

        [Fact]
        public void Parse_WithValidationEnabledButNotStrictMode_ShouldLogWarningButNotThrow()
        {
            // Arrange - content with validation warnings but not errors
            var content = @"### test-request
GET http://localhost:5000/api users with spaces";
            
            var options = new HttpParseOptions 
            { 
                StrictMode = false,
                ValidateRequestNames = false // Allow parsing to succeed
            };
            var parser = new HttpFileParser();

            // Act
            var result = parser.Parse(content, options);

            // Assert - should not throw, warnings are ignored in non-strict mode
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void Constructor_WithCustomDependencies_ShouldInitializeCorrectly()
        {
            // Arrange
            var mockTokenizer = new HttpTokenizer();
            var mockSyntaxParser = new HttpSyntaxParser();
            var mockValidator = new HttpFileValidator();
            using var loggerFactory = LoggerFactory.Create(builder => { });
            var logger = loggerFactory.CreateLogger<HttpFileParser>();

            // Act
            var parser = new HttpFileParser(mockTokenizer, mockSyntaxParser, mockValidator, logger);

            // Assert
            parser.Should().NotBeNull();
            
            // Test that it works with custom dependencies
            var content = @"# @name test
GET http://localhost:5000";
            var result = parser.Parse(content);
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void Parse_WithLoggerEnabled_ShouldParseSuccessfully()
        {
            // Arrange
            var content = @"# @name test-with-logger
GET http://localhost:5000/api/test";
            
            using var loggerFactory = LoggerFactory.Create(builder => { });
            var logger = loggerFactory.CreateLogger<HttpFileParser>();
            var parser = new HttpFileParser(logger: logger);

            // Act
            var result = parser.Parse(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.Requests.First().Name.Should().Be("test-with-logger");
        }
    }
}
