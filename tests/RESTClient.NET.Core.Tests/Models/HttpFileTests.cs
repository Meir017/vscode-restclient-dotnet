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
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" },
                new HttpRequest { Name = "test2", Method = "POST", Url = "http://api.com" }
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
        public void TryGetRequestByName_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" },
                new HttpRequest { Name = "test2", Method = "POST", Url = "http://api.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = httpFile.TryGetRequestByName("test1", out var request);

            // Assert
            result.Should().BeTrue();
            request.Should().NotBeNull();
            request!.Name.Should().Be("test1");
            request.Method.Should().Be("GET");
        }

        [Fact]
        public void TryGetRequestByName_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var result = httpFile.TryGetRequestByName("nonexistent", out var request);

            // Assert
            result.Should().BeFalse();
            request.Should().BeNull();
        }

        [Fact]
        public void TryGetRequestByName_WithNullOrEmptyId_ShouldReturnFalse()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act & Assert
            httpFile.TryGetRequestByName(null, out var request1).Should().BeFalse();
            request1.Should().BeNull();

            httpFile.TryGetRequestByName("", out var request2).Should().BeFalse();
            request2.Should().BeNull();

            httpFile.TryGetRequestByName("   ", out var request3).Should().BeFalse();
            request3.Should().BeNull();
        }

        [Fact]
        public void GetRequestByName_WithExistingId_ShouldReturnRequest()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" },
                new HttpRequest { Name = "test2", Method = "POST", Url = "http://api.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act
            var request = httpFile.GetRequestByName("test2");

            // Assert
            request.Should().NotBeNull();
            request.Name.Should().Be("test2");
            request.Method.Should().Be("POST");
        }

        [Fact]
        public void GetRequestByName_WithNonExistingId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" }
            };
            var httpFile = new HttpFile(requests);

            // Act & Assert
            Action act = () => httpFile.GetRequestByName("nonexistent");
            act.Should().Throw<KeyNotFoundException>()
                .WithMessage("Request with name 'nonexistent' not found");
        }

        [Fact]
        public void Constructor_WithDuplicateNames_ShouldStoreAllRequests()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "duplicate", Method = "GET", Url = "http://example.com" },
                new HttpRequest { Name = "duplicate", Method = "POST", Url = "http://api.com" },
                new HttpRequest { Name = "unique", Method = "PUT", Url = "http://other.com" }
            };

            // Act
            var httpFile = new HttpFile(requests);

            // Assert
            httpFile.Requests.Should().HaveCount(3);
            
            // The lookup should return the first occurrence
            var foundRequest = httpFile.GetRequestByName("duplicate");
            foundRequest.Method.Should().Be("GET");
        }

        [Fact]
        public void Constructor_WithEmptyFileVariables_ShouldInitializeEmptyDictionary()
        {
            // Arrange
            var requests = new List<HttpRequest>
            {
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" }
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
                new HttpRequest { Name = "test1", Method = "GET", Url = "http://example.com" }
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
