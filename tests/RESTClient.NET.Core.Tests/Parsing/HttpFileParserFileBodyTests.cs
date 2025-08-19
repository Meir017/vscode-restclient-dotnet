using System.Linq;
using AwesomeAssertions;
using RESTClient.NET.Core.Parsing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Parsing
{
    public class HttpFileParserFileBodyTests
    {
        [Fact]
        public async Task ParseAsync_WithAllFileBodyFormats_ShouldParseCorrectly()
        {
            // Arrange
            var content = @"@baseUrl = https://example.com

# @name raw-file-body
POST {{baseUrl}}/upload
Content-Type: application/xml

< C:\Users\Default\Desktop\demo.xml

# @name file-body-with-variables
POST {{baseUrl}}/template
Content-Type: application/xml

<@ ./demo.xml

# @name file-body-with-encoding
POST {{baseUrl}}/data
Content-Type: text/plain

<@latin1 ./demo.xml";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(3);

            // Test raw file body
            var rawRequest = result.GetRequestByName("raw-file-body");
            rawRequest.FileBodyReference.Should().NotBeNull();
            rawRequest.FileBodyReference!.FilePath.Should().Be(@"C:\Users\Default\Desktop\demo.xml");
            rawRequest.FileBodyReference.ProcessVariables.Should().BeFalse();
            rawRequest.FileBodyReference.Encoding.Should().BeNull();
            rawRequest.Body.Should().BeNull();

            // Test file body with variables
            var variablesRequest = result.GetRequestByName("file-body-with-variables");
            variablesRequest.FileBodyReference.Should().NotBeNull();
            variablesRequest.FileBodyReference!.FilePath.Should().Be("./demo.xml");
            variablesRequest.FileBodyReference.ProcessVariables.Should().BeTrue();
            variablesRequest.Body.Should().BeNull();

            // Test file body with encoding
            var encodingRequest = result.GetRequestByName("file-body-with-encoding");
            encodingRequest.FileBodyReference.Should().NotBeNull();
            encodingRequest.FileBodyReference!.FilePath.Should().Be("./demo.xml");
            encodingRequest.FileBodyReference.ProcessVariables.Should().BeTrue();
            encodingRequest.FileBodyReference.Encoding!.WebName.Should().Be("iso-8859-1");
            encodingRequest.Body.Should().BeNull();
        }

        [Fact]
        public async Task ParseAsync_WithFileBodyAndExpectations_ShouldParseMetadata()
        {
            // Arrange
            var content = @"# @name file-upload
# @expect status 201
# @expect header Location
POST https://api.example.com/upload
Content-Type: application/xml
Authorization: Bearer {{token}}

<@ ./upload-data.xml";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.GetRequestByName("file-upload");
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./upload-data.xml");
            request.FileBodyReference.ProcessVariables.Should().BeTrue();
            
            request.Metadata.Expectations.Should().HaveCount(2);
            request.Headers.Should().HaveCount(2);
            request.Headers["Content-Type"].Should().Be("application/xml");
            request.Headers["Authorization"].Should().Be("Bearer {{token}}");
        }

        [Fact] 
        public async Task ParseAsync_WithWhitespaceAroundFilePath_ShouldTrimCorrectly()
        {
            // Arrange
            var content = @"# @name whitespace-test
POST https://example.com/api
Content-Type: application/json

<@   utf8    ./data with spaces.json   ";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.GetRequestByName("whitespace-test");
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./data with spaces.json");
        }

        [Fact]
        public async Task ParseAsync_WithTraditionalSeparators_ShouldWork()
        {
            // Arrange
            var content = @"### File Body Test 1

POST https://example.com/api
Content-Type: application/xml

< ./file1.xml

### File Body Test 2

POST https://example.com/api  
Content-Type: application/json

<@ ./template.json";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(2);

            var request1 = result.Requests.First();
            request1.FileBodyReference.Should().NotBeNull();
            request1.FileBodyReference!.FilePath.Should().Be("./file1.xml");
            request1.FileBodyReference.ProcessVariables.Should().BeFalse();

            var request2 = result.Requests.Last();
            request2.FileBodyReference.Should().NotBeNull();
            request2.FileBodyReference!.FilePath.Should().Be("./template.json");
            request2.FileBodyReference.ProcessVariables.Should().BeTrue();
        }
    }
}
