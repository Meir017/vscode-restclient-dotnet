using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Parsing;

namespace RESTClient.NET.Core.Validation
{
    /// <summary>
    /// Default implementation of HTTP file validator
    /// </summary>
    public partial class HttpFileValidator : IHttpFileValidator
    {
        private static readonly Regex _requestNameValidationRegex = MyRegex();
        private static readonly Regex _urlValidationRegex = new Regex(@"^https?://|^/|^\{\{", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public ValidationResult Validate(HttpFile httpFile)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            var errors = new List<ValidationError>();
            var warnings = new List<ValidationWarning>();

            // Validate request names
            ValidateRequestNames(httpFile, errors, warnings);

            // Validate individual requests
            ValidateRequests(httpFile, errors, warnings);

            // Validate file variables
            ValidateFileVariables(httpFile, errors, warnings);

            return new ValidationResult(errors, warnings);
        }

        private static void ValidateRequestNames(HttpFile httpFile, List<ValidationError> errors, List<ValidationWarning> warnings)
        {
            var requestNames = new HashSet<string>();
            var requestNameCounts = new Dictionary<string, int>();

            foreach (HttpRequest request in httpFile.Requests)
            {
                string requestName = request.Name;

                // Check for empty request names
                if (string.IsNullOrWhiteSpace(requestName))
                {
                    errors.Add(new ValidationError(
                        request.LineNumber,
                        "Request is missing a required request name",
                        ValidationErrorType.MissingRequestName));
                    continue;
                }

                // Check request name format
                if (!_requestNameValidationRegex.IsMatch(requestName))
                {
                    errors.Add(new ValidationError(
                        request.LineNumber,
                        $"Invalid request name '{requestName}'. Request names must contain only alphanumeric characters, hyphens, and underscores",
                        ValidationErrorType.InvalidRequestName));
                    continue;
                }

                // Check request name length
                if (requestName.Length > 50)
                {
                    errors.Add(new ValidationError(
                        request.LineNumber,
                        $"Request name '{requestName}' is too long. Maximum length is 50 characters",
                        ValidationErrorType.InvalidRequestName));
                    continue;
                }

                // Track duplicates
                if (!requestNames.Add(requestName))
                {
                    requestNameCounts[requestName] = requestNameCounts.TryGetValue(requestName, out int count) ? count + 1 : 2;
                }
                else
                {
                    requestNameCounts[requestName] = 1;
                }
            }

            // Report duplicate request names
            foreach (KeyValuePair<string, int> kvp in requestNameCounts.Where(x => x.Value > 1))
            {
                var duplicateRequests = httpFile.Requests
                    .Where(r => r.Name == kvp.Key)
                    .Skip(1) // Skip the first occurrence
                    .ToList();

                foreach (HttpRequest? request in duplicateRequests)
                {
                    errors.Add(new ValidationError(
                        request.LineNumber,
                        $"Duplicate request name '{kvp.Key}' found",
                        ValidationErrorType.DuplicateRequestName));
                }
            }
        }

        private static void ValidateRequests(HttpFile httpFile, List<ValidationError> errors, List<ValidationWarning> warnings)
        {
            foreach (HttpRequest request in httpFile.Requests)
            {
                // Validate HTTP method
                ValidateHttpMethod(request, errors, warnings);

                // Validate URL
                ValidateUrl(request, errors, warnings);

                // Validate headers
                ValidateHeaders(request, errors, warnings);

                // Validate expectations
                ValidateExpectations(request, errors, warnings);
            }
        }

        private static void ValidateHttpMethod(HttpRequest request, List<ValidationError> errors, List<ValidationWarning> warnings)
        {
            string[] validMethods = new[]
            {
                "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS",
                "CONNECT", "TRACE", "LOCK", "UNLOCK", "PROPFIND", "PROPPATCH",
                "COPY", "MOVE", "MKCOL", "MKCALENDAR", "ACL", "SEARCH"
            };

            if (string.IsNullOrWhiteSpace(request.Method))
            {
                warnings.Add(new ValidationWarning(
                    request.LineNumber,
                    "HTTP method is empty, defaulting to GET"));
            }
            else if (!validMethods.Contains(request.Method.ToUpperInvariant()))
            {
                warnings.Add(new ValidationWarning(
                    request.LineNumber,
                    $"Unknown HTTP method '{request.Method}'"));
            }
        }

        private static void ValidateUrl(HttpRequest request, List<ValidationError> errors, List<ValidationWarning> warnings)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                errors.Add(new ValidationError(
                    request.LineNumber,
                    "Request URL is required",
                    ValidationErrorType.InvalidHttpSyntax));
                return;
            }

            // Basic URL format validation
            if (!_urlValidationRegex.IsMatch(request.Url))
            {
                warnings.Add(new ValidationWarning(
                    request.LineNumber,
                    $"URL '{request.Url}' may not be valid. Expected format: http://..., https://..., /path, or {{{{variable}}}}"));
            }

