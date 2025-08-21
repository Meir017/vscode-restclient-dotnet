using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Parsing;
using RESTClient.NET.Core.Processing;
using RESTClient.NET.Core.Validation;

namespace RESTClient.NET.Core
{
    /// <summary>
    /// Main facade for parsing and processing HTTP files with comprehensive variable resolution and validation.
    /// Provides the primary entry point for all HTTP file operations in RESTClient.NET.
    /// </summary>
    /// <remarks>
    /// <para>HttpFileProcessor serves as the main orchestrator for HTTP file processing:</para>
    /// <list type="number">
    /// <item>File parsing: Reads and parses .http files from disk or content strings</item>
    /// <item>Variable resolution: Processes file variables and system variables</item>
    /// <item>Validation: Ensures request names and syntax compliance</item>
    /// <item>Error handling: Provides detailed parsing and validation error information</item>
    /// </list>
    /// <para>Supports both synchronous and asynchronous operations for different usage scenarios.</para>
    /// <para>Integrates with Microsoft.Extensions.Logging for comprehensive operation tracking.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic usage with file path
    /// var processor = new HttpFileProcessor();
    /// var httpFile = await processor.ParseFileAsync("api-tests.http");
    ///
    /// // With logging and custom validation
    /// var logger = serviceProvider.GetService&lt;ILogger&lt;HttpFileProcessor&gt;&gt;();
    /// var validator = new CustomHttpFileValidator();
    /// var processor = new HttpFileProcessor(logger, validator);
    ///
    /// // Parse from content string
    /// var content = @"
    /// @baseUrl = https://api.example.com
    ///
    /// # @name health-check
    /// GET {{baseUrl}}/health HTTP/1.1";
    ///
    /// var httpFile = processor.ParseContent(content);
    /// var healthCheck = httpFile.GetRequestByName("health-check");
    ///
    /// // Process with variable resolution
    /// var processedFile = await processor.ProcessFileAsync("complex-api.http",
    ///     new HttpParseOptions { ResolveVariables = true });
    /// </code>
    /// </example>
    public class HttpFileProcessor
    {
        private readonly HttpFileParser _parser;
        private readonly IHttpFileValidator _validator;
        private readonly ILogger<HttpFileProcessor>? _logger;

        /// <summary>
        /// Initializes a new instance of HttpFileProcessor
        /// </summary>
        /// <param name="logger">Optional logger</param>
        /// <param name="validator">Optional custom validator</param>
        public HttpFileProcessor(ILogger<HttpFileProcessor>? logger = null, IHttpFileValidator? validator = null)
        {
            _logger = logger;
            _validator = validator ?? new HttpFileValidator();
            _parser = new HttpFileParser();
        }

        /// <summary>
        /// Parses an HTTP file from a file path
        /// </summary>
        /// <param name="filePath">Path to the HTTP file</param>
        /// <param name="options">Parsing options</param>
        /// <returns>Parsed HTTP file</returns>
        public async Task<HttpFile> ParseFileAsync(string filePath, HttpParseOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"HTTP file not found: {filePath}");
            }

            _logger?.LogInformation("Parsing HTTP file: {FilePath}", filePath);

