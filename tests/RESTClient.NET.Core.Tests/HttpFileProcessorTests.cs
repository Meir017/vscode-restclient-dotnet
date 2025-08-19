using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Parsing;
using RESTClient.NET.Core.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace RESTClient.NET.Core.Tests
{
    public class HttpFileProcessorTests
    {
        [Fact]
        public void Constructor_WithDefaults_ShouldInitializeCorrectly()
        {
            // Act
            var processor = new HttpFileProcessor();

            // Assert
            processor.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithLogger_ShouldInitializeCorrectly()
        {
            // Arrange
            var logger = new TestLogger<HttpFileProcessor>();

            // Act
            var processor = new HttpFileProcessor(logger);

            // Assert
            processor.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithValidator_ShouldInitializeCorrectly()
        {
            // Arrange
            var validator = new TestValidator();

            // Act
            var processor = new HttpFileProcessor(null, validator);

            // Assert
            processor.Should().NotBeNull();
        }

        [Fact]
        public async Task ParseFileAsync_WithNullPath_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Func<Task> act = async () => await processor.ParseFileAsync(null!);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task ParseFileAsync_WithEmptyPath_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Func<Task> act = async () => await processor.ParseFileAsync("");
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task ParseFileAsync_WithWhitespacePath_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Func<Task> act = async () => await processor.ParseFileAsync("   ");
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task ParseFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var nonExistentPath = "non-existent-file.http";

            // Act & Assert
            Func<Task> act = async () => await processor.ParseFileAsync(nonExistentPath);
            await act.Should().ThrowAsync<FileNotFoundException>();
        }

        [Fact]
        public async Task ParseContentAsync_WithNullContent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Func<Task> act = async () => await processor.ParseContentAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ParseContentAsync_WithValidContent_ShouldReturnHttpFile()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var content = @"
# @name test-request
GET https://api.example.com/users HTTP/1.1
";

            // Act
            var result = await processor.ParseContentAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
            result.Requests[0].Name.Should().Be("test-request");
        }

        [Fact]
        public async Task ParseContentAsync_WithOptions_ShouldUseOptions()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var content = @"
# @name test-request
GET https://api.example.com/users HTTP/1.1
";
            var options = new HttpParseOptions();

            // Act
            var result = await processor.ParseContentAsync(content, options);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ParseAndProcessContentAsync_WithVariables_ShouldResolveVariables()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var content = @"
@baseUrl = https://api.example.com

# @name test-request
GET {{baseUrl}}/users HTTP/1.1
";
            var envVars = new Dictionary<string, string>();

            // Act
            var result = await processor.ParseAndProcessContentAsync(content, envVars);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void ProcessVariables_WithNullHttpFile_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Action act = () => processor.ProcessVariables(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ProcessVariables_WithValidHttpFile_ShouldReturnProcessedFile()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest 
                { 
                    Name = "test", 
                    Method = "GET", 
                    Url = "{{baseUrl}}/users", 
                    LineNumber = 1 
                }
            };
            var fileVariables = new Dictionary<string, string> { { "baseUrl", "https://api.example.com" } };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = processor.ProcessVariables(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void ValidateVariableReferences_WithNullHttpFile_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Action act = () => processor.ValidateVariableReferences(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidateVariableReferences_WithValidHttpFile_ShouldReturnValidationResult()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest 
                { 
                    Name = "test", 
                    Method = "GET", 
                    Url = "https://api.example.com/users", 
                    LineNumber = 1 
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = processor.ValidateVariableReferences(httpFile);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void ValidateVariableReferences_WithCircularReference_ShouldReturnErrors()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest 
                { 
                    Name = "test", 
                    Method = "GET", 
                    Url = "https://api.example.com/users", 
                    LineNumber = 1 
                }
            };
            var fileVariables = new Dictionary<string, string>
            {
                { "var1", "{{var2}}" },
                { "var2", "{{var1}}" }  // Circular reference
            };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = processor.ValidateVariableReferences(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void GetProcessedRequest_WithNullHttpFile_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Action act = () => processor.GetProcessedRequest(null!, "test");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetProcessedRequest_WithNullRequestName_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var httpFile = new HttpFile(new List<HttpRequest>());

            // Act & Assert
            Action act = () => processor.GetProcessedRequest(httpFile, null!);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetProcessedRequest_WithEmptyRequestName_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var httpFile = new HttpFile(new List<HttpRequest>());

            // Act & Assert
            Action act = () => processor.GetProcessedRequest(httpFile, "");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetProcessedRequest_WithWhitespaceRequestName_ShouldThrowArgumentException()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var httpFile = new HttpFile(new List<HttpRequest>());

            // Act & Assert
            Action act = () => processor.GetProcessedRequest(httpFile, "   ");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetProcessedRequest_WithNonExistentRequest_ShouldReturnNull()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "existing-request", Method = "GET", Url = "https://api.example.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = processor.GetProcessedRequest(httpFile, "non-existent-request");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetProcessedRequest_WithExistingRequest_ShouldReturnProcessedRequest()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest 
                { 
                    Name = "test-request", 
                    Method = "GET", 
                    Url = "{{baseUrl}}/users",
                    LineNumber = 1
                }
            };
            var fileVariables = new Dictionary<string, string> { { "baseUrl", "https://api.example.com" } };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = processor.GetProcessedRequest(httpFile, "test-request");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("test-request");
        }

        [Fact]
        public void GetAllProcessedRequests_WithNullHttpFile_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Action act = () => processor.GetAllProcessedRequests(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetAllProcessedRequests_WithValidHttpFile_ShouldReturnAllProcessedRequests()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest 
                { 
                    Name = "request1", 
                    Method = "GET", 
                    Url = "{{baseUrl}}/users",
                    LineNumber = 1
                },
                new HttpRequest 
                { 
                    Name = "request2", 
                    Method = "POST", 
                    Url = "{{baseUrl}}/posts",
                    LineNumber = 5
                }
            };
            var fileVariables = new Dictionary<string, string> { { "baseUrl", "https://api.example.com" } };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = processor.GetAllProcessedRequests(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void ValidateHttpFile_WithNullHttpFile_ShouldThrowArgumentNullException()
        {
            // Arrange
            var processor = new HttpFileProcessor();

            // Act & Assert
            Action act = () => processor.ValidateHttpFile(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidateHttpFile_WithValidHttpFile_ShouldReturnValidationResult()
        {
            // Arrange
            var processor = new HttpFileProcessor();
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
            var httpFile = new HttpFile(requests);

            // Act
            var result = processor.ValidateHttpFile(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateHttpFile_WithCustomValidator_ShouldUseCustomValidator()
        {
            // Arrange
            var validator = new TestValidator();
            var processor = new HttpFileProcessor(null, validator);
            var httpFile = new HttpFile(new List<HttpRequest>());

            // Act
            var result = processor.ValidateHttpFile(httpFile);

            // Assert
            result.Should().NotBeNull();
            validator.ValidateCalled.Should().BeTrue();
        }

        [Fact]
        public void ProcessVariables_WithEnvironmentVariables_ShouldResolveWithEnvironmentVariables()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest 
                { 
                    Name = "test", 
                    Method = "GET", 
                    Url = "{{baseUrl}}/{{endpoint}}", 
                    LineNumber = 1 
                }
            };
            var fileVariables = new Dictionary<string, string> { { "baseUrl", "https://api.example.com" } };
            var envVariables = new Dictionary<string, string> { { "endpoint", "users" } };
            var httpFile = new HttpFile(requests, fileVariables);

            // Act
            var result = processor.ProcessVariables(httpFile, envVariables);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);
        }

        [Fact]
        public void ValidateVariableReferences_WithUnresolvedVariables_ShouldReturnWarnings()
        {
            // Arrange
            var processor = new HttpFileProcessor();
            var requests = new List<HttpRequest>
            {
                new HttpRequest 
                { 
                    Name = "test", 
                    Method = "GET", 
                    Url = "{{undefinedVariable}}/users", 
                    LineNumber = 1 
                }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = processor.ValidateVariableReferences(httpFile);

            // Assert
            result.Should().NotBeNull();
            result.HasWarnings.Should().BeTrue();
        }

        // Test helper classes
        private class TestLogger<T> : ILogger<T>
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }

        private class TestValidator : IHttpFileValidator
        {
            public bool ValidateCalled { get; private set; }

            public ValidationResult Validate(HttpFile httpFile)
            {
                ValidateCalled = true;
                return new ValidationResult(new List<ValidationError>(), new List<ValidationWarning>());
            }
        }
    }
}
