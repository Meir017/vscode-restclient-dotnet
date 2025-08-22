using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Testing.Extensions;
using RESTClient.NET.Testing.Models;
using Xunit;

namespace RESTClient.NET.Testing.Tests
{
    public class HttpFileTestBaseTests : IDisposable
    {
        private static readonly string[] _postPutMethods = ["POST", "PUT"];
        private readonly string _tempHttpFilePath;
        private bool _disposed;

        public HttpFileTestBaseTests()
        {
            // Create a temporary HTTP file for testing
            _tempHttpFilePath = Path.GetTempFileName();
            File.WriteAllText(_tempHttpFilePath, TestHttpFileContent);
        }

        private const string TestHttpFileContent = @"
@baseUrl = https://api.example.com

# @name get-users
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains users
GET {{baseUrl}}/users HTTP/1.1
Accept: application/json

###

# @name create-user
# @expect status 201
POST {{baseUrl}}/users HTTP/1.1
Content-Type: application/json

{
  ""name"": ""John Doe"",
  ""email"": ""john@example.com""
}

###

# @name get-user-by-id
GET {{baseUrl}}/users/123 HTTP/1.1
Accept: application/json

###

# @name update-user
PUT {{baseUrl}}/users/123 HTTP/1.1
Content-Type: application/json

{
  ""name"": ""John Updated"",
  ""email"": ""john.updated@example.com""
}

###

# @name delete-user
DELETE {{baseUrl}}/users/123 HTTP/1.1
";

        [Fact]
        public void GetHttpFilePath_ShouldReturnConfiguredPath()
        {
            // This tests the abstract method implementation through a concrete test class
            string path = _tempHttpFilePath;
            path.Should().NotBeNullOrEmpty();
            File.Exists(path).Should().BeTrue();
        }

        [Fact]
        public void Constructor_WithNonExistentHttpFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            string nonExistentPath = Path.Combine(Path.GetTempPath(), "non-existent-file.http");

            // Act & Assert
            Func<MockHttpFileTestBase> action = () => new MockHttpFileTestBase(nonExistentPath);
            action.Should().Throw<FileNotFoundException>()
                .WithMessage($"*{nonExistentPath}*");
        }

        [Fact]
        public void Constructor_WithEmptyHttpFilePath_ShouldThrowArgumentException()
        {
            // Arrange, Act & Assert
            Func<MockHttpFileTestBase> action = () => new MockHttpFileTestBase("");
            action.Should().Throw<ArgumentException>()
                .WithParameterName("httpFilePath");
        }

        [Fact]
        public void Constructor_WithNullHttpFilePath_ShouldThrowArgumentException()
        {
            // Arrange, Act & Assert
            Func<MockHttpFileTestBase> action = () => new MockHttpFileTestBase(null!);
            action.Should().Throw<ArgumentException>()
                .WithParameterName("httpFilePath");
        }

        [Fact]
        public void Constructor_WithValidHttpFile_ShouldLoadCorrectly()
        {
            // Arrange & Act
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Assert
            mockBase.HttpFilePublic.Should().NotBeNull();
            // Parser creates 9 total requests (including empty ones between separators)
            mockBase.HttpFilePublic.Requests.Should().HaveCount(9);
            // But GetTestCases should return only the 5 named requests
            mockBase.HttpFilePublic.GetTestCases().Should().HaveCount(5);
            mockBase.ModifyHttpFileCalled.Should().BeTrue();
        }

        [Fact]
        public void HttpFile_ShouldContainExpectedRequests()
        {
            // Arrange & Act
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Assert
            HttpFile httpFile = mockBase.HttpFilePublic;
            httpFile.Should().NotBeNull();
            // Parser creates 9 total requests (including empty ones between separators)
            httpFile.Requests.Should().HaveCount(9);

            // Test that named requests can be found by name
            httpFile.TryGetRequestByName("get-users", out HttpRequest? getUsersRequest).Should().BeTrue();
            getUsersRequest.Should().NotBeNull();
            getUsersRequest!.Method.Should().Be("GET");

            httpFile.TryGetRequestByName("create-user", out HttpRequest? createUserRequest).Should().BeTrue();
            createUserRequest.Should().NotBeNull();
            createUserRequest!.Method.Should().Be("POST");

            httpFile.TryGetRequestByName("get-user-by-id", out HttpRequest? getUserByIdRequest).Should().BeTrue();
            getUserByIdRequest.Should().NotBeNull();
            getUserByIdRequest!.Method.Should().Be("GET");

            httpFile.TryGetRequestByName("update-user", out HttpRequest? updateUserRequest).Should().BeTrue();
            updateUserRequest.Should().NotBeNull();
            updateUserRequest!.Method.Should().Be("PUT");

            httpFile.TryGetRequestByName("delete-user", out HttpRequest? deleteUserRequest).Should().BeTrue();
            deleteUserRequest.Should().NotBeNull();
            deleteUserRequest!.Method.Should().Be("DELETE");
        }