            // Check for common URL issues
            if (request.Url.Contains(' ') && !request.Url.Contains("{{"))
            {
                warnings.Add(new ValidationWarning(
                    request.LineNumber,
                    "URL contains spaces. Consider URL encoding"));
            }
        }

        private static void ValidateHeaders(HttpRequest request, List<ValidationError> errors, List<ValidationWarning> warnings)
        {
            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                // Check for empty header names
                if (string.IsNullOrWhiteSpace(header.Key))
                {
                    errors.Add(new ValidationError(
                        request.LineNumber,
                        "Header name cannot be empty",
                        ValidationErrorType.InvalidHttpSyntax));
                    continue;
                }

                // Check for invalid header name characters
                if (header.Key.Contains(' ') || header.Key.Contains('\t'))
                {
                    warnings.Add(new ValidationWarning(
                        request.LineNumber,
                        $"Header name '{header.Key}' contains whitespace"));
                }

                // Check for specific header validations
                ValidateSpecificHeaders(request, header.Key, header.Value, errors, warnings);
            }
        }

        private static void ValidateSpecificHeaders(
            HttpRequest request,
            string headerName,
            string headerValue,
            List<ValidationError> errors,
            List<ValidationWarning> warnings)
        {
            string lowerHeaderName = headerName.ToLowerInvariant();

            switch (lowerHeaderName)
            {
                case "content-type":
                    if (string.IsNullOrWhiteSpace(headerValue))
                    {
                        warnings.Add(new ValidationWarning(
                            request.LineNumber,
                            "Content-Type header is empty"));
                    }
                    break;

                case "authorization":
                    if (string.IsNullOrWhiteSpace(headerValue))
                    {
                        warnings.Add(new ValidationWarning(
                            request.LineNumber,
                            "Authorization header is empty"));
                    }
                    break;

                case "content-length":
                    warnings.Add(new ValidationWarning(
                        request.LineNumber,
                        "Content-Length header will be automatically calculated"));
                    break;

                default:
                    // No specific validation for other headers
                    break;
            }
        }

        private static void ValidateExpectations(HttpRequest request, List<ValidationError> errors, List<ValidationWarning> warnings)
        {
            foreach (TestExpectation expectation in request.Metadata.Expectations)
            {
                switch (expectation.Type)
                {
                    case ExpectationType.StatusCode:
                        if (!int.TryParse(expectation.Value, out int statusCode) || statusCode < 100 || statusCode >= 600)
                        {
                            errors.Add(new ValidationError(
                                request.LineNumber,
                                $"Invalid status code expectation '{expectation.Value}'. Must be a number between 100-599",
                                ValidationErrorType.InvalidExpectation));
                        }
                        break;

                    case ExpectationType.MaxTime:
                        if (!expectation.Value.EndsWith("ms", StringComparison.Ordinal) ||
                            !int.TryParse(expectation.Value.AsSpan(0, expectation.Value.Length - 2), out int timeMs) ||
                            timeMs <= 0)
                        {
                            errors.Add(new ValidationError(
                                request.LineNumber,
                                $"Invalid max-time expectation '{expectation.Value}'. Must be a positive number followed by 'ms'",
                                ValidationErrorType.InvalidExpectation));
                        }
                        break;

                    case ExpectationType.BodyPath:
                        if (string.IsNullOrWhiteSpace(expectation.Value))
                        {
                            errors.Add(new ValidationError(
                                request.LineNumber,
                                "body-path expectation cannot be empty",
                                ValidationErrorType.InvalidExpectation));
                        }
                        break;

                    case ExpectationType.Schema:
                        if (string.IsNullOrWhiteSpace(expectation.Value))
                        {
                            errors.Add(new ValidationError(
                                request.LineNumber,
                                "schema expectation cannot be empty",
                                ValidationErrorType.InvalidExpectation));
                        }
                        break;

                    case ExpectationType.Header:
                        if (string.IsNullOrWhiteSpace(expectation.Value))
                        {
                            errors.Add(new ValidationError(
                                request.LineNumber,
                                "header expectation cannot be empty",
                                ValidationErrorType.InvalidExpectation));
                        }
                        break;

                    case ExpectationType.BodyContains:
                        if (string.IsNullOrWhiteSpace(expectation.Value))
                        {
                            errors.Add(new ValidationError(
                                request.LineNumber,
                                "body-contains expectation cannot be empty",
                                ValidationErrorType.InvalidExpectation));
                        }
                        break;

                    default:
                        // Unknown expectation type
                        break;
                }
            }
        }

        private static void ValidateFileVariables(HttpFile httpFile, List<ValidationError> errors, List<ValidationWarning> warnings)
        {
            foreach (KeyValuePair<string, string> variable in httpFile.FileVariables)
            {
                // Check for empty variable names
                if (string.IsNullOrWhiteSpace(variable.Key))
                {
                    errors.Add(new ValidationError(
                        0,
                        "Variable name cannot be empty",
                        ValidationErrorType.InvalidVariable));
                    continue;
                }

                // Check for invalid variable name characters
                if (variable.Key.Contains(' ') || variable.Key.Contains('\t'))
                {
                    warnings.Add(new ValidationWarning(
                        0,
                        $"Variable name '{variable.Key}' contains whitespace"));
                }

                // Check for circular references (basic check)
                if (variable.Value.Contains($"{{{{{variable.Key}}}}}"))
                {
                    errors.Add(new ValidationError(
                        0,
                        $"Variable '{variable.Key}' has a circular reference to itself",
                        ValidationErrorType.InvalidVariable));
                }
            }
        }

        [GeneratedRegex(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
    }
}
