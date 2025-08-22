using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class HttpRequestTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaults()
        {
            // Act
            var request = new HttpRequest();

            // Assert
            request.Name.Should().Be(string.Empty);
            request.Method.Should().Be("GET");
            request.Url.Should().Be(string.Empty);
            request.Body.Should().BeNull();
            request.FileBodyReference.Should().BeNull();
            request.LineNumber.Should().Be(0);
            request.Headers.Should().NotBeNull().And.BeEmpty();
            request.Metadata.Should().NotBeNull();
        }

        [Fact]
        public void Headers_ShouldBeCaseInsensitive()
        {
            // Arrange
            var request = new HttpRequest();

            // Act
            request.Headers["Content-Type"] = "application/json";
            request.Headers["content-type"] = "text/plain";

            // Assert
            request.Headers.Should().HaveCount(1);
            request.Headers["Content-Type"].Should().Be("text/plain");
            request.Headers["CONTENT-TYPE"].Should().Be("text/plain");
        }

        [Fact]
        public void GetHeader_WithExistingHeader_ShouldReturnValue()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Authorization"] = "Bearer token123";

            // Act
            string? result = request.GetHeader("Authorization");

            // Assert
            result.Should().Be("Bearer token123");
        }

        [Fact]
        public void GetHeader_WithExistingHeaderDifferentCase_ShouldReturnValue()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Authorization"] = "Bearer token123";

            // Act
            string? result = request.GetHeader("AUTHORIZATION");

            // Assert
            result.Should().Be("Bearer token123");
        }

        [Fact]
        public void GetHeader_WithNonExistentHeader_ShouldReturnNull()
        {
            // Arrange
            var request = new HttpRequest();

            // Act
            string? result = request.GetHeader("NonExistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void SetHeader_ShouldAddNewHeader()
        {
            // Arrange
            var request = new HttpRequest();

            // Act
            request.SetHeader("Content-Type", "application/json");

            // Assert
            request.Headers.Should().HaveCount(1);
            request.Headers["Content-Type"].Should().Be("application/json");
        }

        [Fact]
        public void SetHeader_WithExistingHeader_ShouldOverwriteValue()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Content-Type"] = "text/plain";

            // Act
            request.SetHeader("Content-Type", "application/json");

            // Assert
            request.Headers.Should().HaveCount(1);
            request.Headers["Content-Type"].Should().Be("application/json");
        }

        [Fact]
        public void SetHeader_WithDifferentCase_ShouldOverwriteValue()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Content-Type"] = "text/plain";

            // Act
            request.SetHeader("CONTENT-TYPE", "application/json");

            // Assert
            request.Headers.Should().HaveCount(1);
            request.Headers["Content-Type"].Should().Be("application/json");
        }

        [Fact]
        public void HasHeader_WithExistingHeader_ShouldReturnTrue()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Authorization"] = "Bearer token123";

            // Act
            bool result = request.HasHeader("Authorization");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasHeader_WithExistingHeaderDifferentCase_ShouldReturnTrue()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Authorization"] = "Bearer token123";

            // Act
            bool result = request.HasHeader("AUTHORIZATION");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasHeader_WithNonExistentHeader_ShouldReturnFalse()
        {
            // Arrange
            var request = new HttpRequest();

            // Act
            bool result = request.HasHeader("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RemoveHeader_WithExistingHeader_ShouldReturnTrueAndRemoveHeader()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Authorization"] = "Bearer token123";
            request.Headers["Content-Type"] = "application/json";

            // Act
            bool result = request.RemoveHeader("Authorization");

            // Assert
            result.Should().BeTrue();
            request.Headers.Should().HaveCount(1);
            request.Headers.Should().ContainKey("Content-Type");
            request.Headers.Should().NotContainKey("Authorization");
        }

        [Fact]
        public void RemoveHeader_WithExistingHeaderDifferentCase_ShouldReturnTrueAndRemoveHeader()
        {
            // Arrange
            var request = new HttpRequest();
            request.Headers["Authorization"] = "Bearer token123";

            // Act
            bool result = request.RemoveHeader("AUTHORIZATION");

            // Assert
            result.Should().BeTrue();
            request.Headers.Should().BeEmpty();
        }

        [Fact]
        public void RemoveHeader_WithNonExistentHeader_ShouldReturnFalse()
        {
            // Arrange
            var request = new HttpRequest();

            // Act
            bool result = request.RemoveHeader("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var request = new HttpRequest
            {
                Name = "test-request",
                Method = "POST",
                Url = "https://api.example.com/users"
            };

            // Act
            string result = request.ToString();

            // Assert
            result.Should().Be("test-request: POST https://api.example.com/users");
        }

        [Fact]
        public void ToString_WithEmptyName_ShouldReturnFormattedString()
        {
            // Arrange
            var request = new HttpRequest
            {
                Method = "GET",
                Url = "https://api.example.com/users"
            };

            // Act
            string result = request.ToString();

            // Assert
            result.Should().Be(": GET https://api.example.com/users");
        }

        [Fact]
        public void Properties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var request = new HttpRequest();
            var fileBodyRef = new FileBodyReference("test.json", true, System.Text.Encoding.UTF8, 1);

            // Act
            request.Name = "test-request";
            request.Method = "POST";
            request.Url = "https://api.example.com/users";
            request.Body = "test body";
            request.FileBodyReference = fileBodyRef;
            request.LineNumber = 42;

            // Assert
            request.Name.Should().Be("test-request");
            request.Method.Should().Be("POST");
            request.Url.Should().Be("https://api.example.com/users");
            request.Body.Should().Be("test body");
            request.FileBodyReference.Should().Be(fileBodyRef);
            request.LineNumber.Should().Be(42);
        }

        [Fact]
        public void Headers_MultipleDifferentHeaders_ShouldStoreCorrectly()
        {
            // Arrange
            var request = new HttpRequest();

            // Act
            request.SetHeader("Content-Type", "application/json");
            request.SetHeader("Authorization", "Bearer token123");
            request.SetHeader("X-Custom-Header", "custom-value");
            request.SetHeader("Accept", "application/json");

            // Assert
            request.Headers.Should().HaveCount(4);
            request.GetHeader("Content-Type").Should().Be("application/json");
            request.GetHeader("Authorization").Should().Be("Bearer token123");
            request.GetHeader("X-Custom-Header").Should().Be("custom-value");
            request.GetHeader("Accept").Should().Be("application/json");
        }

        [Theory]
        [InlineData("Content-Type")]
        [InlineData("content-type")]
        [InlineData("CONTENT-TYPE")]
        [InlineData("Content-type")]
        [InlineData("CoNtEnT-TyPe")]
        public void HeaderOperations_ShouldBeCaseInsensitive(string headerName)
        {
            // Arrange
            var request = new HttpRequest();
            request.SetHeader("Content-Type", "application/json");

            // Act & Assert
            request.HasHeader(headerName).Should().BeTrue();
            request.GetHeader(headerName).Should().Be("application/json");

            request.SetHeader(headerName, "text/plain");
            request.GetHeader("Content-Type").Should().Be("text/plain");
            request.Headers.Should().HaveCount(1); // Should not create duplicate

            request.RemoveHeader(headerName).Should().BeTrue();
            request.Headers.Should().BeEmpty();
        }

        [Fact]
        public void Body_ShouldAllowNullAndStringValues()
        {
            // Arrange
            var request = new HttpRequest
            {
                Body = null
            };
            request.Body.Should().BeNull();

            // Act & Assert - string body
            request.Body = "test body content";
            request.Body.Should().Be("test body content");

            // Act & Assert - empty string body
            request.Body = "";
            request.Body.Should().Be("");
        }

        [Fact]
        public void FileBodyReference_ShouldAllowNullAndFileBodyReferenceValues()
        {
            // Arrange
            var request = new HttpRequest();
            var fileBodyRef = new FileBodyReference("test.json", true, System.Text.Encoding.UTF8, 1);

            // Act & Assert - null reference
            request.FileBodyReference = null;
            request.FileBodyReference.Should().BeNull();

            // Act & Assert - valid reference
            request.FileBodyReference = fileBodyRef;
            request.FileBodyReference.Should().Be(fileBodyRef);
            request.FileBodyReference.FilePath.Should().Be("test.json");
            request.FileBodyReference.ProcessVariables.Should().BeTrue();
            request.FileBodyReference.Encoding.Should().Be(System.Text.Encoding.UTF8);
        }

        [Fact]
        public void Metadata_ShouldBeInitializedAndAccessible()
        {
            // Arrange
            var request = new HttpRequest();

            // Act & Assert
            request.Metadata.Should().NotBeNull();
            request.Metadata.Should().BeOfType<HttpRequestMetadata>();
        }

        [Fact]
        public void CreateRequestWithAllProperties_SetsPropertiesCorrectly()
        {
            // Arrange
            var request = new HttpRequest();
            const string Name = "test-request";
            const string Method = "POST";
            const string Url = "https://api.example.com/users";
            const string Body = /*lang=json,strict*/ "{\"name\":\"test\"}";
            const int LineNumber = 42;

            // Act
            request.Name = Name;
            request.Method = Method;
            request.Url = Url;
            request.Body = Body;
            request.LineNumber = LineNumber;
            request.SetHeader("Content-Type", "application/json");
            request.SetHeader("Authorization", "Bearer token123");

            // Assert
            request.Name.Should().Be(Name);
            request.Method.Should().Be(Method);
            request.Url.Should().Be(Url);
            request.Body.Should().Be(Body);
            request.LineNumber.Should().Be(LineNumber);
            request.Headers.Should().HaveCount(2);
            request.GetHeader("Content-Type").Should().Be("application/json");
            request.GetHeader("Authorization").Should().Be("Bearer token123");
        }

        [Fact]
        public void CreateRequestWithFileBodyReference_SetsPropertiesCorrectly()
        {
            // Arrange
            var request = new HttpRequest();
            const string Name = "file-request";
            const string Method = "PUT";
            const string Url = "https://api.example.com/upload";
            var fileBodyRef = new FileBodyReference("test-file.txt", true, null, 1);

            // Act
            request.Name = Name;
            request.Method = Method;
            request.Url = Url;
            request.FileBodyReference = fileBodyRef;
            request.SetHeader("Content-Type", "application/octet-stream");

            // Assert
            request.Name.Should().Be(Name);
            request.Method.Should().Be(Method);
            request.Url.Should().Be(Url);
            request.Body.Should().BeNull(); // Body should be null when FileBodyReference is set
            request.FileBodyReference.Should().Be(fileBodyRef);
            request.GetHeader("Content-Type").Should().Be("application/octet-stream");
        }

        [Fact]
        public void Method_ShouldDefaultToGet()
        {
            // Arrange & Act
            var request = new HttpRequest();

            // Assert
            request.Method.Should().Be("GET");
        }

        [Fact]
        public void LineNumber_ShouldDefaultToZero()
        {
            // Arrange & Act
            var request = new HttpRequest();

            // Assert
            request.LineNumber.Should().Be(0);
        }

        [Fact]
        public void Name_ShouldDefaultToEmptyString()
        {
            // Arrange & Act
            var request = new HttpRequest();

            // Assert
            request.Name.Should().Be(string.Empty);
        }

        [Fact]
        public void Url_ShouldDefaultToEmptyString()
        {
            // Arrange & Act
            var request = new HttpRequest();

            // Assert
            request.Url.Should().Be(string.Empty);
        }

        [Fact]
        public void Headers_ShouldBeInitializedAndEmpty()
        {
            // Arrange & Act
            var request = new HttpRequest();

            // Assert
            request.Headers.Should().NotBeNull();
            request.Headers.Should().BeEmpty();
        }

        [Fact]
        public void Headers_ShouldBeOfCorrectType()
        {
            // Arrange & Act
            var request = new HttpRequest();

            // Assert
            request.Headers.Should().BeOfType<Dictionary<string, string>>();
        }
    }
}
