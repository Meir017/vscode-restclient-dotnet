using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Validation;
using System;
using System.Collections.Generic;
using Xunit;

namespace RESTClient.NET.Core.Tests.Validation
{
    public class HttpFileValidatorTests
    {
        [Fact]
        public void Validate_WithNullHttpFile_ShouldThrowArgumentNullException()
        {
            // Arrange
            var validator = new HttpFileValidator();

            // Act & Assert
            Action act = () => validator.Validate(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Validate_WithValidHttpFile_ShouldReturnValidResult()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "valid-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithInvalidRequestName_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "invalid name with spaces",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidRequestName);
        }

        [Fact]
        public void Validate_WithDuplicateRequestNames_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "duplicate-name",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                },
                new HttpRequest
                {
                    Name = "duplicate-name",
                    Method = "POST",
                    Url = "https://api.example.com/users",
                    LineNumber = 5
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.DuplicateRequestName);
        }

        [Fact]
        public void Validate_WithEmptyRequestName_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.MissingRequestName);
        }

        [Fact]
        public void Validate_WithNullRequestName_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = null!,
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.MissingRequestName);
        }

        [Fact]
        public void Validate_WithInvalidHttpMethod_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "INVALID_METHOD",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);
        }

        [Fact]
        public void Validate_WithEmptyHttpMethod_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);
        }

        [Fact]
        public void Validate_WithEmptyUrl_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);
        }

        [Fact]
        public void Validate_WithInvalidUrl_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "not-a-valid-url",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);
        }

        [Fact]
        public void Validate_WithInvalidHeaderFormat_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "GET",
                Url = "https://api.example.com/users",
                LineNumber = 1
            };
            request.SetHeader("Invalid Header", "value"); // Header with space
            
            var requests = new List<HttpRequest> { request };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);
        }

        [Fact]
        public void Validate_WithEmptyHeaderName_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var headers = new Dictionary<string, string>
            {
                { "", "value" }
            };
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    Headers = headers,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);
        }

        [Fact]
        public void Validate_WithMultipleContentTypeHeaders_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "content-type", "text/plain" } // Case-insensitive duplicate
            };
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "POST",
                    Url = "https://api.example.com/users",
                    Headers = headers,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidHttpSyntax);
        }

        [Fact]
        public void Validate_WithInvalidExpectationFormat_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var metadata = new HttpRequestMetadata();
            metadata.Expectations.Add(new ExpectationMetadata
            {
                Type = "invalid-type",
                Value = "some-value",
                LineNumber = 2
            });
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    Metadata = metadata,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidExpectation);
        }

        [Fact]
        public void Validate_WithInvalidStatusCodeExpectation_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var metadata = new HttpRequestMetadata();
            metadata.Expectations.Add(new ExpectationMetadata
            {
                Type = "status",
                Value = "invalid-status",
                LineNumber = 2
            });
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    Metadata = metadata,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidExpectation);
        }

        [Fact]
        public void Validate_WithInvalidFileVariableName_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var fileVariables = new Dictionary<string, string>
            {
                { "invalid variable name", "value" } // Variable name with spaces
            };
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidVariable);
        }

        [Fact]
        public void Validate_WithEmptyFileVariableName_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var fileVariables = new Dictionary<string, string>
            {
                { "", "value" }
            };
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidVariable);
        }

        [Fact]
        public void Validate_WithCircularFileVariableReference_ShouldReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var fileVariables = new Dictionary<string, string>
            {
                { "var1", "{{var2}}" },
                { "var2", "{{var1}}" }
            };
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Type == ValidationErrorType.InvalidVariable);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        [InlineData("HEAD")]
        [InlineData("OPTIONS")]
        public void Validate_WithValidHttpMethods_ShouldNotReturnErrors(string method)
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = method,
                    Url = "https://api.example.com/users",
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("https://api.example.com")]
        [InlineData("http://localhost:8080")]
        [InlineData("https://subdomain.example.com/path")]
        [InlineData("http://192.168.1.1:3000/api/v1")]
        public void Validate_WithValidUrls_ShouldNotReturnErrors(string url)
        {
            // Arrange
            var validator = new HttpFileValidator();
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = url,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithValidHeaders_ShouldNotReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Authorization", "Bearer token123" },
                { "X-Custom-Header", "custom-value" }
            };
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "POST",
                    Url = "https://api.example.com/users",
                    Headers = headers,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithValidExpectations_ShouldNotReturnErrors()
        {
            // Arrange
            var validator = new HttpFileValidator();
            var metadata = new HttpRequestMetadata();
            metadata.Expectations.Add(new ExpectationMetadata
            {
                Type = "status",
                Value = "200",
                LineNumber = 2
            });
            metadata.Expectations.Add(new ExpectationMetadata
            {
                Type = "header",
                Value = "Content-Type application/json",
                LineNumber = 3
            });
            var requests = new List<HttpRequest>
            {
                new HttpRequest
                {
                    Name = "test-request",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    Metadata = metadata,
                    LineNumber = 1
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = validator.Validate(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }
    }
}
