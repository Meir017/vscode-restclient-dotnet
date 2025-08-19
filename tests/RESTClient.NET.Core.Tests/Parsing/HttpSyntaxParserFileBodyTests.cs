using System.Linq;
using System.Text;
using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Parsing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Parsing
{
    public class HttpSyntaxParserFileBodyTests
    {
        [Fact]
        public void Parse_WithRawFileBody_ShouldCreateFileBodyReference()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

< ./demo.xml";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./demo.xml");
            request.FileBodyReference.ProcessVariables.Should().BeFalse();
            request.FileBodyReference.Encoding.Should().BeNull();
            request.Body.Should().BeNull(); // Should not have regular body when file body is used
        }

        [Fact]
        public void Parse_WithFileBodyWithVariables_ShouldCreateFileBodyReference()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@ ./template.xml";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./template.xml");
            request.FileBodyReference.ProcessVariables.Should().BeTrue();
            request.FileBodyReference.Encoding.Should().Be(Encoding.UTF8);
            request.Body.Should().BeNull();
        }

        [Fact]
        public void Parse_WithFileBodyWithLatin1Encoding_ShouldCreateFileBodyReference()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@latin1 ./data.txt";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./data.txt");
            request.FileBodyReference.ProcessVariables.Should().BeTrue();
            request.FileBodyReference.Encoding.Should().Be(Encoding.Latin1);
            request.Body.Should().BeNull();
        }

        [Fact]
        public void Parse_WithFileBodyWithUTF8Encoding_ShouldCreateFileBodyReference()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@utf8 ./data.json";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./data.json");
            request.FileBodyReference.ProcessVariables.Should().BeTrue();
            request.FileBodyReference.Encoding.Should().Be(Encoding.UTF8);
        }

        [Fact]
        public void Parse_WithFileBodyWithUnknownEncoding_ShouldFallbackToUTF8()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@unknown-encoding ./data.txt";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./data.txt");
            request.FileBodyReference.ProcessVariables.Should().BeTrue();
            request.FileBodyReference.Encoding.Should().Be(Encoding.UTF8); // Should fallback to UTF8
        }

        [Fact]
        public void Parse_WithAbsoluteWindowsPath_ShouldHandleCorrectly()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

< C:\Users\Default\Desktop\demo.xml";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be(@"C:\Users\Default\Desktop\demo.xml");
            request.FileBodyReference.ProcessVariables.Should().BeFalse();
        }

        [Theory]
        [InlineData("ascii")]
        [InlineData("utf16")]
        [InlineData("utf-16")]
        [InlineData("windows1252")]
        public void Parse_WithSupportedEncodings_ShouldCreateCorrectEncoding(string encodingName)
        {
            // Arrange
            var content = $@"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@{encodingName} ./data.txt";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.Encoding.Should().NotBeNull();
        }

        [Fact]
        public void Parse_WithMixedBodyAndFileBody_ShouldOnlyUseFileBody()
        {
            // Arrange - This scenario shouldn't normally happen, but let's test precedence
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

{""some"": ""json""}
<@ ./data.xml";

            var tokenizer = new HttpTokenizer();
            var syntaxParser = new HttpSyntaxParser();
            var tokens = tokenizer.Tokenize(content);

            // Act
            var httpFile = syntaxParser.Parse(tokens);

            // Assert
            httpFile.Requests.Should().HaveCount(1);
            var request = httpFile.Requests.First();
            request.FileBodyReference.Should().NotBeNull();
            request.FileBodyReference!.FilePath.Should().Be("./data.xml");
            request.Body.Should().NotBeNull(); // Regular body content should still be there
        }
    }
}
