using AwesomeAssertions;
using RESTClient.NET.Core.Exceptions;
using System;
using Xunit;

namespace RESTClient.NET.Core.Tests.Exceptions
{
    public class HttpParseExceptionTests
    {
        [Fact]
        public void Constructor_WithMinimalParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var message = "Test error message";

            // Act
            var exception = new HttpParseException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.LineNumber.Should().Be(0);
            exception.ColumnNumber.Should().Be(0);
            exception.ParsedContent.Should().BeNull();
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithAllParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var message = "Test error message";
            var lineNumber = 42;
            var columnNumber = 15;
            var parsedContent = "Some parsed content";
            var innerException = new ArgumentException("Inner exception");

            // Act
            var exception = new HttpParseException(message, lineNumber, columnNumber, parsedContent, innerException);

            // Assert
            exception.Message.Should().Be(message);
            exception.LineNumber.Should().Be(lineNumber);
            exception.ColumnNumber.Should().Be(columnNumber);
            exception.ParsedContent.Should().Be(parsedContent);
            exception.InnerException.Should().Be(innerException);
        }

        [Fact]
        public void Constructor_WithLineNumberOnly_ShouldInitializeCorrectly()
        {
            // Arrange
            var message = "Test error message";
            var lineNumber = 42;

            // Act
            var exception = new HttpParseException(message, lineNumber);

            // Assert
            exception.Message.Should().Be(message);
            exception.LineNumber.Should().Be(lineNumber);
            exception.ColumnNumber.Should().Be(0);
            exception.ParsedContent.Should().BeNull();
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void ToString_WithLineNumberOnly_ShouldReturnFormattedString()
        {
            // Arrange
            var message = "Test error message";
            var lineNumber = 42;
            var exception = new HttpParseException(message, lineNumber);

            // Act
            var result = exception.ToString();

            // Assert
            result.Should().Be("Test error message at line 42");
        }

        [Fact]
        public void ToString_WithLineAndColumnNumber_ShouldReturnFormattedString()
        {
            // Arrange
            var message = "Test error message";
            var lineNumber = 42;
            var columnNumber = 15;
            var exception = new HttpParseException(message, lineNumber, columnNumber);

            // Act
            var result = exception.ToString();

            // Assert
            result.Should().Be("Test error message at line 42, column 15");
        }

        [Fact]
        public void ToString_WithNoLineNumber_ShouldReturnMessageOnly()
        {
            // Arrange
            var message = "Test error message";
            var exception = new HttpParseException(message);

            // Act
            var result = exception.ToString();

            // Assert
            result.Should().Be("Test error message");
        }

        [Fact]
        public void ToString_WithZeroLineNumber_ShouldReturnMessageOnly()
        {
            // Arrange
            var message = "Test error message";
            var exception = new HttpParseException(message, 0);

            // Act
            var result = exception.ToString();

            // Assert
            result.Should().Be("Test error message");
        }

        [Fact]
        public void ToString_WithColumnNumberButNoLineNumber_ShouldReturnMessageWithColumn()
        {
            // Arrange
            var message = "Test error message";
            var exception = new HttpParseException(message, 0, 15);

            // Act
            var result = exception.ToString();

            // Assert
            result.Should().Be("Test error message, column 15");
        }
    }

    public class DuplicateRequestNameExceptionTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var requestName = "test-request";
            var currentLineNumber = 10;
            var firstOccurrenceLineNumber = 5;

            // Act
            var exception = new DuplicateRequestNameException(requestName, currentLineNumber, firstOccurrenceLineNumber);