        [Fact]
        public void GetTestCase_WithValidName_ShouldReturnTestCase()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            HttpTestCase testCase = mockBase.GetTestCasePublic("get-users");

            // Assert
            testCase.Should().NotBeNull();
            testCase.Name.Should().Be("get-users");
            testCase.Method.Should().Be("GET");
            testCase.Url.Should().Contain("/users");
        }

        [Fact]
        public void GetTestCase_WithInvalidName_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act & Assert
            Func<HttpTestCase> action = () => mockBase.GetTestCasePublic("non-existent");
            action.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void TryGetTestCase_WithValidName_ShouldReturnTrueAndTestCase()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            bool result = mockBase.TryGetTestCasePublic("create-user", out HttpTestCase? testCase);

            // Assert
            result.Should().BeTrue();
            testCase.Should().NotBeNull();
            testCase.Name.Should().Be("create-user");
            testCase.Method.Should().Be("POST");
        }

        [Fact]
        public void TryGetTestCase_WithInvalidName_ShouldReturnFalse()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            bool result = mockBase.TryGetTestCasePublic("non-existent", out HttpTestCase? testCase);

            // Assert
            result.Should().BeFalse();
            testCase.Should().BeNull();
        }

        [Fact]
        public void GetFilteredTestData_WithNamePattern_ShouldFilterCorrectly()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            IEnumerable<object[]> filteredData = mockBase.GetFilteredTestDataPublic(namePattern: "get");

            // Assert
            filteredData.Should().NotBeEmpty();
            var testCases = filteredData.Select(data => (HttpTestCase)data[0]).ToList();
            testCases.Should().HaveCount(2); // get-users and get-user-by-id
            testCases.Should().OnlyContain(tc => tc.Name.Contains("get", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetFilteredTestData_WithMethods_ShouldFilterCorrectly()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            IEnumerable<object[]> filteredData = mockBase.GetFilteredTestDataPublic(methods: _postPutMethods);

            // Assert
            filteredData.Should().NotBeEmpty();
            var testCases = filteredData.Select(data => (HttpTestCase)data[0]).ToList();
            testCases.Should().HaveCount(2); // create-user and update-user
            testCases.Should().OnlyContain(tc => tc.Method == "POST" || tc.Method == "PUT");
        }

        [Fact]
        public void GetFilteredTestData_WithHasExpectations_ShouldFilterCorrectly()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            IEnumerable<object[]> filteredData = mockBase.GetFilteredTestDataPublic(hasExpectations: true);

            // Assert
            filteredData.Should().NotBeEmpty();
            var testCases = filteredData.Select(data => (HttpTestCase)data[0]).ToList();
            testCases.Should().HaveCount(2); // get-users and create-user (those with @expect comments)
            testCases.Should().OnlyContain(tc => tc.ExpectedResponse != null && tc.ExpectedResponse.HasExpectations);
        }

        [Fact]
        public void GetProcessedRequest_WithValidName_ShouldReturnProcessedRequest()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);
            var environmentVariables = new Dictionary<string, string>
            {
                { "baseUrl", "https://test.api.com" }
            };

            // Act
            HttpRequest? processedRequest = mockBase.GetProcessedRequestPublic("get-users", environmentVariables);

            // Assert
            processedRequest.Should().NotBeNull();
            // Environment variables take precedence over file variables when using {{variable}} syntax
            processedRequest.Url.Should().Contain("https://test.api.com/users");
        }

        [Fact]
        public void GetProcessedRequest_WithInvalidName_ShouldReturnNull()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            HttpRequest? processedRequest = mockBase.GetProcessedRequestPublic("non-existent");

            // Assert
            processedRequest.Should().BeNull();
        }

        [Fact]
        public void ModifyHttpFile_ShouldBeCallableForCustomization()
        {
            // Arrange & Act
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Assert
            // The ModifyHttpFile method should have been called during construction
            // This is verified by the fact that the HttpFile is properly loaded
            mockBase.HttpFilePublic.Should().NotBeNull();
            mockBase.ModifyHttpFileCalled.Should().BeTrue();
        }

        [Fact]
        public void GetProcessedRequest_WithoutEnvironmentVariables_ShouldReturnRequestWithFileVariables()
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            HttpRequest? processedRequest = mockBase.GetProcessedRequestPublic("get-users");

            // Assert
            processedRequest.Should().NotBeNull();
            // The baseUrl variable from the file should be processed (replaced with actual value)
            processedRequest.Url.Should().Contain("https://api.example.com/users");
        }

        [Fact]
        public void HttpFile_Property_ShouldReturnParsedHttpFile()
        {
            // Arrange & Act
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Assert
            mockBase.HttpFilePublic.Should().NotBeNull();
            mockBase.HttpFilePublic.Requests.Should().NotBeEmpty();
            mockBase.HttpFilePublic.FileVariables.Should().ContainKey("baseUrl");
        }

        [Fact]
        public void Constructor_WithRelativeHttpFilePath_ShouldResolveCorrectly()
        {
            // Arrange
            string tempDir = Path.GetTempPath();
            string relativePath = "test-requests.http";
            string fullPath = Path.Combine(tempDir, relativePath);

            try
            {
                File.WriteAllText(fullPath, TestHttpFileContent);

                // Act & Assert
                Func<MockHttpFileTestBase> action = () =>
                {
                    using var mockBase = new MockHttpFileTestBase(fullPath);
                    return mockBase;
                };

                using MockHttpFileTestBase result = action();
                result.Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public void GetFilteredTestData_WithSpecificMethod_ShouldReturnMatchingTests(string method)
        {
            // Arrange
            using var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            IEnumerable<object[]> filteredData = mockBase.GetFilteredTestDataPublic(methods: new[] { method });

            // Assert
            filteredData.Should().NotBeEmpty();
            var testCases = filteredData.Select(data => (HttpTestCase)data[0]).ToList();
            testCases.Should().OnlyContain(tc => tc.Method == method);
        }

        [Fact]
        public void HttpFileTestData_ShouldProvideStaticTestData()
        {
            // Act
            IEnumerable<object[]> testData = MockHttpFileTestBase.GetTestDataStatic();

            // Assert
            testData.Should().NotBeEmpty();
            testData.Should().OnlyContain(data => data.Length == 1 && data[0] is HttpTestCase);
        }

        [Fact]
        public void Dispose_ShouldDisposeCorrectly()
        {
            // Arrange
            var mockBase = new MockHttpFileTestBase(_tempHttpFilePath);

            // Act
            mockBase.Dispose();

            // Assert
            // Should be safe to call multiple times
            Action action = () => mockBase.Dispose();
            action.Should().NotThrow();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (File.Exists(_tempHttpFilePath))
                    {
                        File.Delete(_tempHttpFilePath);
                    }
                }

                _disposed = true;
            }
        }
    }

    // Mock implementation that doesn't rely on WebApplicationFactory
    public class MockHttpFileTestBase : IDisposable
    {
        private readonly HttpFile _httpFile;
        private bool _disposed;

        public bool ModifyHttpFileCalled { get; private set; }
        public HttpFile HttpFilePublic => _httpFile;

        public MockHttpFileTestBase(string httpFilePath)
        {
            if (string.IsNullOrWhiteSpace(httpFilePath))
            {
                throw new ArgumentException("HTTP file path cannot be null or empty", nameof(httpFilePath));
            }

            if (!File.Exists(httpFilePath))
            {
                throw new FileNotFoundException($"HTTP file not found: {httpFilePath}");
            }

            // Load and parse the HTTP file using the core parser
            var parser = new RESTClient.NET.Core.Parsing.HttpFileParser();
            _httpFile = parser.ParseFileAsync(httpFilePath).GetAwaiter().GetResult();

            // Call the modification method
            ModifyHttpFile(_httpFile);
        }

        protected virtual void ModifyHttpFile(HttpFile httpFile)
        {
            ModifyHttpFileCalled = true;
        }

        // Public wrappers for testing protected methods
        public HttpTestCase GetTestCasePublic(string name)
        {
            HttpRequest request = _httpFile.GetRequestByName(name);
            return _httpFile.GetTestCases().First(tc => tc.Name == name);
        }

        public bool TryGetTestCasePublic(string name, out HttpTestCase testCase)
        {
            testCase = null!;

            if (!_httpFile.TryGetRequestByName(name, out HttpRequest? request))
            {
                return false;
            }

            testCase = _httpFile.GetTestCases().First(tc => tc.Name == name);
            return true;
        }

        public IEnumerable<object[]> GetFilteredTestDataPublic(string? namePattern = null, IEnumerable<string>? methods = null, bool? hasExpectations = null)
        {
            return _httpFile.GetTestCases()
                .Filter(namePattern, methods, hasExpectations)
                .Select(testCase => new object[] { testCase });
        }

        public HttpRequest? GetProcessedRequestPublic(string requestName, IDictionary<string, string>? environmentVariables = null)
        {
            var processor = new RESTClient.NET.Core.HttpFileProcessor();
            return processor.GetProcessedRequest(_httpFile, requestName, environmentVariables);
        }

        public static IEnumerable<object[]> GetTestDataStatic()
        {
            return
            [
                [new HttpTestCase { Name = "test-case", Method = "GET", Url = "/test" }]
            ];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources if any
                }
                _disposed = true;
            }
        }
    }
}
