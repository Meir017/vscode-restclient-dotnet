using AwesomeAssertions;
using RESTClient.NET.Core.Parsing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Parsing
{
    public class HttpParseOptionsTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaults()
        {
            // Act
            var options = new HttpParseOptions();

            // Assert
            options.ValidateRequestNames.Should().BeTrue();
            options.ProcessVariables.Should().BeTrue();
            options.StrictMode.Should().BeFalse();
            options.ParseExpectations.Should().BeTrue();
            options.RequireRequestNames.Should().BeTrue();
            options.AllowEmptyBodies.Should().BeTrue();
            options.NormalizeLineEndings.Should().BeTrue();
            options.MaxRequestNameLength.Should().Be(50);
            options.IgnoreUnknownMetadata.Should().BeTrue();
        }

        [Fact]
        public void Properties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act
            options.ValidateRequestNames = false;
            options.ProcessVariables = false;
            options.StrictMode = true;
            options.ParseExpectations = false;
            options.RequireRequestNames = false;
            options.AllowEmptyBodies = false;
            options.NormalizeLineEndings = false;
            options.MaxRequestNameLength = 100;
            options.IgnoreUnknownMetadata = false;

            // Assert
            options.ValidateRequestNames.Should().BeFalse();
            options.ProcessVariables.Should().BeFalse();
            options.StrictMode.Should().BeTrue();
            options.ParseExpectations.Should().BeFalse();
            options.RequireRequestNames.Should().BeFalse();
            options.AllowEmptyBodies.Should().BeFalse();
            options.NormalizeLineEndings.Should().BeFalse();
            options.MaxRequestNameLength.Should().Be(100);
            options.IgnoreUnknownMetadata.Should().BeFalse();
        }

        [Fact]
        public void Default_ShouldReturnDefaultOptions()
        {
            // Act
            var options = HttpParseOptions.Default();

            // Assert
            options.Should().NotBeNull();
            options.ValidateRequestNames.Should().BeTrue();
            options.ProcessVariables.Should().BeTrue();
            options.StrictMode.Should().BeFalse();
            options.ParseExpectations.Should().BeTrue();
            options.RequireRequestNames.Should().BeTrue();
            options.AllowEmptyBodies.Should().BeTrue();
            options.NormalizeLineEndings.Should().BeTrue();
            options.MaxRequestNameLength.Should().Be(50);
            options.IgnoreUnknownMetadata.Should().BeTrue();
        }

        [Fact]
        public void Strict_ShouldReturnStrictOptions()
        {
            // Act
            var options = HttpParseOptions.Strict();

            // Assert
            options.Should().NotBeNull();
            options.StrictMode.Should().BeTrue();
            options.RequireRequestNames.Should().BeTrue();
            options.IgnoreUnknownMetadata.Should().BeFalse();
            options.AllowEmptyBodies.Should().BeFalse();
            
            // These should retain default values
            options.ValidateRequestNames.Should().BeTrue();
            options.ProcessVariables.Should().BeTrue();
            options.ParseExpectations.Should().BeTrue();
            options.NormalizeLineEndings.Should().BeTrue();
            options.MaxRequestNameLength.Should().Be(50);
        }

        [Fact]
        public void Lenient_ShouldReturnLenientOptions()
        {
            // Act
            var options = HttpParseOptions.Lenient();

            // Assert
            options.Should().NotBeNull();
            options.ValidateRequestNames.Should().BeFalse();
            options.RequireRequestNames.Should().BeFalse();
            options.StrictMode.Should().BeFalse();
            options.IgnoreUnknownMetadata.Should().BeTrue();
            
            // These should retain default values
            options.ProcessVariables.Should().BeTrue();
            options.ParseExpectations.Should().BeTrue();
            options.AllowEmptyBodies.Should().BeTrue();
            options.NormalizeLineEndings.Should().BeTrue();
            options.MaxRequestNameLength.Should().Be(50);
        }

        [Fact]
        public void Default_ShouldReturnNewInstanceEachTime()
        {
            // Act
            var options1 = HttpParseOptions.Default();
            var options2 = HttpParseOptions.Default();

            // Assert
            options1.Should().NotBeSameAs(options2);
        }

        [Fact]
        public void Strict_ShouldReturnNewInstanceEachTime()
        {
            // Act
            var options1 = HttpParseOptions.Strict();
            var options2 = HttpParseOptions.Strict();

            // Assert
            options1.Should().NotBeSameAs(options2);
        }

        [Fact]
        public void Lenient_ShouldReturnNewInstanceEachTime()
        {
            // Act
            var options1 = HttpParseOptions.Lenient();
            var options2 = HttpParseOptions.Lenient();

            // Assert
            options1.Should().NotBeSameAs(options2);
        }

        [Fact]
        public void ValidateRequestNames_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default true
            options.ValidateRequestNames.Should().BeTrue();

            // Act & Assert - set to false
            options.ValidateRequestNames = false;
            options.ValidateRequestNames.Should().BeFalse();

            // Act & Assert - set back to true
            options.ValidateRequestNames = true;
            options.ValidateRequestNames.Should().BeTrue();
        }

        [Fact]
        public void ProcessVariables_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default true
            options.ProcessVariables.Should().BeTrue();

            // Act & Assert - set to false
            options.ProcessVariables = false;
            options.ProcessVariables.Should().BeFalse();

            // Act & Assert - set back to true
            options.ProcessVariables = true;
            options.ProcessVariables.Should().BeTrue();
        }

        [Fact]
        public void StrictMode_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default false
            options.StrictMode.Should().BeFalse();

            // Act & Assert - set to true
            options.StrictMode = true;
            options.StrictMode.Should().BeTrue();

            // Act & Assert - set back to false
            options.StrictMode = false;
            options.StrictMode.Should().BeFalse();
        }

        [Fact]
        public void ParseExpectations_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default true
            options.ParseExpectations.Should().BeTrue();

            // Act & Assert - set to false
            options.ParseExpectations = false;
            options.ParseExpectations.Should().BeFalse();

            // Act & Assert - set back to true
            options.ParseExpectations = true;
            options.ParseExpectations.Should().BeTrue();
        }

        [Fact]
        public void RequireRequestNames_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default true
            options.RequireRequestNames.Should().BeTrue();

            // Act & Assert - set to false
            options.RequireRequestNames = false;
            options.RequireRequestNames.Should().BeFalse();

            // Act & Assert - set back to true
            options.RequireRequestNames = true;
            options.RequireRequestNames.Should().BeTrue();
        }

        [Fact]
        public void AllowEmptyBodies_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default true
            options.AllowEmptyBodies.Should().BeTrue();

            // Act & Assert - set to false
            options.AllowEmptyBodies = false;
            options.AllowEmptyBodies.Should().BeFalse();

            // Act & Assert - set back to true
            options.AllowEmptyBodies = true;
            options.AllowEmptyBodies.Should().BeTrue();
        }

        [Fact]
        public void NormalizeLineEndings_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default true
            options.NormalizeLineEndings.Should().BeTrue();

            // Act & Assert - set to false
            options.NormalizeLineEndings = false;
            options.NormalizeLineEndings.Should().BeFalse();

            // Act & Assert - set back to true
            options.NormalizeLineEndings = true;
            options.NormalizeLineEndings.Should().BeTrue();
        }

        [Fact]
        public void IgnoreUnknownMetadata_ShouldBeBooleanProperty()
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act & Assert - default true
            options.IgnoreUnknownMetadata.Should().BeTrue();

            // Act & Assert - set to false
            options.IgnoreUnknownMetadata = false;
            options.IgnoreUnknownMetadata.Should().BeFalse();

            // Act & Assert - set back to true
            options.IgnoreUnknownMetadata = true;
            options.IgnoreUnknownMetadata.Should().BeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(200)]
        public void MaxRequestNameLength_ShouldAcceptPositiveValues(int length)
        {
            // Arrange
            var options = new HttpParseOptions();

            // Act
            options.MaxRequestNameLength = length;

            // Assert
            options.MaxRequestNameLength.Should().Be(length);
        }

        [Fact]
        public void MaxRequestNameLength_ShouldDefaultTo50()
        {
            // Arrange & Act
            var options = new HttpParseOptions();

            // Assert
            options.MaxRequestNameLength.Should().Be(50);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void MaxRequestNameLength_ShouldAcceptZeroAndNegativeValues(int length)
        {
            // Note: The class doesn't validate the value, so this documents current behavior
            // Arrange
            var options = new HttpParseOptions();

            // Act
            options.MaxRequestNameLength = length;

            // Assert
            options.MaxRequestNameLength.Should().Be(length);
        }

        [Fact]
        public void FactoryMethods_ShouldProduceDistinctConfigurations()
        {
            // Act
            var defaultOptions = HttpParseOptions.Default();
            var strictOptions = HttpParseOptions.Strict();
            var lenientOptions = HttpParseOptions.Lenient();

            // Assert - Strict mode differences
            defaultOptions.StrictMode.Should().BeFalse();
            strictOptions.StrictMode.Should().BeTrue();
            lenientOptions.StrictMode.Should().BeFalse();

            // Assert - AllowEmptyBodies differences
            defaultOptions.AllowEmptyBodies.Should().BeTrue();
            strictOptions.AllowEmptyBodies.Should().BeFalse();
            lenientOptions.AllowEmptyBodies.Should().BeTrue();

            // Assert - ValidateRequestNames differences
            defaultOptions.ValidateRequestNames.Should().BeTrue();
            strictOptions.ValidateRequestNames.Should().BeTrue();
            lenientOptions.ValidateRequestNames.Should().BeFalse();

            // Assert - RequireRequestNames differences
            defaultOptions.RequireRequestNames.Should().BeTrue();
            strictOptions.RequireRequestNames.Should().BeTrue();
            lenientOptions.RequireRequestNames.Should().BeFalse();

            // Assert - IgnoreUnknownMetadata differences
            defaultOptions.IgnoreUnknownMetadata.Should().BeTrue();
            strictOptions.IgnoreUnknownMetadata.Should().BeFalse();
            lenientOptions.IgnoreUnknownMetadata.Should().BeTrue();
        }

        [Fact]
        public void CustomConfiguration_ShouldAllowMixingOptions()
        {
            // Arrange & Act
            var options = new HttpParseOptions
            {
                StrictMode = true,
                ValidateRequestNames = false,
                ProcessVariables = false,
                ParseExpectations = true,
                RequireRequestNames = false,
                AllowEmptyBodies = true,
                NormalizeLineEndings = false,
                MaxRequestNameLength = 75,
                IgnoreUnknownMetadata = false
            };

            // Assert
            options.StrictMode.Should().BeTrue();
            options.ValidateRequestNames.Should().BeFalse();
            options.ProcessVariables.Should().BeFalse();
            options.ParseExpectations.Should().BeTrue();
            options.RequireRequestNames.Should().BeFalse();
            options.AllowEmptyBodies.Should().BeTrue();
            options.NormalizeLineEndings.Should().BeFalse();
            options.MaxRequestNameLength.Should().Be(75);
            options.IgnoreUnknownMetadata.Should().BeFalse();
        }
    }
}
