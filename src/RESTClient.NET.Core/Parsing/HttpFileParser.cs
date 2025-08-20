using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Validation;

namespace RESTClient.NET.Core.Parsing
{
    /// <summary>
    /// Main entry point for parsing HTTP files into structured objects.
    /// Provides comprehensive parsing of VS Code REST Client (.http) files with enhanced metadata support.
    /// </summary>
    /// <remarks>
    /// <para>The HttpFileParser orchestrates the parsing pipeline:</para>
    /// <list type="number">
    /// <item>Tokenization: Breaks content into structured tokens</item>
    /// <item>Syntax parsing: Applies VS Code REST Client syntax rules</item>
    /// <item>Validation: Ensures request names are unique and valid</item>
    /// <item>Processing: Resolves variables and metadata</item>
    /// </list>
    /// <para>Supports enhanced metadata comments like <c># @name</c>, <c># @expect-status</c>, and <c># @expect-header</c>.</para>
    /// <para>Compatible with traditional <c>###</c> separators and modern <c># @name</c> identification.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var parser = new HttpFileParser();
    /// 
    /// var content = @"
    /// @baseUrl = https://api.example.com
    /// 
    /// # @name get-users
    /// # @expect status 200
    /// GET {{baseUrl}}/users HTTP/1.1
    /// Authorization: Bearer {{token}}
    /// 
    /// # @name create-user
    /// # @expect status 201
    /// POST {{baseUrl}}/users HTTP/1.1
    /// Content-Type: application/json
    /// 
    /// {""name"": ""John Doe""}";
    /// 
    /// var httpFile = parser.Parse(content);
    /// var getUsersRequest = httpFile.GetRequestByName("get-users");
    /// Console.WriteLine($"Method: {getUsersRequest.Method}");
    /// Console.WriteLine($"URL: {getUsersRequest.Url}");
    /// </code>
    /// </example>
    public class HttpFileParser
    {
        private readonly IHttpTokenizer _tokenizer;
        private readonly IHttpSyntaxParser _syntaxParser;
        private readonly IHttpFileValidator _validator;
        private readonly ILogger<HttpFileParser>? _logger;

        /// <summary>
        /// Initializes a new instance of the HttpFileParser class
        /// </summary>
        /// <param name="tokenizer">The tokenizer to use</param>
        /// <param name="syntaxParser">The syntax parser to use</param>
        /// <param name="validator">The validator to use</param>
        /// <param name="logger">Optional logger</param>
        public HttpFileParser(
            IHttpTokenizer? tokenizer = null,
            IHttpSyntaxParser? syntaxParser = null,
            IHttpFileValidator? validator = null,
            ILogger<HttpFileParser>? logger = null)
        {
            _tokenizer = tokenizer ?? new HttpTokenizer();
            _syntaxParser = syntaxParser ?? new HttpSyntaxParser();
            _validator = validator ?? new HttpFileValidator();
            _logger = logger;
        }

        /// <summary>
        /// Parses HTTP file content
        /// </summary>
        /// <param name="content">The HTTP file content</param>
        /// <param name="options">Parsing options</param>
        /// <returns>The parsed HTTP file</returns>
        public HttpFile Parse(string content, HttpParseOptions? options = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            options ??= HttpParseOptions.Default();

            try
            {
                _logger?.LogDebug("Starting HTTP file parsing");

                // Tokenize the content
                var tokens = _tokenizer.Tokenize(content);
                _logger?.LogDebug("Tokenization completed");

                // Parse the tokens into an HTTP file
                var httpFile = _syntaxParser.Parse(tokens, options);
                _logger?.LogDebug("Syntax parsing completed. Found {RequestCount} requests", httpFile.Requests.Count);

                // Validate the result if requested
                if (options.ValidateRequestNames || options.StrictMode)
                {
                    var validationResult = _validator.Validate(httpFile);
                    if (!validationResult.IsValid)
                    {
                        _logger?.LogWarning("Validation failed with {ErrorCount} errors", validationResult.Errors.Count);

                        if (options.StrictMode)
                        {
                            var firstError = validationResult.Errors[0];
                            throw new Exceptions.HttpParseException(
                                $"Validation failed: {firstError.Message}",
                                firstError.LineNumber);
                        }
                    }
                }

                _logger?.LogInformation("HTTP file parsing completed successfully");
                return httpFile;
            }
            catch (Exception ex) when (!(ex is Exceptions.HttpParseException))
            {
                _logger?.LogError(ex, "Unexpected error during HTTP file parsing");
                throw new Exceptions.HttpParseException("An unexpected error occurred during parsing", 0, 0, null, ex);
            }
        }

        /// <summary>
        /// Parses HTTP file content from a stream
        /// </summary>
        /// <param name="stream">The stream containing HTTP file content</param>
        /// <param name="options">Parsing options</param>
        /// <returns>The parsed HTTP file</returns>
        public HttpFile Parse(Stream stream, HttpParseOptions? options = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            return Parse(content, options);
        }

        /// <summary>
        /// Parses an HTTP file from a file path asynchronously
        /// </summary>
        /// <param name="filePath">Path to the HTTP file</param>
        /// <param name="options">Parsing options</param>
        /// <returns>The parsed HTTP file</returns>
        public async Task<HttpFile> ParseFileAsync(string filePath, HttpParseOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"HTTP file not found: {filePath}");

            _logger?.LogDebug("Reading HTTP file from: {FilePath}", filePath);

#if NETSTANDARD2_0
            var content = File.ReadAllText(filePath);
            return await Task.FromResult(Parse(content, options)).ConfigureAwait(false);
#else
            var content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            return Parse(content, options);
#endif
        }

        /// <summary>
        /// Parses HTTP file content asynchronously
        /// </summary>
        /// <param name="content">The HTTP file content</param>
        /// <param name="options">Parsing options</param>
        /// <returns>The parsed HTTP file</returns>
        public Task<HttpFile> ParseAsync(string content, HttpParseOptions? options = null)
        {
            return Task.FromResult(Parse(content, options));
        }

        /// <summary>
        /// Validates an HTTP file without parsing
        /// </summary>
        /// <param name="content">The HTTP file content</param>
        /// <param name="options">Parsing options</param>
        /// <returns>The validation result</returns>
        public ValidationResult Validate(string content, HttpParseOptions? options = null)
        {
            try
            {
                var httpFile = Parse(content, options);
                return _validator.Validate(httpFile);
            }
            catch (Exceptions.HttpParseException ex)
            {
                var error = new ValidationError(
                    ex.LineNumber,
                    ex.Message,
                    ValidationErrorType.InvalidHttpSyntax,
                    ex.ParsedContent);

                return ValidationResult.Failure(new[] { error });
            }
        }
    }
}
