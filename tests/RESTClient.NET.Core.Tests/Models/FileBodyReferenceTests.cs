using System.Text;
using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class FileBodyReferenceTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            string filePath = "./data.json";
            bool processVariables = true;
            Encoding encoding = Encoding.UTF8;
            int lineNumber = 42;

            // Act
            var fileBodyRef = new FileBodyReference(filePath, processVariables, encoding, lineNumber);

            // Assert
            fileBodyRef.FilePath.Should().Be(filePath);
            fileBodyRef.ProcessVariables.Should().Be(processVariables);
            fileBodyRef.Encoding.Should().Be(encoding);
            fileBodyRef.LineNumber.Should().Be(lineNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidFilePath_ShouldThrowArgumentException(string invalidPath)
        {
            // Act & Assert
            Func<FileBodyReference> action = () => new FileBodyReference(invalidPath, false, null, 0);
            action.Should().Throw<ArgumentException>()
                .WithMessage("File path cannot be null or whitespace*")
                .And.ParamName.Should().Be("filePath");
        }

        [Fact]
        public void Constructor_WithNullFilePath_ShouldThrowArgumentException()
        {
            // Act & Assert
            Func<FileBodyReference> action = () => new FileBodyReference(null!, false, null, 0);
            action.Should().Throw<ArgumentException>()
                .WithMessage("File path cannot be null or whitespace*")
                .And.ParamName.Should().Be("filePath");
        }

        [Fact]
        public void Raw_ShouldCreateRawFileBodyReference()
        {
            // Arrange
            string filePath = "./data.xml";
            int lineNumber = 15;

            // Act
            var fileBodyRef = FileBodyReference.Raw(filePath, lineNumber);

            // Assert
            fileBodyRef.FilePath.Should().Be(filePath);
            fileBodyRef.ProcessVariables.Should().BeFalse();
            fileBodyRef.Encoding.Should().BeNull();
            fileBodyRef.LineNumber.Should().Be(lineNumber);
        }

        [Fact]
        public void WithVariables_ShouldCreateFileBodyReferenceWithVariableProcessing()
        {
            // Arrange
            string filePath = "./template.json";
            int lineNumber = 20;

            // Act
            var fileBodyRef = FileBodyReference.WithVariables(filePath, lineNumber);

            // Assert
            fileBodyRef.FilePath.Should().Be(filePath);
            fileBodyRef.ProcessVariables.Should().BeTrue();
            fileBodyRef.Encoding.Should().Be(Encoding.UTF8);
            fileBodyRef.LineNumber.Should().Be(lineNumber);
        }

        [Fact]
        public void WithVariablesAndEncoding_ShouldCreateFileBodyReferenceWithCustomEncoding()
        {
            // Arrange
            string filePath = "./data.txt";
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            int lineNumber = 25;

            // Act
            var fileBodyRef = FileBodyReference.WithVariablesAndEncoding(filePath, encoding, lineNumber);

            // Assert
            fileBodyRef.FilePath.Should().Be(filePath);
            fileBodyRef.ProcessVariables.Should().BeTrue();
            fileBodyRef.Encoding.Should().Be(encoding);
            fileBodyRef.LineNumber.Should().Be(lineNumber);
        }

        [Fact]
        public void ToString_WithRawFileBody_ShouldReturnCorrectFormat()
        {
            // Arrange
            var fileBodyRef = FileBodyReference.Raw("./data.json");

            // Act
            string result = fileBodyRef.ToString();

            // Assert
            result.Should().Be("< ./data.json");
        }

        [Fact]
        public void ToString_WithVariableProcessing_ShouldReturnCorrectFormat()
        {
            // Arrange
            var fileBodyRef = FileBodyReference.WithVariables("./template.xml");

            // Act
            string result = fileBodyRef.ToString();

            // Assert
            result.Should().Be("<@ ./template.xml");
        }

        [Fact]
        public void ToString_WithCustomEncoding_ShouldReturnCorrectFormat()
        {
            // Arrange
            var fileBodyRef = FileBodyReference.WithVariablesAndEncoding("./data.txt", Encoding.GetEncoding("ISO-8859-1"));

            // Act
            string result = fileBodyRef.ToString();

            // Assert
            result.Should().Be("<@iso-8859-1 ./data.txt");
        }

        [Fact]
        public void Equals_WithSameProperties_ShouldReturnTrue()
        {
            // Arrange
            var fileBodyRef1 = new FileBodyReference("./data.json", true, Encoding.UTF8, 10);
            var fileBodyRef2 = new FileBodyReference("./data.json", true, Encoding.UTF8, 20); // Different line number

            // Act & Assert
            fileBodyRef1.Equals(fileBodyRef2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentProperties_ShouldReturnFalse()
        {
            // Arrange
            var fileBodyRef1 = new FileBodyReference("./data.json", true, Encoding.UTF8, 10);
            var fileBodyRef2 = new FileBodyReference("./other.json", true, Encoding.UTF8, 10);

            // Act & Assert
            fileBodyRef1.Equals(fileBodyRef2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameProperties_ShouldReturnSameHashCode()
        {
            // Arrange
            var fileBodyRef1 = new FileBodyReference("./data.json", true, Encoding.UTF8, 10);
            var fileBodyRef2 = new FileBodyReference("./data.json", true, Encoding.UTF8, 20); // Different line number

            // Act
            int hash1 = fileBodyRef1.GetHashCode();
            int hash2 = fileBodyRef2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Theory]
        [InlineData("   ./spaced.json  ", "./spaced.json")]
        [InlineData("\t./tabbed.xml\t", "./tabbed.xml")]
        public void Constructor_ShouldTrimFilePath(string inputPath, string expectedPath)
        {
            // Act
            var fileBodyRef = FileBodyReference.Raw(inputPath);

            // Assert
            fileBodyRef.FilePath.Should().Be(expectedPath);
        }
    }
}
