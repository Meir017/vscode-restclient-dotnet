using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Validation;
using Xunit;

namespace RESTClient.NET.Core.Tests.Validation
{
    public class HttpFileValidatorTestsFixed
    {
        private readonly HttpFileValidator _validator;

        public HttpFileValidatorTestsFixed()
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

        #region Request Name Validation Tests

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
        public void Validate_WithWhitespaceOnlyRequestName_ShouldReturnMissingRequestNameError()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "   \t  ",
                    Method = "GET",
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
            result.Errors[0].Type.Should().Be(ValidationErrorType.MissingRequestName);
            result.Errors[0].LineNumber.Should().Be(5);
        }

        [Theory]
        [InlineData("invalid name with spaces")]
        [InlineData("invalid@name")]
        [InlineData("invalid#name")]
        [InlineData("invalid$name")]
        [InlineData("invalid%name")]
        [InlineData("invalid&name")]
        [InlineData("invalid*name")]
        [InlineData("invalid(name)")]
        [InlineData("invalid+name")]
        [InlineData("invalid=name")]
        [InlineData("invalid[name]")]
        [InlineData("invalid{name}")]
        [InlineData("invalid|name")]
        [InlineData("invalid\\name")]
        [InlineData("invalid:name")]
        [InlineData("invalid;name")]
        [InlineData("invalid\"name")]
        [InlineData("invalid'name")]
        [InlineData("invalid<name>")]
        [InlineData("invalid,name")]
        [InlineData("invalid.name")]
        [InlineData("invalid?name")]
        [InlineData("invalid/name")]
        public void Validate_WithInvalidRequestNameCharacters_ShouldReturnInvalidRequestNameError(string invalidName)
        {
            // Arrange
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

        [Theory]
        [InlineData("valid-name")]
        [InlineData("valid_name")]
        [InlineData("ValidName")]
        [InlineData("validname")]
        [InlineData("VALIDNAME")]
        [InlineData("valid123")]
        [InlineData("123valid")]
        [InlineData("a")]
        [InlineData("A")]
        [InlineData("1")]
        [InlineData("valid-name_with-both")]
        [InlineData("test-123_abc")]
        public void Validate_WithValidRequestNameCharacters_ShouldPass(string validName)
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = validName,
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
        public void Validate_WithExactly50CharacterRequestName_ShouldPass()
        {
            // Arrange
            string exactName = new('a', 50); // Exactly 50 characters
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = exactName,
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
        public void Validate_WithMultipleDuplicateRequestNames_ShouldReturnMultipleErrors()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() { Name = "duplicate-1", Method = "GET", Url = "https://example.com", LineNumber = 1 },
                new() { Name = "duplicate-1", Method = "POST", Url = "https://example.com", LineNumber = 5 },
                new() { Name = "duplicate-1", Method = "PUT", Url = "https://example.com", LineNumber = 10 },
                new() { Name = "duplicate-2", Method = "GET", Url = "https://example.com", LineNumber = 15 },
                new() { Name = "duplicate-2", Method = "DELETE", Url = "https://example.com", LineNumber = 20 }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(3); // 2 duplicates of duplicate-1 + 1 duplicate of duplicate-2

            var duplicate1Errors = result.Errors.Where(e => e.Message.Contains("duplicate-1")).ToList();
            duplicate1Errors.Should().HaveCount(2);
            duplicate1Errors.Should().Contain(e => e.LineNumber == 5);
            duplicate1Errors.Should().Contain(e => e.LineNumber == 10);

            var duplicate2Errors = result.Errors.Where(e => e.Message.Contains("duplicate-2")).ToList();
            duplicate2Errors.Should().HaveCount(1);
            duplicate2Errors[0].LineNumber.Should().Be(20);
        }

        #endregion

        #region HTTP Method Validation Tests

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        [InlineData("HEAD")]
        [InlineData("OPTIONS")]
        [InlineData("CONNECT")]
        [InlineData("TRACE")]
        [InlineData("LOCK")]
        [InlineData("UNLOCK")]
        [InlineData("PROPFIND")]
        [InlineData("PROPPATCH")]
        [InlineData("COPY")]
        [InlineData("MOVE")]
        [InlineData("MKCOL")]
        [InlineData("MKCALENDAR")]
        [InlineData("ACL")]
        [InlineData("SEARCH")]
        public void Validate_WithValidHttpMethods_ShouldPass(string method)
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = method,
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
        }

