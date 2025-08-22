using AwesomeAssertions;
using RESTClient.NET.Core.Validation;
using Xunit;

namespace RESTClient.NET.Core.Tests.Validation
{
    public class ValidationResultTests
    {
        [Fact]
        public void Constructor_WithNoErrorsOrWarnings_ShouldBeValid()
        {
            // Act
            var result = new ValidationResult();

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeFalse();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithErrors_ShouldBeInvalid()
        {
            // Arrange
            var errors = new List<ValidationError>
            {
                new(1, "Test error", ValidationErrorType.InvalidHttpSyntax)
            };

            // Act
            var result = new ValidationResult(errors);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Message.Should().Be("Test error");
        }

        [Fact]
        public void Constructor_WithWarnings_ShouldBeValidButHaveWarnings()
        {
            // Arrange
            var warnings = new List<ValidationWarning>
            {
                new(1, "Test warning")
            };

            // Act
            var result = new ValidationResult(null, warnings);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Message.Should().Be("Test warning");
        }

        [Fact]
        public void Constructor_WithBothErrorsAndWarnings_ShouldBeInvalidAndHaveWarnings()
        {
            // Arrange
            var errors = new List<ValidationError>
            {
                new(1, "Test error", ValidationErrorType.InvalidHttpSyntax)
            };
            var warnings = new List<ValidationWarning>
            {
                new(2, "Test warning")
            };

            // Act
            var result = new ValidationResult(errors, warnings);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Warnings.Should().HaveCount(1);
        }

        [Fact]
        public void Constructor_WithNullErrors_ShouldHandleGracefully()
        {
            // Act
            var result = new ValidationResult(null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithNullWarnings_ShouldHandleGracefully()
        {
            // Act
            var result = new ValidationResult(null, null);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void Success_WithoutWarnings_ShouldCreateValidResult()
        {
            // Act
            var result = ValidationResult.Success();

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeFalse();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void Success_WithWarnings_ShouldCreateValidResultWithWarnings()
        {
            // Arrange
            var warnings = new List<ValidationWarning>
            {
                new(1, "Test warning")
            };

            // Act
            var result = ValidationResult.Success(warnings);

            // Assert
            result.IsValid.Should().BeTrue();
            result.HasErrors.Should().BeFalse();
            result.HasWarnings.Should().BeTrue();
            result.Warnings.Should().HaveCount(1);
        }

        [Fact]
        public void Failure_WithErrors_ShouldCreateInvalidResult()
        {
            // Arrange
            var errors = new List<ValidationError>
            {
                new(1, "Test error", ValidationErrorType.InvalidHttpSyntax)
            };

            // Act
            var result = ValidationResult.Failure(errors);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.HasWarnings.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        [Fact]
        public void Failure_WithErrorsAndWarnings_ShouldCreateInvalidResultWithWarnings()
        {
            // Arrange
            var errors = new List<ValidationError>
            {
                new(1, "Test error", ValidationErrorType.InvalidHttpSyntax)
            };
            var warnings = new List<ValidationWarning>
            {
                new(2, "Test warning")
            };

            // Act
            var result = ValidationResult.Failure(errors, warnings);

            // Assert
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Warnings.Should().HaveCount(1);
        }

        [Fact]
        public void Errors_ShouldBeReadOnly()
        {
            // Arrange
            var errors = new List<ValidationError>
            {
                new(1, "Test error", ValidationErrorType.InvalidHttpSyntax)
            };

            // Act
            var result = new ValidationResult(errors);

            // Assert
            result.Errors.Should().BeAssignableTo<IReadOnlyList<ValidationError>>();
        }

        [Fact]
        public void Warnings_ShouldBeReadOnly()
        {
            // Arrange
            var warnings = new List<ValidationWarning>
            {
                new(1, "Test warning")
            };

            // Act
            var result = new ValidationResult(null, warnings);

            // Assert
            result.Warnings.Should().BeAssignableTo<IReadOnlyList<ValidationWarning>>();
        }
    }

    public class ValidationErrorTests
    {
        [Fact]
        public void Constructor_WithAllParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            int lineNumber = 42;
            string message = "Test error message";
            ValidationErrorType type = ValidationErrorType.InvalidHttpSyntax;
            string context = "Additional context";

            // Act
            var error = new ValidationError(lineNumber, message, type, context);

            // Assert
            error.LineNumber.Should().Be(lineNumber);
            error.Message.Should().Be(message);
            error.Type.Should().Be(type);
            error.Context.Should().Be(context);
        }

        [Fact]
        public void Constructor_WithoutContext_ShouldSetContextToNull()
        {
            // Arrange
            int lineNumber = 42;
            string message = "Test error message";
            ValidationErrorType type = ValidationErrorType.InvalidHttpSyntax;

            // Act
            var error = new ValidationError(lineNumber, message, type);

            // Assert
            error.LineNumber.Should().Be(lineNumber);
            error.Message.Should().Be(message);
            error.Type.Should().Be(type);
            error.Context.Should().BeNull();
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var error = new ValidationError(42, "Test error message", ValidationErrorType.InvalidHttpSyntax);

            // Act
            string result = error.ToString();

            // Assert
            result.Should().Be("Line 42: Test error message");
        }

        [Theory]
        [InlineData(ValidationErrorType.InvalidRequestName)]
        [InlineData(ValidationErrorType.DuplicateRequestName)]
        [InlineData(ValidationErrorType.InvalidHttpSyntax)]
        [InlineData(ValidationErrorType.InvalidVariable)]
        [InlineData(ValidationErrorType.MissingRequestName)]
        [InlineData(ValidationErrorType.InvalidExpectation)]
        public void Constructor_WithDifferentErrorTypes_ShouldStoreCorrectly(ValidationErrorType errorType)
        {
            // Arrange & Act
            var error = new ValidationError(1, "Test message", errorType);

            // Assert
            error.Type.Should().Be(errorType);
        }
    }

    public class ValidationWarningTests
    {
        [Fact]
        public void Constructor_WithAllParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            int lineNumber = 42;
            string message = "Test warning message";
            string context = "Additional context";

            // Act
            var warning = new ValidationWarning(lineNumber, message, context);

            // Assert
            warning.LineNumber.Should().Be(lineNumber);
            warning.Message.Should().Be(message);
            warning.Context.Should().Be(context);
        }

        [Fact]
        public void Constructor_WithoutContext_ShouldSetContextToNull()
        {
            // Arrange
            int lineNumber = 42;
            string message = "Test warning message";

            // Act
            var warning = new ValidationWarning(lineNumber, message);

            // Assert
            warning.LineNumber.Should().Be(lineNumber);
            warning.Message.Should().Be(message);
            warning.Context.Should().BeNull();
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var warning = new ValidationWarning(42, "Test warning message");

            // Act
            string result = warning.ToString();

            // Assert
            result.Should().Be("Line 42: Test warning message (Warning)");
        }

        [Fact]
        public void ToString_WithContext_ShouldReturnFormattedString()
        {
            // Arrange
            var warning = new ValidationWarning(42, "Test warning message", "context");

            // Act
            string result = warning.ToString();

            // Assert
            result.Should().Be("Line 42: Test warning message (Warning)");
        }
    }
}
