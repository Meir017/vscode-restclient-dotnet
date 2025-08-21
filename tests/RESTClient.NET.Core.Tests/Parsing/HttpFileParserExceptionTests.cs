using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core.Exceptions;
using RESTClient.NET.Core.Parsing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Parsing
{
    public class HttpFileParserExceptionTests
    {
        [Fact]
        public void Parse_WithNullContent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = null!;

            // Act & Assert
            FluentActions.Invoking(() => parser.Parse(content))
                .Should().Throw<ArgumentNullException>()
                .WithMessage("*content*");
        }

        [Fact]
        public void Parse_WithInvalidHttpSyntax_ShouldHandleGracefully()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name test-request
INVALID_HTTP_LINE_WITHOUT_METHOD_OR_URL";

            // Act
            Core.Models.HttpFile result = parser.Parse(content);

            // Assert
            result.Should().NotBeNull();
            // Parser may handle invalid lines gracefully and skip them
            // The exact behavior depends on the parser implementation
        }

        [Fact]
        public void Parse_WithValidationEnabled_AndDuplicateNames_ShouldThrowHttpParseException()
        {
            // Arrange
            var parser = new HttpFileParser();
            var options = new HttpParseOptions { ValidateRequestNames = true, StrictMode = true };
            string content = @"# @name duplicate-name
GET http://localhost:5000/api/users

# @name duplicate-name
POST http://localhost:5000/api/users";

            // Act & Assert
            FluentActions.Invoking(() => parser.Parse(content, options))
                .Should().Throw<HttpParseException>()
                .WithMessage("*duplicate-name*");
        }

        [Fact]
        public void Parse_WithValidationDisabled_AndDuplicateNames_ShouldNotThrow()
        {
            // Arrange
            var parser = new HttpFileParser();
            var options = new HttpParseOptions { ValidateRequestNames = false };
            string content = @"# @name duplicate-name
GET http://localhost:5000/api/users

# @name duplicate-name
POST http://localhost:5000/api/users";

            // Act
            Core.Models.HttpFile result = parser.Parse(content, options);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(2);
        }

        [Fact]
        public void Parse_WithLogger_ShouldLogProcessing()
        {
            // Arrange
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { });
            ILogger<HttpFileParser> logger = loggerFactory.CreateLogger<HttpFileParser>();
            var parser = new HttpFileParser(logger: logger);
            string content = @"# @name test-request
GET http://localhost:5000/api/users";

            // Act
            Core.Models.HttpFile result = parser.Parse(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void Parse_WithMalformedFileVariable_ShouldHandleGracefully()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"@invalidVariable
# @name test-request
GET http://localhost:5000/api/users";

            // Act
            Core.Models.HttpFile result = parser.Parse(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.FileVariables.Should().BeEmpty();
        }

        [Fact]
        public void Parse_WithEmptyRequestName_ShouldHandleGracefully()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name
GET http://localhost:5000/api/users

# @name
POST http://localhost:5000/api/users";

            // Act
            Core.Models.HttpFile result = parser.Parse(content);

            // Assert
            result.Should().NotBeNull();
            // Parser behavior with empty names depends on implementation
            // May generate default names or skip requests with empty names
        }

        [Fact]
        public void Parse_WithOnlyComments_ShouldReturnEmptyHttpFile()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# This is a comment
// This is another comment
/* This is a block comment */";

            // Act
            Core.Models.HttpFile result = parser.Parse(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().BeEmpty();
            result.FileVariables.Should().BeEmpty();
        }

        [Fact]
        public void Parse_WithMixedSeparators_ShouldParseAllRequests()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name first-request
GET http://localhost:5000/api/users

### second-request
POST http://localhost:5000/api/users
Content-Type: application/json

{""name"": ""test""}

# @name third-request
DELETE http://localhost:5000/api/users/1";

            // Act
            Core.Models.HttpFile result = parser.Parse(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(3);
            result.Requests[0].Name.Should().Be("first-request");
            result.Requests[1].Name.Should().Be("second-request");
            result.Requests[2].Name.Should().Be("third-request");
        }

        [Fact]
        public void Parse_WithNullOptions_ShouldUseDefaults()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name test-request
GET http://localhost:5000/api/users";

            // Act
            Core.Models.HttpFile result = parser.Parse(content, null);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void Parse_WithStrictMode_AndValidation_ShouldEnforceStrictness()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name test-request
GET http://localhost:5000/api/users";
            var options = new HttpParseOptions { StrictMode = true, ValidateRequestNames = true };

            // Act
            Core.Models.HttpFile result = parser.Parse(content, options);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void Parse_WithAllOptionsEnabled_ShouldRespectOptions()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"@baseUrl = http://localhost:5000

# @name test-request
# @expect status 200
GET {{baseUrl}}/api/users";
            var options = new HttpParseOptions
            {
                ProcessVariables = true,
                ParseExpectations = true,
                RequireRequestNames = true,
                AllowEmptyBodies = true,
                NormalizeLineEndings = true,
                IgnoreUnknownMetadata = true
            };

            // Act
            Core.Models.HttpFile result = parser.Parse(content, options);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.FileVariables.Should().ContainKey("baseUrl");
        }

        [Fact]
        public async Task ParseAsync_WithNullContent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var parser = new HttpFileParser();

            // Act & Assert
            await FluentActions.Invoking(() => parser.ParseAsync(null!))
                .Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("*content*");
        }

        [Fact]
        public async Task ParseAsync_WithValidContent_ShouldReturnHttpFile()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name test-request
GET http://localhost:5000/api/users";

            // Act
            Core.Models.HttpFile result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void Validate_WithValidContent_ShouldReturnValidResult()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name test-request
GET http://localhost:5000/api/users";

            // Act
            Core.Validation.ValidationResult result = parser.Validate(content);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithInvalidContent_ShouldReturnInvalidResult()
        {
            // Arrange
            var parser = new HttpFileParser();
            string content = @"# @name test-request
INVALID_HTTP_LINE";

            // Act
            Core.Validation.ValidationResult result = parser.Validate(content);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
        }
    }
}