        [Theory]
        [InlineData("get")] // lowercase
        [InlineData("post")]
        [InlineData("put")]
        [InlineData("delete")]
        public void Validate_WithLowercaseHttpMethods_ShouldPass(string method)
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = method,
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
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
        public void Validate_WithWhitespaceHttpMethod_ShouldReturnWarning()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "   ",
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
            result.Warnings[0].Message.Should().Be("HTTP method is empty, defaulting to GET");
        }

        [Theory]
        [InlineData("CUSTOM")]
        [InlineData("UNKNOWN")]
        [InlineData("FOOBAR")]
        [InlineData("INVALID")]
        public void Validate_WithUnknownHttpMethod_ShouldReturnWarning(string method)
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = method,
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
            result.Warnings[0].Message.Should().Be($"Unknown HTTP method '{method}'");
            result.Warnings[0].LineNumber.Should().Be(1);
        }

        #endregion

        #region URL Validation Tests

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
        public void Validate_WithWhitespaceOnlyUrl_ShouldReturnError()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "GET",
                    Url = "   \t  ",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidHttpSyntax);
            result.Errors[0].Message.Should().Be("Request URL is required");
        }

        [Theory]
        [InlineData("https://api.example.com")]
        [InlineData("http://localhost:3000")]
        [InlineData("https://api.example.com/users")]
        [InlineData("http://192.168.1.1:8080/api")]
        [InlineData("/api/users")]
        [InlineData("/relative/path")]
        [InlineData("{{baseUrl}}/api/users")]
        [InlineData("{{protocol}}://{{host}}/{{path}}")]
        public void Validate_WithValidUrls_ShouldPass(string url)
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "GET",
                    Url = url,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
        }

        [Theory]
        [InlineData("invalid-url")]
        [InlineData("not-a-url")]
        [InlineData("example.com")]
        [InlineData("ftp://example.com")]
        public void Validate_WithInvalidUrlFormat_ShouldReturnWarning(string url)
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "GET",
                    Url = url,
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
            result.Warnings[0].Message.Should().Contain($"URL '{url}' may not be valid");
            result.Warnings[0].Message.Should().Contain("Expected format: http://..., https://..., /path, or {{variable}}");
        }

        [Fact]
        public void Validate_WithUrlContainingSpaces_ShouldReturnWarning()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/path with spaces",
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
            result.Warnings[0].Message.Should().Be("URL contains spaces. Consider URL encoding");
        }

        [Fact]
        public void Validate_WithUrlContainingSpacesButHasVariables_ShouldNotReturnSpacesWarning()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new() {
                    Name = "test-request",
                    Method = "GET",
                    Url = "{{baseUrl}}/path with spaces", // Has variables, so spaces might be valid
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
        }

        #endregion

        #region Header Validation Tests

        [Fact]
        public void Validate_WithValidHeaders_ShouldPass()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers["Content-Type"] = "application/json";
            request.Headers["Authorization"] = "Bearer token123";
            request.Headers["Accept"] = "application/json";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
        }

        [Fact]
        public void Validate_WithEmptyHeaderName_ShouldReturnError()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers[""] = "some-value";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidHttpSyntax);
            result.Errors[0].Message.Should().Be("Header name cannot be empty");
        }

        [Fact]
        public void Validate_WithWhitespaceOnlyHeaderName_ShouldReturnError()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers["   "] = "some-value";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors[0].Message.Should().Be("Header name cannot be empty");
        }

        [Theory]
        [InlineData("Header With Spaces")]
        [InlineData("Header\tWith\tTabs")]
        [InlineData(" HeaderWithLeadingSpace")]
        [InlineData("HeaderWithTrailingSpace ")]
        public void Validate_WithWhitespaceInHeaderName_ShouldReturnWarning(string headerName)
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers[headerName] = "some-value";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be($"Header name '{headerName}' contains whitespace");
        }

        [Fact]
        public void Validate_WithEmptyContentTypeHeader_ShouldReturnWarning()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers["Content-Type"] = "";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be("Content-Type header is empty");
        }

        [Fact]
        public void Validate_WithWhitespaceOnlyContentTypeHeader_ShouldReturnWarning()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers["Content-Type"] = "   ";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings[0].Message.Should().Be("Content-Type header is empty");
        }

        [Fact]
        public void Validate_WithEmptyAuthorizationHeader_ShouldReturnWarning()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers["Authorization"] = "";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be("Authorization header is empty");
        }

        [Fact]
        public void Validate_WithContentLengthHeader_ShouldReturnWarning()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers["Content-Length"] = "123";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be("Content-Length header will be automatically calculated");
        }

        [Theory]
        [InlineData("content-type")]
        [InlineData("CONTENT-TYPE")]
        [InlineData("Content-type")]
        [InlineData("authorization")]
        [InlineData("AUTHORIZATION")]
        [InlineData("content-length")]
        [InlineData("CONTENT-LENGTH")]
        public void Validate_WithCaseInsensitiveSpecialHeaders_ShouldTriggerAppropriateValidation(string headerName)
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.Headers[headerName] = "";
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();

            string lowerHeaderName = headerName.ToLowerInvariant();
            if (lowerHeaderName == "content-type")
            {
                result.Warnings[0].Message.Should().Be("Content-Type header is empty");
            }
            else if (lowerHeaderName == "authorization")
            {
                result.Warnings[0].Message.Should().Be("Authorization header is empty");
            }
            else if (lowerHeaderName == "content-length")
            {
                result.Warnings[0].Message.Should().Be("Content-Length header will be automatically calculated");
            }
        }

        #endregion

        #region Expectation Validation Tests

        [Theory]
        [InlineData("200")]
        [InlineData("404")]
        [InlineData("500")]
        [InlineData("100")]
        [InlineData("599")]
        public void Validate_WithValidStatusCodeExpectations_ShouldPass(string statusCode)
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
                statusCode
            ));
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("99")]   // Below 100
        [InlineData("600")]  // Above 599
        [InlineData("1000")] // Way above 599
        [InlineData("0")]    // Zero
        [InlineData("-1")]   // Negative
        [InlineData("abc")]  // Non-numeric
        [InlineData("")]     // Empty
        [InlineData("20x")]  // Mixed
        public void Validate_WithInvalidStatusCodeExpectations_ShouldReturnError(string statusCode)
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
                statusCode
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
            result.Errors[0].Message.Should().Be($"Invalid status code expectation '{statusCode}'. Must be a number between 100-599");
        }

        [Theory]
        [InlineData("1000ms")]
        [InlineData("500ms")]
        [InlineData("1ms")]
        [InlineData("99999ms")]
        [InlineData("100 ms")]   // Space is handled by int.TryParse on substring
        public void Validate_WithValidMaxTimeExpectations_ShouldPass(string maxTime)
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
                maxTime
            ));
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("1000")]     // Missing 'ms'
        [InlineData("0ms")]      // Zero
        [InlineData("-100ms")]   // Negative
        [InlineData("abcms")]    // Non-numeric
        [InlineData("")]         // Empty
        [InlineData("1000s")]    // Wrong unit
        [InlineData("ms")]       // Just unit
        public void Validate_WithInvalidMaxTimeExpectations_ShouldReturnError(string maxTime)
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
                maxTime
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
            result.Errors[0].Message.Should().Be($"Invalid max-time expectation '{maxTime}'. Must be a positive number followed by 'ms'");
        }

        [Fact]
        public void Validate_WithEmptyBodyPathExpectation_ShouldReturnError()
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
                ExpectationType.BodyPath,
                ""
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
            result.Errors[0].Message.Should().Be("body-path expectation cannot be empty");
        }

        [Fact]
        public void Validate_WithWhitespaceOnlyBodyPathExpectation_ShouldReturnError()
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
                ExpectationType.BodyPath,
                "   "
            ));
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors[0].Message.Should().Be("body-path expectation cannot be empty");
        }

        [Fact]
        public void Validate_WithEmptySchemaExpectation_ShouldReturnError()
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
                ExpectationType.Schema,
                ""
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
            result.Errors[0].Message.Should().Be("schema expectation cannot be empty");
        }

        [Theory]
        [InlineData(ExpectationType.Header, "Content-Type application/json")]
        [InlineData(ExpectationType.BodyContains, "user")]
        [InlineData(ExpectationType.BodyPath, "$.users[0].name")]
        [InlineData(ExpectationType.Schema, "user-schema.json")]
        public void Validate_WithValidExpectationTypes_ShouldPass(ExpectationType expectationType, string value)
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
                expectationType,
                value
            ));
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region File Variable Validation Tests

        [Fact]
        public void Validate_WithValidFileVariables_ShouldPass()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                ["baseUrl"] = "https://api.example.com",
                ["apiKey"] = "secret123",
                ["version"] = "v1"
            };
            var httpFile = new HttpFile([], fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
        }

        [Fact]
        public void Validate_WithEmptyVariableName_ShouldReturnError()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                [""] = "some-value"
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

        [Fact]
        public void Validate_WithWhitespaceOnlyVariableName_ShouldReturnError()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                ["   "] = "some-value"
            };
            var httpFile = new HttpFile([], fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.Errors[0].Message.Should().Be("Variable name cannot be empty");
        }

        [Theory]
        [InlineData("variable with spaces")]
        [InlineData("variable\twith\ttabs")]
        [InlineData(" variableWithLeadingSpace")]
        [InlineData("variableWithTrailingSpace ")]
        public void Validate_WithWhitespaceInVariableName_ShouldReturnWarning(string variableName)
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                [variableName] = "some-value"
            };
            var httpFile = new HttpFile([], fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be($"Variable name '{variableName}' contains whitespace");
            result.Warnings[0].LineNumber.Should().Be(0);
        }

        [Fact]
        public void Validate_WithCircularReferenceInVariable_ShouldReturnError()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                ["selfRef"] = "prefix-{{selfRef}}-suffix"
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
        public void Validate_WithNonCircularVariableReference_ShouldPass()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                ["baseUrl"] = "https://api.example.com",
                ["fullUrl"] = "{{baseUrl}}/users" // References another variable, not itself
            };
            var httpFile = new HttpFile([], fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Complex Validation Scenarios

        [Fact]
        public void Validate_WithMultipleValidationIssues_ShouldReturnAllErrors()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                [""] = "empty-name",
                ["selfRef"] = "{{selfRef}}"
            };

            var requests = new List<HttpRequest>();

            // Add requests with various issues
            var request1 = new HttpRequest
            {
                Name = "", // Missing name
                Method = "INVALID_METHOD",
                Url = "", // Missing URL
                LineNumber = 1
            };
            request1.Headers[""] = "empty-header-name"; // Empty header name
            requests.Add(request1);

            var request2 = new HttpRequest
            {
                Name = "invalid@name", // Invalid characters
                Method = "GET",
                Url = "https://api.example.com",
                LineNumber = 5
            };
            request2.Metadata.Expectations.Add(new TestExpectation(
                ExpectationType.StatusCode,
                "invalid" // Invalid status code
            ));
            requests.Add(request2);

            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();

            // Should have multiple errors
            result.Errors.Count.Should().BeGreaterThan(3);

            // Should have variable errors
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidVariable);

            // Should have request name errors
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.MissingRequestName);
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidRequestName);

            // Should have HTTP syntax errors
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);

            // Should have expectation errors
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidExpectation);

            // Should have method warning
            result.Warnings.Should().Contain(w => w.Message.Contains("Unknown HTTP method"));
        }

        [Fact]
        public void Validate_WithAllValidData_ShouldReturnSuccess()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                ["baseUrl"] = "https://api.example.com",
                ["apiKey"] = "secret123"
            };

            var requests = new List<HttpRequest>();

            // Add valid requests
            var request1 = new HttpRequest
            {
                Name = "get-users",
                Method = "GET",
                Url = "{{baseUrl}}/users",
                LineNumber = 1
            };
            request1.Headers["Authorization"] = "Bearer {{apiKey}}";
            request1.Headers["Accept"] = "application/json";
            request1.Metadata.Expectations.Add(new TestExpectation(
                ExpectationType.StatusCode,
                "200"
            ));
            requests.Add(request1);

            var request2 = new HttpRequest
            {
                Name = "create-user",
                Method = "POST",
                Url = "{{baseUrl}}/users",
                LineNumber = 10
            };
            request2.Headers["Content-Type"] = "application/json";
            request2.Headers["Authorization"] = "Bearer {{apiKey}}";
            request2.Metadata.Expectations.Add(new TestExpectation(
                ExpectationType.StatusCode,
                "201"
            ));
            request2.Metadata.Expectations.Add(new TestExpectation(
                ExpectationType.MaxTime,
                "5000ms"
            ));
            requests.Add(request2);

            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            ValidationResult result = _validator.Validate(httpFile);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeFalse();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        #endregion
    }
}