            // Assert
            exception.DuplicateRequestName.Should().Be(requestName);
            exception.LineNumber.Should().Be(currentLineNumber);
            exception.FirstOccurrenceLineNumber.Should().Be(firstOccurrenceLineNumber);
            exception.Message.Should().Be($"Duplicate request name '{requestName}' found. First defined at line {firstOccurrenceLineNumber}");
        }

        [Fact]
        public void Constructor_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var requestName = "test-request_with-special_123";
            var currentLineNumber = 20;
            var firstOccurrenceLineNumber = 15;

            // Act
            var exception = new DuplicateRequestNameException(requestName, currentLineNumber, firstOccurrenceLineNumber);

            // Assert
            exception.DuplicateRequestName.Should().Be(requestName);
            exception.Message.Should().Contain(requestName);
        }

        [Fact]
        public void Constructor_WithEmptyRequestName_ShouldHandleCorrectly()
        {
            // Arrange
            var requestName = "";
            var currentLineNumber = 10;
            var firstOccurrenceLineNumber = 5;

            // Act
            var exception = new DuplicateRequestNameException(requestName, currentLineNumber, firstOccurrenceLineNumber);

            // Assert
            exception.DuplicateRequestName.Should().Be(requestName);
            exception.Message.Should().Contain("''");
        }
    }

    public class DuplicateRequestIdExceptionTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var requestId = "test-request-id";
            var currentLineNumber = 10;
            var firstOccurrenceLineNumber = 5;

            // Act
            var exception = new DuplicateRequestIdException(requestId, currentLineNumber, firstOccurrenceLineNumber);

            // Assert
            exception.DuplicateRequestId.Should().Be(requestId);
            exception.LineNumber.Should().Be(currentLineNumber);
            exception.FirstOccurrenceLineNumber.Should().Be(firstOccurrenceLineNumber);
            exception.Message.Should().Be($"Duplicate request ID '{requestId}' found. First defined at line {firstOccurrenceLineNumber}");
        }

        [Fact]
        public void Constructor_WithGuidRequestId_ShouldHandleCorrectly()
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var currentLineNumber = 20;
            var firstOccurrenceLineNumber = 15;

            // Act
            var exception = new DuplicateRequestIdException(requestId, currentLineNumber, firstOccurrenceLineNumber);

            // Assert
            exception.DuplicateRequestId.Should().Be(requestId);
            exception.Message.Should().Contain(requestId);
        }

        [Fact]
        public void Constructor_WithNullRequestId_ShouldHandleCorrectly()
        {
            // Arrange
            string? requestId = null;
            var currentLineNumber = 10;
            var firstOccurrenceLineNumber = 5;

            // Act
            var exception = new DuplicateRequestIdException(requestId!, currentLineNumber, firstOccurrenceLineNumber);

            // Assert
            exception.DuplicateRequestId.Should().BeNull();
            exception.Message.Should().Contain("''");
        }
    }

    public class MissingRequestIdExceptionTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var requestStartLineNumber = 42;

            // Act
            var exception = new MissingRequestIdException(requestStartLineNumber);

            // Assert
            exception.RequestStartLineNumber.Should().Be(requestStartLineNumber);
            exception.LineNumber.Should().Be(requestStartLineNumber);
            exception.Message.Should().Be($"Request at line {requestStartLineNumber} is missing a required request ID");
        }

        [Fact]
        public void Constructor_WithZeroLineNumber_ShouldHandleCorrectly()
        {
            // Arrange
            var requestStartLineNumber = 0;

            // Act
            var exception = new MissingRequestIdException(requestStartLineNumber);

            // Assert
            exception.RequestStartLineNumber.Should().Be(requestStartLineNumber);
            exception.LineNumber.Should().Be(requestStartLineNumber);
            exception.Message.Should().Be("Request at line 0 is missing a required request ID");
        }

        [Fact]
        public void Constructor_WithNegativeLineNumber_ShouldHandleCorrectly()
        {
            // Arrange
            var requestStartLineNumber = -1;

            // Act
            var exception = new MissingRequestIdException(requestStartLineNumber);

            // Assert
            exception.RequestStartLineNumber.Should().Be(requestStartLineNumber);
            exception.LineNumber.Should().Be(requestStartLineNumber);
            exception.Message.Should().Be("Request at line -1 is missing a required request ID");
        }
    }

    public class MissingRequestNameExceptionTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var requestStartLineNumber = 42;

            // Act
            var exception = new MissingRequestNameException(requestStartLineNumber);

            // Assert
            exception.RequestStartLineNumber.Should().Be(requestStartLineNumber);
            exception.LineNumber.Should().Be(requestStartLineNumber);
            exception.Message.Should().Be($"Request at line {requestStartLineNumber} is missing a required request name");
        }

        [Fact]
        public void Constructor_WithZeroLineNumber_ShouldHandleCorrectly()
        {
            // Arrange
            var requestStartLineNumber = 0;

            // Act
            var exception = new MissingRequestNameException(requestStartLineNumber);

            // Assert
            exception.RequestStartLineNumber.Should().Be(requestStartLineNumber);
            exception.LineNumber.Should().Be(requestStartLineNumber);
            exception.Message.Should().Be("Request at line 0 is missing a required request name");
        }

        [Fact]
        public void Constructor_WithLargeLineNumber_ShouldHandleCorrectly()
        {
            // Arrange
            var requestStartLineNumber = int.MaxValue;

            // Act
            var exception = new MissingRequestNameException(requestStartLineNumber);

            // Assert
            exception.RequestStartLineNumber.Should().Be(requestStartLineNumber);
            exception.LineNumber.Should().Be(requestStartLineNumber);
            exception.Message.Should().Be($"Request at line {requestStartLineNumber} is missing a required request name");
        }
    }

    public class InvalidRequestNameExceptionTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var invalidRequestName = "invalid request name with spaces";
            var lineNumber = 42;

            // Act
            var exception = new InvalidRequestNameException(invalidRequestName, lineNumber);

            // Assert
            exception.InvalidRequestName.Should().Be(invalidRequestName);
            exception.LineNumber.Should().Be(lineNumber);
            exception.Message.Should().Be($"Invalid request name '{invalidRequestName}'. Request names must contain only alphanumeric characters, hyphens, and underscores");
        }

        [Fact]
        public void Constructor_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var invalidRequestName = "test@request#name$with%special&chars";
            var lineNumber = 15;

            // Act
            var exception = new InvalidRequestNameException(invalidRequestName, lineNumber);

            // Assert
            exception.InvalidRequestName.Should().Be(invalidRequestName);
            exception.Message.Should().Contain(invalidRequestName);
            exception.Message.Should().Contain("alphanumeric characters, hyphens, and underscores");
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldHandleCorrectly()
        {
            // Arrange
            var invalidRequestName = "";
            var lineNumber = 5;

            // Act
            var exception = new InvalidRequestNameException(invalidRequestName, lineNumber);

            // Assert
            exception.InvalidRequestName.Should().Be(invalidRequestName);
            exception.Message.Should().Contain("''");
        }

        [Fact]
        public void Constructor_WithNullName_ShouldHandleCorrectly()
        {
            // Arrange
            string? invalidRequestName = null;
            var lineNumber = 5;

            // Act
            var exception = new InvalidRequestNameException(invalidRequestName!, lineNumber);

            // Assert
            exception.InvalidRequestName.Should().BeNull();
            exception.Message.Should().Contain("''");
        }
    }

    public class InvalidRequestIdExceptionTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var invalidRequestId = "invalid request id with spaces";
            var lineNumber = 42;

            // Act
            var exception = new InvalidRequestIdException(invalidRequestId, lineNumber);

            // Assert
            exception.InvalidRequestId.Should().Be(invalidRequestId);
            exception.LineNumber.Should().Be(lineNumber);
            exception.Message.Should().Be($"Invalid request ID '{invalidRequestId}'. Request IDs must contain only alphanumeric characters, hyphens, and underscores");
        }

        [Fact]
        public void Constructor_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var invalidRequestId = "test@request#id$with%special&chars";
            var lineNumber = 25;

            // Act
            var exception = new InvalidRequestIdException(invalidRequestId, lineNumber);

            // Assert
            exception.InvalidRequestId.Should().Be(invalidRequestId);
            exception.Message.Should().Contain(invalidRequestId);
            exception.Message.Should().Contain("alphanumeric characters, hyphens, and underscores");
        }

        [Fact]
        public void Constructor_WithEmptyId_ShouldHandleCorrectly()
        {
            // Arrange
            var invalidRequestId = "";
            var lineNumber = 10;

            // Act
            var exception = new InvalidRequestIdException(invalidRequestId, lineNumber);

            // Assert
            exception.InvalidRequestId.Should().Be(invalidRequestId);
            exception.Message.Should().Contain("''");
        }

        [Fact]
        public void Constructor_WithVeryLongId_ShouldHandleCorrectly()
        {
            // Arrange
            var invalidRequestId = new string('a', 1000) + " invalid";
            var lineNumber = 100;

            // Act
            var exception = new InvalidRequestIdException(invalidRequestId, lineNumber);

            // Assert
            exception.InvalidRequestId.Should().Be(invalidRequestId);
            exception.Message.Should().Contain(invalidRequestId);
        }
    }
}
