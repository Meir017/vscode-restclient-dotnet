using AwesomeAssertions;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Core.Tests.Models
{
    public class TestExpectationTests
    {
        [Fact]
        public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            const ExpectationType Type = ExpectationType.StatusCode;
            const string Value = "200";
            const string Context = "OK response";

            // Act
            var expectation = new TestExpectation(Type, Value, Context);

            // Assert
            expectation.Type.Should().Be(Type);
            expectation.Value.Should().Be(Value);
            expectation.Context.Should().Be(Context);
        }

        [Fact]
        public void Constructor_WithoutContext_SetsContextToNull()
        {
            // Arrange
            const ExpectationType Type = ExpectationType.Header;
            const string Value = "application/json";

            // Act
            var expectation = new TestExpectation(Type, Value);

            // Assert
            expectation.Type.Should().Be(Type);
            expectation.Value.Should().Be(Value);
            expectation.Context.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithNullValue_SetsValueToNull()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.BodyContains, null!);

            // Assert
            expectation.Value.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithEmptyValue_SetsValueToEmpty()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.BodyContains, "");

            // Assert
            expectation.Value.Should().Be("");
        }

        [Fact]
        public void Constructor_WithNullContext_SetsContextToNull()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.Header, "value", null);

            // Assert
            expectation.Context.Should().BeNull();
        }

        [Theory]
        [InlineData(ExpectationType.StatusCode)]
        [InlineData(ExpectationType.Header)]
        [InlineData(ExpectationType.BodyContains)]
        [InlineData(ExpectationType.BodyPath)]
        [InlineData(ExpectationType.Schema)]
        [InlineData(ExpectationType.MaxTime)]
        public void Constructor_WithDifferentExpectationTypes_SetsTypeCorrectly(ExpectationType type)
        {
            // Arrange & Act
            var expectation = new TestExpectation(type, "test-value");

            // Assert
            expectation.Type.Should().Be(type);
        }

        [Fact]
        public void Properties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var expectation = new TestExpectation(ExpectationType.StatusCode, "200")
            {
                // Act
                Type = ExpectationType.Header,
                Value = "application/json",
                Context = "Content-Type"
            };

            // Assert
            expectation.Type.Should().Be(ExpectationType.Header);
            expectation.Value.Should().Be("application/json");
            expectation.Context.Should().Be("Content-Type");
        }

        [Fact]
        public void StatusCodeExpectation_ShouldBeCreatedCorrectly()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.StatusCode, "201", "Created");

            // Assert
            expectation.Type.Should().Be(ExpectationType.StatusCode);
            expectation.Value.Should().Be("201");
            expectation.Context.Should().Be("Created");
        }

        [Fact]
        public void HeaderExpectation_ShouldBeCreatedCorrectly()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.Header, "Bearer token123", "Authorization");

            // Assert
            expectation.Type.Should().Be(ExpectationType.Header);
            expectation.Value.Should().Be("Bearer token123");
            expectation.Context.Should().Be("Authorization");
        }

        [Fact]
        public void BodyContainsExpectation_ShouldBeCreatedCorrectly()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.BodyContains, "success", "Response should contain success message");

            // Assert
            expectation.Type.Should().Be(ExpectationType.BodyContains);
            expectation.Value.Should().Be("success");
            expectation.Context.Should().Be("Response should contain success message");
        }

        [Fact]
        public void BodyPathExpectation_ShouldBeCreatedCorrectly()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.BodyPath, "John Doe", "$.user.name");

            // Assert
            expectation.Type.Should().Be(ExpectationType.BodyPath);
            expectation.Value.Should().Be("John Doe");
            expectation.Context.Should().Be("$.user.name");
        }

        [Fact]
        public void SchemaExpectation_ShouldBeCreatedCorrectly()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.Schema, "user-schema.json", "User response schema");

            // Assert
            expectation.Type.Should().Be(ExpectationType.Schema);
            expectation.Value.Should().Be("user-schema.json");
            expectation.Context.Should().Be("User response schema");
        }

        [Fact]
        public void MaxTimeExpectation_ShouldBeCreatedCorrectly()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.MaxTime, "1000", "Response time should be under 1 second");

            // Assert
            expectation.Type.Should().Be(ExpectationType.MaxTime);
            expectation.Value.Should().Be("1000");
            expectation.Context.Should().Be("Response time should be under 1 second");
        }

        [Fact]
        public void Value_ShouldDefaultToEmptyStringIfNotSet()
        {
            // Arrange & Act
            var expectation = new TestExpectation(ExpectationType.StatusCode, "200")
            {
                Value = string.Empty
            };

            // Assert
            expectation.Value.Should().Be(string.Empty);
        }

        [Fact]
        public void Context_ShouldAcceptNullAndStringValues()
        {
            // Arrange
            var expectation = new TestExpectation(ExpectationType.Header, "application/json")
            {
                // Act & Assert - null value
                Context = null
            };
            expectation.Context.Should().BeNull();

            // Act & Assert - string value
            expectation.Context = "Content-Type header";
            expectation.Context.Should().Be("Content-Type header");

            // Act & Assert - empty string
            expectation.Context = "";
            expectation.Context.Should().Be("");
        }

        [Fact]
        public void ExpectationType_EnumValues_ShouldBeCorrect()
        {
            // This test ensures the enum values haven't changed
            // Act & Assert
            ((int)ExpectationType.StatusCode).Should().Be(0);
            ((int)ExpectationType.Header).Should().Be(1);
            ((int)ExpectationType.BodyContains).Should().Be(2);
            ((int)ExpectationType.BodyPath).Should().Be(3);
            ((int)ExpectationType.Schema).Should().Be(4);
            ((int)ExpectationType.MaxTime).Should().Be(5);
        }

        [Theory]
        [InlineData(ExpectationType.StatusCode, "200", null)]
        [InlineData(ExpectationType.Header, "application/json", "Content-Type")]
        [InlineData(ExpectationType.BodyContains, "success", "")]
        [InlineData(ExpectationType.BodyPath, "value", "$.path")]
        [InlineData(ExpectationType.Schema, "schema.json", "validation")]
        [InlineData(ExpectationType.MaxTime, "1000", "performance")]
        public void Constructor_WithVariousInputs_SetsPropertiesCorrectly(ExpectationType type, string value, string? context)
        {
            // Arrange & Act
            var expectation = new TestExpectation(type, value, context);

            // Assert
            expectation.Type.Should().Be(type);
            expectation.Value.Should().Be(value);
            expectation.Context.Should().Be(context);
        }

        [Fact]
        public void Constructor_WithSpecialCharactersInValue_SetsValueCorrectly()
        {
            // Arrange
            const string SpecialValue = "!@#$%^&*()_+-={}[]|\\:;\"'<>?,./ 中文 🚀";

            // Act
            var expectation = new TestExpectation(ExpectationType.BodyContains, SpecialValue);

            // Assert
            expectation.Value.Should().Be(SpecialValue);
        }

        [Fact]
        public void Constructor_WithSpecialCharactersInContext_SetsContextCorrectly()
        {
            // Arrange
            const string SpecialContext = "Special chars: !@#$%^&*()_+-={}[]|\\:;\"'<>?,./ 中文 🚀";

            // Act
            var expectation = new TestExpectation(ExpectationType.Header, "value", SpecialContext);

            // Assert
            expectation.Context.Should().Be(SpecialContext);
        }
    }
}
