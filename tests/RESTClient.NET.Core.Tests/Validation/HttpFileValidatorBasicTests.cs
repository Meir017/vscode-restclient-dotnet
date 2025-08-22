using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Validation;
using Xunit;

namespace RESTClient.NET.Core.Tests.Validation
{
    public class HttpFileValidatorBasicTests
    {
        private readonly HttpFileValidator _validator;

        public HttpFileValidatorBasicTests()
        {
            _validator = new HttpFileValidator();
        }

        [Fact]
        public void Validate_WithNullHttpFile_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => _validator.Validate(null!))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("httpFile");
        }

        [Fact]
        public void Validate_WithEmptyHttpFile_ShouldReturnSuccess()
        {
            // Arrange
            var httpFile = new HttpFile([]);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeFalse();
        }

        [Fact]
        public void Validate_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void Validate_WithEmptyRequestName_ShouldReturnMissingRequestNameError()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.MissingRequestName);
            result.Errors[0].Message.Should().Be("Request is missing a required request name");
            result.Errors[0].LineNumber.Should().Be(1);
        }

        [Fact]
        public void Validate_WithInvalidRequestNameCharacters_ShouldReturnInvalidRequestNameError()
        {
            // Arrange
            string invalidName = "invalid name with spaces";
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = invalidName,
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidRequestName);
            result.Errors[0].Message.Should().Contain($"Invalid request name '{invalidName}'");
            result.Errors[0].Message.Should().Contain("alphanumeric characters, hyphens, and underscores");
        }

        [Fact]
        public void Validate_WithTooLongRequestName_ShouldReturnInvalidRequestNameError()
        {
            // Arrange
            string longName = new('a', 51); // 51 characters, exceeding the 50 character limit
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = longName,
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidRequestName);
            result.Errors[0].Message.Should().Contain($"Request name '{longName}' is too long");
            result.Errors[0].Message.Should().Contain("Maximum length is 50 characters");
        }

        [Fact]
        public void Validate_WithDuplicateRequestNames_ShouldReturnDuplicateRequestNameError()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "duplicate-name",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                },
                new() {
                    Name = "duplicate-name",
                    Method = "POST",
                    Url = "https://api.example.com/users",
                    LineNumber = 5
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.DuplicateRequestName);
            result.Errors[0].Message.Should().Be("Duplicate request name 'duplicate-name' found");
            result.Errors[0].LineNumber.Should().Be(5); // Should report the second occurrence
        }

        [Fact]
        public void Validate_WithEmptyUrl_ShouldReturnError()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "GET",
                    Url = "",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidHttpSyntax);
            result.Errors[0].Message.Should().Be("Request URL is required");
            result.Errors[0].LineNumber.Should().Be(1);
        }

        [Fact]
        public void Validate_WithEmptyHttpMethod_ShouldReturnWarning()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be("HTTP method is empty, defaulting to GET");
            result.Warnings[0].LineNumber.Should().Be(1);
        }

        [Fact]
        public void Validate_WithUnknownHttpMethod_ShouldReturnWarning()
        {
            // Arrange
            string customMethod = "CUSTOM";
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = customMethod,
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be($"Unknown HTTP method '{customMethod}'");
            result.Warnings[0].LineNumber.Should().Be(1);
        }

        [Fact]
        public void Validate_WithInvalidStatusCodeExpectation_ShouldReturnError()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "GET",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Metadata.Expectations.Add(new TestExpectation(
                ExpectationType.StatusCode,
                "invalid"
            ));
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidExpectation);
            result.Errors[0].Message.Should().Be("Invalid status code expectation 'invalid'. Must be a number between 100-599");
        }

        [Fact]
        public void Validate_WithInvalidMaxTimeExpectation_ShouldReturnError()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "GET",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Metadata.Expectations.Add(new TestExpectation(
                ExpectationType.MaxTime,
                "1000" // Missing 'ms'
            ));
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidExpectation);
            result.Errors[0].Message.Should().Be("Invalid max-time expectation '1000'. Must be a positive number followed by 'ms'");
        }

        [Fact]
        public void Validate_WithCircularReferenceInVariable_ShouldReturnError()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                { "selfRef", "prefix-{{selfRef}}-suffix" }
            };
            var httpFile = new HttpFile([], fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidVariable);
            result.Errors[0].Message.Should().Be("Variable 'selfRef' has a circular reference to itself");
            result.Errors[0].LineNumber.Should().Be(0);
        }

        [Fact]
        public void Validate_WithEmptyVariableName_ShouldReturnError()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                { "", "some-value" }
            };
            var httpFile = new HttpFile([], fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidVariable);
            result.Errors[0].Message.Should().Be("Variable name cannot be empty");
            result.Errors[0].LineNumber.Should().Be(0);
        }
    }
}
