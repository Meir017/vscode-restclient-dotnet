using System.Linq;
using AwesomeAssertions;
using RESTClient.NET.Core.Parsing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Parsing
{
    public class HttpTokenizerFileBodyTests
    {
        [Fact]
        public void Tokenize_WithRawFileBody_ShouldCreateFileBodyToken()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

< C:\Users\Default\Desktop\demo.xml";

            var tokenizer = new HttpTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(content).ToList();

            // Assert
            var fileBodyToken = tokens.FirstOrDefault(t => t.Type == HttpTokenType.FileBody);
            fileBodyToken.Should().NotBeNull();
            fileBodyToken!.Value.Should().Be(@"C:\Users\Default\Desktop\demo.xml");
        }

        [Fact]
        public void Tokenize_WithRelativeFileBody_ShouldCreateFileBodyToken()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

< ./demo.xml";

            var tokenizer = new HttpTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(content).ToList();

            // Assert
            var fileBodyToken = tokens.FirstOrDefault(t => t.Type == HttpTokenType.FileBody);
            fileBodyToken.Should().NotBeNull();
            fileBodyToken!.Value.Should().Be("./demo.xml");
        }

        [Fact]
        public void Tokenize_WithFileBodyWithVariables_ShouldCreateFileBodyWithVariablesToken()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@ ./demo.xml";

            var tokenizer = new HttpTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(content).ToList();

            // Assert
            var fileBodyToken = tokens.FirstOrDefault(t => t.Type == HttpTokenType.FileBodyWithVariables);
            fileBodyToken.Should().NotBeNull();
            fileBodyToken!.Value.Should().Be("./demo.xml");
        }

        [Fact]
        public void Tokenize_WithFileBodyWithEncoding_ShouldCreateFileBodyWithEncodingToken()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@latin1 ./demo.xml";

            var tokenizer = new HttpTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(content).ToList();

            // Assert
            var fileBodyToken = tokens.FirstOrDefault(t => t.Type == HttpTokenType.FileBodyWithEncoding);
            fileBodyToken.Should().NotBeNull();
            fileBodyToken!.Value.Should().Be("latin1|./demo.xml");
        }

        [Fact]
        public void Tokenize_WithFileBodySpacing_ShouldHandleWhitespace()
        {
            // Arrange
            var content = @"# @name test-request
POST https://example.com/api/data
Content-Type: application/xml

<@  utf8   ./demo with spaces.xml  ";

            var tokenizer = new HttpTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(content).ToList();

            // Assert
            var fileBodyToken = tokens.FirstOrDefault(t => t.Type == HttpTokenType.FileBodyWithEncoding);
            fileBodyToken.Should().NotBeNull();
            fileBodyToken!.Value.Should().Be("utf8|./demo with spaces.xml");
        }

        [Fact]
        public void Tokenize_WithMixedBodyContent_ShouldTokenizeCorrectly()
        {
            // Arrange
            var content = @"# @name mixed-content
POST https://example.com/api/data
Content-Type: application/json

{
  ""file_reference"": ""< not a file body reference in JSON""
}";

            var tokenizer = new HttpTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(content).ToList();

            // Assert
            var bodyTokens = tokens.Where(t => t.Type == HttpTokenType.Body).ToList();
            bodyTokens.Should().HaveCount(3); // Opening brace, content line, closing brace
            
            var fileBodyTokens = tokens.Where(t => t.Type == HttpTokenType.FileBody || 
                                                 t.Type == HttpTokenType.FileBodyWithVariables || 
                                                 t.Type == HttpTokenType.FileBodyWithEncoding).ToList();
            fileBodyTokens.Should().BeEmpty(); // Should not create file body tokens for content inside JSON
        }

        [Theory]
        [InlineData("< ./file.txt")]
        [InlineData("<@ ./file.txt")]
        [InlineData("<@utf8 ./file.txt")]
        [InlineData("<@latin1 ./file.txt")]
        [InlineData("< C:\\absolute\\path.xml")]
        [InlineData("<@ /unix/absolute/path.json")]
        public void Tokenize_WithVariousFileBodyFormats_ShouldRecognizeAll(string fileBodyLine)
        {
            // Arrange
            var content = $@"# @name test
POST https://example.com/api
Content-Type: application/xml

{fileBodyLine}";

            var tokenizer = new HttpTokenizer();

            // Act
            var tokens = tokenizer.Tokenize(content).ToList();

            // Assert
            var fileBodyTokens = tokens.Where(t => t.Type == HttpTokenType.FileBody || 
                                                 t.Type == HttpTokenType.FileBodyWithVariables || 
                                                 t.Type == HttpTokenType.FileBodyWithEncoding).ToList();
            fileBodyTokens.Should().HaveCount(1);
        }
    }
}