            return await _parser.ParseFileAsync(filePath, options ?? new HttpParseOptions());
        }

        /// <summary>
        /// Parses HTTP content from a string
        /// </summary>
        /// <param name="content">HTTP file content</param>
        /// <param name="options">Parsing options</param>
        /// <returns>Parsed HTTP file</returns>
        public Task<HttpFile> ParseContentAsync(string content, HttpParseOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(content);

            _logger?.LogInformation("Parsing HTTP content ({Length} characters)", content.Length);

            return _parser.ParseAsync(content, options ?? new HttpParseOptions());
        }

        /// <summary>
        /// Parses and processes an HTTP file with variable resolution
        /// </summary>
        /// <param name="filePath">Path to the HTTP file</param>
        /// <param name="environmentVariables">Environment variables for resolution</param>
        /// <param name="options">Parsing options</param>
        /// <returns>Processed HTTP file with variables resolved</returns>
        public async Task<HttpFile> ParseAndProcessFileAsync(
            string filePath,
            IDictionary<string, string>? environmentVariables = null,
            HttpParseOptions? options = null)
        {
            HttpFile httpFile = await ParseFileAsync(filePath, options);
            return ProcessVariables(httpFile, environmentVariables);
        }

        /// <summary>
        /// Parses and processes HTTP content with variable resolution
        /// </summary>
        /// <param name="content">HTTP file content</param>
        /// <param name="environmentVariables">Environment variables for resolution</param>
        /// <param name="options">Parsing options</param>
        /// <returns>Processed HTTP file with variables resolved</returns>
        public async Task<HttpFile> ParseAndProcessContentAsync(
            string content,
            IDictionary<string, string>? environmentVariables = null,
            HttpParseOptions? options = null)
        {
            HttpFile httpFile = await ParseContentAsync(content, options);
            return ProcessVariables(httpFile, environmentVariables);
        }

        /// <summary>
        /// Processes variables in an already parsed HTTP file
        /// </summary>
        /// <param name="httpFile">The HTTP file to process</param>
        /// <param name="environmentVariables">Environment variables for resolution</param>
        /// <returns>Processed HTTP file with variables resolved</returns>
        public HttpFile ProcessVariables(
            HttpFile httpFile,
            IDictionary<string, string>? environmentVariables = null)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            _logger?.LogInformation("Processing variables in HTTP file with {RequestCount} requests", httpFile.Requests.Count);

            // Validate variable references
            ValidateVariableReferences(httpFile, environmentVariables);

            // Process the file
            HttpFile processedFile = VariableProcessor.ProcessHttpFile(httpFile, environmentVariables);

            _logger?.LogInformation("Variable processing completed");

            return processedFile;
        }

        /// <summary>
        /// Validates variable references in an HTTP file
        /// </summary>
        /// <param name="httpFile">The HTTP file to validate</param>
        /// <param name="environmentVariables">Available environment variables</param>
        /// <returns>Validation result with any variable reference issues</returns>
        public ValidationResult ValidateVariableReferences(
            HttpFile httpFile,
            IDictionary<string, string>? environmentVariables = null)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            var errors = new List<ValidationError>();
            var warnings = new List<ValidationWarning>();

            // Check for circular references in file variables
            List<string> circularVariables = VariableProcessor.DetectCircularReferences(httpFile.FileVariables);
            foreach (string variable in circularVariables)
            {
                errors.Add(new ValidationError(
                    0,
                    $"Circular reference detected in variable '{variable}'",
                    ValidationErrorType.InvalidVariable));
            }

            // Validate variable references in each request
            foreach (HttpRequest request in httpFile.Requests)
            {
                Dictionary<string, List<string>> unresolvedVariables = VariableProcessor.ValidateRequestVariables(
                    request,
                    httpFile.FileVariables,
                    environmentVariables);

                foreach (KeyValuePair<string, List<string>> kvp in unresolvedVariables)
                {
                    foreach (string variable in kvp.Value)
                    {
                        warnings.Add(new ValidationWarning(
                            request.LineNumber,
                            $"Unresolved variable '{variable}' in {kvp.Key}"));
                    }
                }
            }

            var result = new ValidationResult(errors, warnings);

            if (result.HasErrors)
            {
                _logger?.LogWarning("Variable validation found {ErrorCount} errors and {WarningCount} warnings",
                    result.Errors.Count, result.Warnings.Count);
            }
            else if (result.HasWarnings)
            {
                _logger?.LogInformation("Variable validation found {WarningCount} warnings",
                    result.Warnings.Count);
            }

            return result;
        }

        /// <summary>
        /// Gets a specific request by name from an HTTP file
        /// </summary>
        /// <param name="httpFile">The HTTP file</param>
        /// <param name="requestName">The request name to find</param>
        /// <param name="environmentVariables">Environment variables for processing</param>
        /// <returns>The processed request, or null if not found</returns>
        public HttpRequest? GetProcessedRequest(
            HttpFile httpFile,
            string requestName,
            IDictionary<string, string>? environmentVariables = null)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            if (string.IsNullOrWhiteSpace(requestName))
            {
                throw new ArgumentException("Request name cannot be null or empty", nameof(requestName));
            }

            if (!httpFile.TryGetRequestByName(requestName, out HttpRequest? request))
            {
                _logger?.LogWarning("Request with name '{RequestName}' not found", requestName);
                return null;
            }

            _logger?.LogDebug("Processing request '{RequestName}'", requestName);

            return VariableProcessor.ProcessRequest(request!, httpFile.FileVariables, environmentVariables);
        }

        /// <summary>
        /// Gets all requests from an HTTP file, processed with variable resolution
        /// </summary>
        /// <param name="httpFile">The HTTP file</param>
        /// <param name="environmentVariables">Environment variables for processing</param>
        /// <returns>List of processed requests</returns>
        public List<HttpRequest> GetAllProcessedRequests(
            HttpFile httpFile,
            IDictionary<string, string>? environmentVariables = null)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            var processedRequests = new List<HttpRequest>();

            foreach (HttpRequest request in httpFile.Requests)
            {
                HttpRequest processedRequest = VariableProcessor.ProcessRequest(request, httpFile.FileVariables, environmentVariables);
                processedRequests.Add(processedRequest);
            }

            _logger?.LogDebug("Processed {RequestCount} requests", processedRequests.Count);

            return processedRequests;
        }

        /// <summary>
        /// Validates an HTTP file for syntax and semantic correctness
        /// </summary>
        /// <param name="httpFile">The HTTP file to validate</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidateHttpFile(HttpFile httpFile)
        {
            ArgumentNullException.ThrowIfNull(httpFile);

            _logger?.LogInformation("Validating HTTP file with {RequestCount} requests", httpFile.Requests.Count);

            ValidationResult result = _validator.Validate(httpFile);

            if (result.HasErrors)
            {
                _logger?.LogWarning("Validation found {ErrorCount} errors and {WarningCount} warnings",
                    result.Errors.Count, result.Warnings.Count);
            }
            else if (result.HasWarnings)
            {
                _logger?.LogInformation("Validation found {WarningCount} warnings", result.Warnings.Count);
            }
            else
            {
                _logger?.LogInformation("Validation completed successfully");
            }

            return result;
        }
    }
}
