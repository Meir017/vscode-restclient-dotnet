using System.Collections.Generic;
using System.Linq;

namespace RESTClient.NET.Core.Validation
{
    /// <summary>
    /// Represents the result of HTTP file validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the validation passed
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// Gets a value indicating whether there are any errors
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Gets a value indicating whether there are any warnings
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;

        /// <summary>
        /// Gets the validation errors
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Gets the validation warnings
        /// </summary>
        public IReadOnlyList<ValidationWarning> Warnings { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationResult class
        /// </summary>
        /// <param name="errors">The validation errors</param>
        /// <param name="warnings">The validation warnings</param>
        public ValidationResult(
            IEnumerable<ValidationError>? errors = null,
            IEnumerable<ValidationWarning>? warnings = null)
        {
            Errors = (errors ?? []).ToList().AsReadOnly();
            Warnings = (warnings ?? []).ToList().AsReadOnly();
        }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        /// <param name="warnings">Optional warnings</param>
        /// <returns>A successful validation result</returns>
        public static ValidationResult Success(IEnumerable<ValidationWarning>? warnings = null)
        {
            return new ValidationResult(null, warnings);
        }

        /// <summary>
        /// Creates a failed validation result
        /// </summary>
        /// <param name="errors">The validation errors</param>
        /// <param name="warnings">Optional warnings</param>
        /// <returns>A failed validation result</returns>
        public static ValidationResult Failure(IEnumerable<ValidationError> errors, IEnumerable<ValidationWarning>? warnings = null)
        {
            return new ValidationResult(errors, warnings);
        }
    }

    /// <summary>
    /// Represents a validation error
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets the line number where the error occurred
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the error message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the type of validation error
        /// </summary>
        public ValidationErrorType Type { get; }

        /// <summary>
        /// Gets additional context about the error
        /// </summary>
        public string? Context { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationError class
        /// </summary>
        /// <param name="lineNumber">The line number where the error occurred</param>
        /// <param name="message">The error message</param>
        /// <param name="type">The type of validation error</param>
        /// <param name="context">Additional context</param>
        public ValidationError(int lineNumber, string message, ValidationErrorType type, string? context = null)
        {
            LineNumber = lineNumber;
            Message = message;
            Type = type;
            Context = context;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Line {LineNumber}: {Message}";
        }
    }

    /// <summary>
    /// Represents a validation warning
    /// </summary>
    public class ValidationWarning
    {
        /// <summary>
        /// Gets the line number where the warning occurred
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the warning message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets additional context about the warning
        /// </summary>
        public string? Context { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationWarning class
        /// </summary>
        /// <param name="lineNumber">The line number where the warning occurred</param>
        /// <param name="message">The warning message</param>
        /// <param name="context">Additional context</param>
        public ValidationWarning(int lineNumber, string message, string? context = null)
        {
            LineNumber = lineNumber;
            Message = message;
            Context = context;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Line {LineNumber}: {Message} (Warning)";
        }
    }

    /// <summary>
    /// Defines the types of validation errors
    /// </summary>
    public enum ValidationErrorType
    {
        /// <summary>
        /// Invalid or missing request name
        /// </summary>
        InvalidRequestName,

        /// <summary>
        /// Duplicate request name
        /// </summary>
        DuplicateRequestName,

        /// <summary>
        /// Invalid HTTP syntax
        /// </summary>
        InvalidHttpSyntax,

        /// <summary>
        /// Invalid variable definition
        /// </summary>
        InvalidVariable,

        /// <summary>
        /// Missing required request name
        /// </summary>
        MissingRequestName,

        /// <summary>
        /// Invalid expectation format
        /// </summary>
        InvalidExpectation
    }
}
