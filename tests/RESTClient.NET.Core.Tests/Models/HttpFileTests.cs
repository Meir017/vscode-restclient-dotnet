using FluentAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class HttpFileTests
    {
        [Fact]
        public void Constructor_WithRequests_ShouldInitializeCorrectly()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" },
                new HttpRequest { RequestId = "test2", Method = "POST", Url = "http://api.com" }
            };

            var fileVariables = new Dictionary<string, string>
            {
                { "baseUrl", "http://localhost" },
                { "apiKey", "secret123" }
            };

            // Act
            var httpFile = new HttpFile(requests, fileVariables);

            // Assert
            httpFile.Requests.Should().HaveCount(2);
            httpFile.Requests.Should().BeEquivalentTo(requests);
            httpFile.FileVariables.Should().HaveCount(2);
            httpFile.FileVariables["baseUrl"].Should().Be("http://localhost");
            httpFile.FileVariables["apiKey"].Should().Be("secret123");
        }

        [Fact]
        public void Constructor_WithNullRequests_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Action act = () => new HttpFile(null);
            act.Should().Throw<ArgumentNullException>().WithParameterName("requests");
        }

        [Fact]
        public void TryGetRequestById_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" },
                new HttpRequest { RequestId = "test2", Method = "POST", Url = "http://api.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = httpFile.TryGetRequestById("test1", out var request);

            // Assert
            result.Should().BeTrue();
            request.Should().NotBeNull();
            request!.RequestId.Should().Be("test1");
            request.Method.Should().Be("GET");
        }

        [Fact]
        public void TryGetRequestById_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = httpFile.TryGetRequestById("nonexistent", out var request);

            // Assert
            result.Should().BeFalse();
            request.Should().BeNull();
        }

        [Fact]
        public void TryGetRequestById_WithNullOrEmptyId_ShouldReturnFalse()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act & Assert
            httpFile.TryGetRequestById(null, out var request1).Should().BeFalse();
            request1.Should().BeNull();

            httpFile.TryGetRequestById("", out var request2).Should().BeFalse();
            request2.Should().BeNull();

            httpFile.TryGetRequestById("   ", out var request3).Should().BeFalse();
            request3.Should().BeNull();
        }

        [Fact]
        public void GetRequestById_WithExistingId_ShouldReturnRequest()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" },
                new HttpRequest { RequestId = "test2", Method = "POST", Url = "http://api.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var request = httpFile.GetRequestById("test2");

            // Assert
            request.Should().NotBeNull();
            request.RequestId.Should().Be("test2");
            request.Method.Should().Be("POST");
        }

        [Fact]
        public void GetRequestById_WithNonExistingId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act & Assert
            Action act = () => httpFile.GetRequestById("nonexistent");
            act.Should().Throw<KeyNotFoundException>()
                .WithMessage("Request with name 'nonexistent' not found");
        }

        [Fact]
        public void Constructor_WithDuplicateRequestIds_ShouldStoreAllRequests()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "duplicate", Method = "GET", Url = "http://example.com" },
                new HttpRequest { RequestId = "duplicate", Method = "POST", Url = "http://api.com" },
                new HttpRequest { RequestId = "unique", Method = "PUT", Url = "http://other.com" }
            };

            // Act
            var httpFile = new HttpFile(requests);

            // Assert
            httpFile.Requests.Should().HaveCount(3);
            
            // The lookup should return the first occurrence
            var foundRequest = httpFile.GetRequestById("duplicate");
            foundRequest.Method.Should().Be("GET");
        }

        [Fact]
        public void Constructor_WithEmptyFileVariables_ShouldInitializeEmptyDictionary()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" }
            };

            // Act
            var httpFile = new HttpFile(requests);

            // Assert
            httpFile.FileVariables.Should().NotBeNull();
            httpFile.FileVariables.Should().BeEmpty();
        }

        [Fact]
        public void FileVariables_ShouldBeCaseInsensitive()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { RequestId = "test1", Method = "GET", Url = "http://example.com" }
            };

            var fileVariables = new Dictionary<string, string>
            {
                { "BaseUrl", "http://localhost" },
                { "apikey", "secret123" }
            };

            // Act
            var httpFile = new HttpFile(requests, fileVariables);

            // Assert
            httpFile.FileVariables.Should().ContainKey("BaseUrl");
            httpFile.FileVariables.Should().ContainKey("apikey");
            httpFile.FileVariables["BaseUrl"].Should().Be("http://localhost");
            httpFile.FileVariables["apikey"].Should().Be("secret123");
        }
    }
}
