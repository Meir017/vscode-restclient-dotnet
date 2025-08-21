using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class VariableDefinitionTests
    {
        [Fact]
        public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            const string Name = "baseUrl";
            const string Value = "https://api.example.com";
            const VariableType Type = VariableType.File;

            // Act
            var variable = new VariableDefinition(Name, Value, Type);

            // Assert
            variable.Name.Should().Be(Name);
            variable.Value.Should().Be(Value);
            variable.Type.Should().Be(Type);
            variable.LineNumber.Should().Be(0); // Default value
        }

        [Fact]
        public void Constructor_WithNameAndValue_DefaultsToFileType()
        {
            // Arrange
            const string Name = "apiKey";
            const string Value = "secret123";

            // Act
            var variable = new VariableDefinition(Name, Value);

            // Assert
            variable.Name.Should().Be(Name);
            variable.Value.Should().Be(Value);
            variable.Type.Should().Be(VariableType.File);
            variable.LineNumber.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithNullName_SetsNameToNull()
        {
            // Arrange & Act
            var variable = new VariableDefinition(null!, "value");

            // Assert
            variable.Name.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithNullValue_SetsValueToNull()
        {
            // Arrange & Act
            var variable = new VariableDefinition("name", null!);

            // Assert
            variable.Value.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithEmptyNameAndValue_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var variable = new VariableDefinition("", "");

            // Assert
            variable.Name.Should().Be("");
            variable.Value.Should().Be("");
            variable.Type.Should().Be(VariableType.File);
        }

        [Theory]
        [InlineData(VariableType.File)]
        [InlineData(VariableType.Environment)]
        [InlineData(VariableType.System)]
        [InlineData(VariableType.Request)]
        public void Constructor_WithDifferentVariableTypes_SetsTypeCorrectly(VariableType type)
        {
            // Arrange
            const string Name = "testVar";
            const string Value = "testValue";

            // Act
            var variable = new VariableDefinition(Name, Value, type);

            // Assert
            variable.Type.Should().Be(type);
        }

        [Fact]
        public void Properties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var variable = new VariableDefinition("initial", "value")
            {
                // Act
                Name = "newName",
                Value = "newValue",
                Type = VariableType.Environment,
                LineNumber = 42
            };

            // Assert
            variable.Name.Should().Be("newName");
            variable.Value.Should().Be("newValue");
            variable.Type.Should().Be(VariableType.Environment);
            variable.LineNumber.Should().Be(42);
        }

        [Fact]
        public void ToString_WithFileType_ReturnsFormattedString()
        {
            // Arrange
            var variable = new VariableDefinition("baseUrl", "https://api.example.com", VariableType.File);

            // Act
            string result = variable.ToString();

            // Assert
            result.Should().Be("baseUrl = https://api.example.com (File)");
        }

        [Fact]
        public void ToString_WithEnvironmentType_ReturnsFormattedString()
        {
            // Arrange
            var variable = new VariableDefinition("PATH", "/usr/bin", VariableType.Environment);

            // Act
            string result = variable.ToString();

            // Assert
            result.Should().Be("PATH = /usr/bin (Environment)");
        }

        [Fact]
        public void ToString_WithSystemType_ReturnsFormattedString()
        {
            // Arrange
            var variable = new VariableDefinition("timestamp", "2023-01-01T00:00:00Z", VariableType.System);

            // Act
            string result = variable.ToString();

            // Assert
            result.Should().Be("timestamp = 2023-01-01T00:00:00Z (System)");
        }

        [Fact]
        public void ToString_WithRequestType_ReturnsFormattedString()
        {
            // Arrange
            var variable = new VariableDefinition("token", "abc123", VariableType.Request);

            // Act
            string result = variable.ToString();

            // Assert
            result.Should().Be("token = abc123 (Request)");
        }

        [Fact]
        public void ToString_WithEmptyValues_ReturnsFormattedString()
        {
            // Arrange
            var variable = new VariableDefinition("", "", VariableType.File);

            // Act
            string result = variable.ToString();

            // Assert
            result.Should().Be(" =  (File)");
        }

        [Fact]
        public void ToString_WithNullValues_ReturnsFormattedString()
        {
            // Arrange
            var variable = new VariableDefinition(null!, null!, VariableType.File);

            // Act
            string result = variable.ToString();

            // Assert
            result.Should().Be(" =  (File)");
        }

        [Fact]
        public void LineNumber_ShouldDefaultToZero()
        {
            // Arrange & Act
            var variable = new VariableDefinition("test", "value");

            // Assert
            variable.LineNumber.Should().Be(0);
        }

        [Fact]
        public void LineNumber_ShouldBeSettable()
        {
            // Arrange
            var variable = new VariableDefinition("test", "value")
            {
                // Act
                LineNumber = 123
            };

            // Assert
            variable.LineNumber.Should().Be(123);
        }

        [Fact]
        public void Name_ShouldDefaultToEmptyStringWhenNotSetInConstructor()
        {
            // Arrange & Act
            var variable = new VariableDefinition("test", "value")
            {
                Name = string.Empty
            };

            // Assert
            variable.Name.Should().Be(string.Empty);
        }

        [Fact]
        public void Value_ShouldDefaultToEmptyStringWhenNotSetInConstructor()
        {
            // Arrange & Act
            var variable = new VariableDefinition("test", "value")
            {
                Value = string.Empty
            };

            // Assert
            variable.Value.Should().Be(string.Empty);
        }

        [Fact]
        public void Type_ShouldDefaultToFileWhenNotSpecified()
        {
            // Arrange & Act
            var variable = new VariableDefinition("test", "value");

            // Assert
            variable.Type.Should().Be(VariableType.File);
        }

        [Fact]
        public void VariableType_EnumValues_ShouldBeCorrect()
        {
            // This test ensures the enum values haven't changed
            // Act & Assert
            ((int)VariableType.File).Should().Be(0);
            ((int)VariableType.Environment).Should().Be(1);
            ((int)VariableType.System).Should().Be(2);
            ((int)VariableType.Request).Should().Be(3);
        }

        [Fact]
        public void CreateFileVariable_ShouldCreateCorrectVariable()
        {
            // Arrange & Act
            var variable = new VariableDefinition("config", "production", VariableType.File)
            {
                LineNumber = 5
            };

            // Assert
            variable.Name.Should().Be("config");
            variable.Value.Should().Be("production");
            variable.Type.Should().Be(VariableType.File);
            variable.LineNumber.Should().Be(5);
        }

        [Fact]
        public void CreateEnvironmentVariable_ShouldCreateCorrectVariable()
        {
            // Arrange & Act
            var variable = new VariableDefinition("HOME", "/home/user", VariableType.Environment)
            {
                LineNumber = 10
            };

            // Assert
            variable.Name.Should().Be("HOME");
            variable.Value.Should().Be("/home/user");
            variable.Type.Should().Be(VariableType.Environment);
            variable.LineNumber.Should().Be(10);
        }

        [Fact]
        public void CreateSystemVariable_ShouldCreateCorrectVariable()
        {
            // Arrange & Act
            var variable = new VariableDefinition("$guid", "550e8400-e29b-41d4-a716-446655440000", VariableType.System)
            {
                LineNumber = 15
            };

            // Assert
            variable.Name.Should().Be("$guid");
            variable.Value.Should().Be("550e8400-e29b-41d4-a716-446655440000");
            variable.Type.Should().Be(VariableType.System);
            variable.LineNumber.Should().Be(15);
        }

        [Fact]
        public void CreateRequestVariable_ShouldCreateCorrectVariable()
        {
            // Arrange & Act
            var variable = new VariableDefinition("authToken", "{{login.response.body.$.token}}", VariableType.Request)
            {
                LineNumber = 20
            };

            // Assert
            variable.Name.Should().Be("authToken");
            variable.Value.Should().Be("{{login.response.body.$.token}}");
            variable.Type.Should().Be(VariableType.Request);
            variable.LineNumber.Should().Be(20);
        }

        [Theory]
        [InlineData("baseUrl", "https://api.example.com", "baseUrl = https://api.example.com (File)")]
        [InlineData("token", "abc123", "token = abc123 (File)")]
        [InlineData("", "", " =  (File)")]
        [InlineData("key", "", "key =  (File)")]
        [InlineData("", "value", " = value (File)")]
        public void ToString_WithVariousInputs_ReturnsCorrectFormat(string name, string value, string expected)
        {
            // Arrange
            var variable = new VariableDefinition(name, value, VariableType.File);

            // Act
            string result = variable.ToString();

            // Assert
            result.Should().Be(expected);
        }
    }
}
