using System.Linq;
using System.Text;
using AwesomeAssertions;
using RESTClient.NET.Core.Parsing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Integration
{
    public class FileBodyIntegrationTests
    {
        [Fact]
        public async Task ParseAsync_CompleteFileBodyDemo_ShouldParseAllRequestsCorrectly()
        {
            // Arrange
            var content = @"# RESTClient.NET File Body Demo
@baseUrl = https://api.example.com
@token = your-auth-token-here

# @name upload-raw-xml
# @expect status 201
POST {{baseUrl}}/upload/xml
Content-Type: application/xml
Authorization: Bearer {{token}}

< ./sample-data.xml

# @name upload-template-json
# @expect status 200
POST {{baseUrl}}/process/template
Content-Type: application/json

<@ ./request-template.json

# @name upload-latin1-text
# @expect status 202
POST {{baseUrl}}/upload/text
Content-Type: text/plain; charset=iso-8859-1

<@latin1 ./legacy-data.txt

# @name upload-utf8-explicit
# @expect status 200
POST {{baseUrl}}/upload/utf8
Content-Type: application/json; charset=utf-8

<@utf8 ./unicode-data.json";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(4);

            // Verify file variables
            result.FileVariables.Should().HaveCount(2);
            result.FileVariables["baseUrl"].Should().Be("https://api.example.com");
            result.FileVariables["token"].Should().Be("your-auth-token-here");

            // Test raw XML file body
            var rawXmlRequest = result.GetRequestByName("upload-raw-xml");
            rawXmlRequest.Should().NotBeNull();
            rawXmlRequest.FileBodyReference.Should().NotBeNull();
            rawXmlRequest.FileBodyReference!.FilePath.Should().Be("./sample-data.xml");
            rawXmlRequest.FileBodyReference.ProcessVariables.Should().BeFalse();
            rawXmlRequest.FileBodyReference.Encoding.Should().BeNull();
            rawXmlRequest.Body.Should().BeNull();
            rawXmlRequest.Method.Should().Be("POST");
            rawXmlRequest.Url.Should().Be("{{baseUrl}}/upload/xml");
            rawXmlRequest.Headers["Content-Type"].Should().Be("application/xml");
            rawXmlRequest.Headers["Authorization"].Should().Be("Bearer {{token}}");
            rawXmlRequest.Metadata.Expectations.Should().HaveCount(1);

            // Test JSON template with variables
            var templateJsonRequest = result.GetRequestByName("upload-template-json");
            templateJsonRequest.Should().NotBeNull();
            templateJsonRequest.FileBodyReference.Should().NotBeNull();
            templateJsonRequest.FileBodyReference!.FilePath.Should().Be("./request-template.json");
            templateJsonRequest.FileBodyReference.ProcessVariables.Should().BeTrue();
            templateJsonRequest.FileBodyReference.Encoding.Should().Be(Encoding.UTF8);
            templateJsonRequest.Body.Should().BeNull();

            // Test Latin1 encoding
            var latin1Request = result.GetRequestByName("upload-latin1-text");
            latin1Request.Should().NotBeNull();
            latin1Request.FileBodyReference.Should().NotBeNull();
            latin1Request.FileBodyReference!.FilePath.Should().Be("./legacy-data.txt");
            latin1Request.FileBodyReference.ProcessVariables.Should().BeTrue();
            latin1Request.FileBodyReference.Encoding.Should().Be(Encoding.GetEncoding("ISO-8859-1"));
            latin1Request.Headers["Content-Type"].Should().Be("text/plain; charset=iso-8859-1");

            // Test UTF8 explicit encoding
            var utf8Request = result.GetRequestByName("upload-utf8-explicit");
            utf8Request.Should().NotBeNull();
            utf8Request.FileBodyReference.Should().NotBeNull();
            utf8Request.FileBodyReference!.FilePath.Should().Be("./unicode-data.json");
            utf8Request.FileBodyReference.ProcessVariables.Should().BeTrue();
            utf8Request.FileBodyReference.Encoding.Should().Be(Encoding.UTF8);
        }

        [Fact]
        public async Task ParseAsync_FileBodyWithTraditionalSeparators_ShouldWork()
        {
            // Arrange
            var content = @"### Raw File Upload

POST https://api.example.com/upload
Content-Type: application/octet-stream

< /absolute/path/to/binary.dat

### Template Processing

POST https://api.example.com/process
Content-Type: application/json

<@ ./template.json

### Legacy Text with Encoding

POST https://api.example.com/legacy
Content-Type: text/plain

<@iso-8859-1 ./legacy.txt";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(3);

            // All requests should have file body references
            foreach (var request in result.Requests)
            {
                request.FileBodyReference.Should().NotBeNull();
                request.Body.Should().BeNull();
            }

            // Check specific requests
            var rawUpload = result.Requests[0];
            rawUpload.FileBodyReference!.FilePath.Should().Be("/absolute/path/to/binary.dat");
            rawUpload.FileBodyReference.ProcessVariables.Should().BeFalse();

            var templateRequest = result.Requests[1];
            templateRequest.FileBodyReference!.FilePath.Should().Be("./template.json");
            templateRequest.FileBodyReference.ProcessVariables.Should().BeTrue();

            var legacyRequest = result.Requests[2];
            legacyRequest.FileBodyReference!.FilePath.Should().Be("./legacy.txt");
            legacyRequest.FileBodyReference.ProcessVariables.Should().BeTrue();
            legacyRequest.FileBodyReference.Encoding.Should().Be(Encoding.GetEncoding("ISO-8859-1"));
        }

        [Theory]
        [InlineData("< ./file.txt", "./file.txt", false, null)]
        [InlineData("<@ ./file.txt", "./file.txt", true, "utf-8")]
        [InlineData("<@utf8 ./file.txt", "./file.txt", true, "utf-8")]
        [InlineData("<@latin1 ./file.txt", "./file.txt", true, "iso-8859-1")]
        [InlineData("<@ascii ./file.txt", "./file.txt", true, "us-ascii")]
        [InlineData("<@utf16 ./file.txt", "./file.txt", true, "utf-16")]
        [InlineData("< C:\\Windows\\file.txt", "C:\\Windows\\file.txt", false, null)]
        [InlineData("<@ /unix/path/file.txt", "/unix/path/file.txt", true, "utf-8")]
        public async Task ParseAsync_VariousFileBodyFormats_ShouldParseCorrectly(
            string fileBodyLine, string expectedPath, bool expectedProcessVariables, string? expectedEncodingName)
        {
            // Arrange
            var content = $@"# @name test-request
POST https://example.com/api
Content-Type: application/json

{fileBodyLine}";

            var parser = new HttpFileParser();

            // Act
            var result = await parser.ParseAsync(content);

            // Assert
            result.Should().NotBeNull();
            result.Requests.Should().HaveCount(1);

            var request = result.GetRequestByName("test-request");
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be(expectedPath);
            request.FileBodyReference.ProcessVariables.Should().Be(expectedProcessVariables);

            if (expectedEncodingName != null)
            {
                request.FileBodyReference.Encoding.Should().NotBeNull();
                request.FileBodyReference.Encoding!.WebName.Should().Be(expectedEncodingName);
            }
            else
            {
                request.FileBodyReference.Encoding.Should().BeNull();
            }
        }
    }
}
