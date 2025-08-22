using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RESTClient.NET.Core.Models;

namespace RESTClient.NET.Core.Parsing
{
    /// <summary>
    /// Default implementation of HTTP syntax parser
    /// </summary>
    public partial class HttpSyntaxParser : IHttpSyntaxParser
    {
        private static readonly Regex _variableDefinitionRegex = VariableDefinitionRegex();
        private static readonly Regex _metadataRegex = new Regex(@"^(?:#|\/{2})\s*@([\w-]+)(?:\s+(.*?))?\s*$", RegexOptions.Compiled);

        /// <inheritdoc />
        public HttpFile Parse(IEnumerable<HttpToken> tokens, HttpParseOptions? options = null)
        {
            options ??= HttpParseOptions.Default();

            var tokenList = tokens.ToList();
            var requests = new List<HttpRequest>();
            var fileVariables = new Dictionary<string, string>();
            var requestNamePositions = new Dictionary<string, int>();

            string currentRequestName = string.Empty;
            var currentRequestTokens = new List<HttpToken>();
            var currentMetadata = new HttpRequestMetadata();
            bool isParsingRequest = false;

            foreach (HttpToken? token in tokenList)
            {
                switch (token.Type)
                {
                    case HttpTokenType.Variable:
                        ParseFileVariable(token.Value, fileVariables);
                        break;

                    case HttpTokenType.RequestSeparator:
                        // Finish previous request if any
                        if (isParsingRequest && !string.IsNullOrEmpty(currentRequestName))
                        {
                            HttpRequest? request = ParseRequest(currentRequestName, currentRequestTokens, currentMetadata);
                            if (request != null)
                            {
                                requests.Add(request);
                            }
                        }

                        // Extract request name from ### <name> pattern
                        string separatorLine = token.Value.Trim();
                        string[] separatorParts = separatorLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                        if (separatorParts.Length > 1)
                        {
                            currentRequestName = string.Join("-", separatorParts.Skip(1)).Trim();
                        }
                        else
                        {
                            // Generate a name if none provided
                            currentRequestName = $"request-{requests.Count + 1}";
                        }

                        // Validate duplicate names if enabled
                        if (options.ValidateRequestNames && !string.IsNullOrEmpty(currentRequestName))
                        {
                            if (!requestNamePositions.TryGetValue(currentRequestName, out int existingLine))
                            {
                                requestNamePositions[currentRequestName] = token.LineNumber;
                            }
                            else
                            {
                                throw new Exceptions.DuplicateRequestNameException(
                                    currentRequestName,
                                    existingLine,
                                    token.LineNumber);
                            }
                        }

                        currentRequestTokens.Clear();
                        currentMetadata = new HttpRequestMetadata();
                        isParsingRequest = true;
                        break;

                    case HttpTokenType.Metadata:
                        ParseMetadata(token.Value, currentMetadata);

                        // Check if this is a @name declaration - start of new request
                        Match nameMatch = _metadataRegex.Match(token.Value);
                        if (nameMatch.Success && string.Equals(nameMatch.Groups[1].Value, "name", StringComparison.OrdinalIgnoreCase))
                        {
                            // Finish previous request if any
                            if (isParsingRequest && !string.IsNullOrEmpty(currentRequestName))
                            {
                                HttpRequest? request = ParseRequest(currentRequestName, currentRequestTokens, currentMetadata);
                                if (request != null)
                                {
                                    requests.Add(request);
                                }
                            }

                            // Start new request
                            currentRequestName = nameMatch.Groups[2].Value.Trim();

                            // Validate duplicate names if enabled
                            if (options.ValidateRequestNames && !string.IsNullOrEmpty(currentRequestName))
                            {
                                if (!requestNamePositions.TryGetValue(currentRequestName, out int existingLineNumber))
                                {
                                    requestNamePositions[currentRequestName] = token.LineNumber;
                                }
                                else
                                {
                                    throw new Exceptions.DuplicateRequestNameException(
                                        currentRequestName,
                                        existingLineNumber,
                                        token.LineNumber);
                                }
                            }

                            currentRequestTokens.Clear();
                            currentMetadata = new HttpRequestMetadata();
                            ParseMetadata(token.Value, currentMetadata); // Re-parse to capture the name
                            isParsingRequest = true;
                        }
                        break;

                    case HttpTokenType.Method:
                    case HttpTokenType.Url:
                    case HttpTokenType.HttpVersion:
                    case HttpTokenType.HeaderName:
                    case HttpTokenType.HeaderValue:
                    case HttpTokenType.Body:
                    case HttpTokenType.FileBody:
                    case HttpTokenType.FileBodyWithVariables:
                    case HttpTokenType.FileBodyWithEncoding:
                    case HttpTokenType.RequestName:
                    case HttpTokenType.VariableReference:
                        if (isParsingRequest)
                        {
                            currentRequestTokens.Add(token);
                        }
                        break;

                    case HttpTokenType.Comment:
                    case HttpTokenType.LineBreak:
                    case HttpTokenType.Whitespace:
                        // Add to current request if we're parsing one
                        if (isParsingRequest)
                        {
                            currentRequestTokens.Add(token);
                        }
                        break;

                    case HttpTokenType.EndOfFile:
                        // Finish last request if any
                        if (isParsingRequest && !string.IsNullOrEmpty(currentRequestName))
                        {
                            HttpRequest? request = ParseRequest(currentRequestName, currentRequestTokens, currentMetadata);
                            if (request != null)
                            {
                                requests.Add(request);
                            }
                        }
                        break;

                    default:
                        // No action needed for unhandled token types
                        break;
                }
            }

            return new HttpFile(requests, fileVariables);
        }

        private static void ParseFileVariable(string variableDefinition, Dictionary<string, string> fileVariables)
        {
            Match match = _variableDefinitionRegex.Match(variableDefinition);
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                fileVariables[name] = value;
            }
        }

        private static void ParseMetadata(string metadataLine, HttpRequestMetadata metadata)
        {
            Match match = _metadataRegex.Match(metadataLine);
            if (!match.Success)
            {
                return;
            }

            string key = match.Groups[1].Value.ToLowerInvariant();
            string value = match.Groups[2].Value;

            switch (key)
            {
                case "name":
                    metadata.Name = value;
                    break;

                case "note":
                    metadata.Note = value;
                    break;

                case "no-redirect":
                    metadata.NoRedirect = true;
                    break;

                case "no-cookie-jar":
                    metadata.NoCookieJar = true;
                    break;

                case "expect":
                    ParseExpectation(value, metadata);
                    break;

                // Handle new expectation format
                case "expect-status-code":
                    metadata.AddExpectation(new TestExpectation(ExpectationType.StatusCode, value));
                    break;
                case "expect-header":
                    metadata.AddExpectation(new TestExpectation(ExpectationType.Header, value));
                    break;
                case "expect-body-contains":
                    metadata.AddExpectation(new TestExpectation(ExpectationType.BodyContains, value));
                    break;
                case "expect-body-path":
                    metadata.AddExpectation(new TestExpectation(ExpectationType.BodyPath, value));
                    break;
                case "expect-schema":
                    metadata.AddExpectation(new TestExpectation(ExpectationType.Schema, value));
                    break;
                case "expect-max-time":
                    metadata.AddExpectation(new TestExpectation(ExpectationType.MaxTime, value));
                    break;

                default:
                    metadata.CustomMetadata[key] = value;
                    break;
            }
        }

        private static void ParseExpectation(string expectationValue, HttpRequestMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(expectationValue))
            {
                return;
            }

            // Support both formats: "status 200" and "status: 200"
            string[] parts = expectationValue.Contains(':')
                ? expectationValue.Split([':'], 2, StringSplitOptions.RemoveEmptyEntries)
                : expectationValue.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                return;
            }

            string expectationType = parts[0].Trim().ToLowerInvariant();
            string expectedValue = parts[1].Trim();

            ExpectationType? type = expectationType switch
            {
                "status" => ExpectationType.StatusCode,
                "header" => ExpectationType.Header,
                "body-contains" => ExpectationType.BodyContains,
                "body-path" => ExpectationType.BodyPath,
                "schema" => ExpectationType.Schema,
                "max-time" => ExpectationType.MaxTime,
                _ => null
            };

            if (type.HasValue)
            {
                metadata.AddExpectation(new TestExpectation(type.Value, expectedValue));
            }
        }

        private static HttpRequest? ParseRequest(string requestName, List<HttpToken> tokens, HttpRequestMetadata metadata)
        {
            if (string.IsNullOrEmpty(requestName))
            {
                return null;
            }

            string method = "GET";
            string url = string.Empty;
            var headers = new Dictionary<string, string>();
            var bodyLines = new List<string>();
            var fileBodyReference = (FileBodyReference?)null;
            bool isInBody = false;
            int lineNumber = tokens.FirstOrDefault()?.LineNumber ?? 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                HttpToken token = tokens[i];

                switch (token.Type)
                {
                    case HttpTokenType.Method:
                        if (!isInBody)
                        {
                            method = token.Value.ToUpperInvariant();
                        }
                        else
                        {
                            bodyLines.Add(token.Value);
                        }
                        break;

                    case HttpTokenType.Url:
                        if (!isInBody)
                        {
                            url = token.Value;
                        }
                        else
                        {
                            bodyLines.Add(token.Value);
                        }
                        break;

                    case HttpTokenType.HeaderName:
                        if (!isInBody && i + 1 < tokens.Count && tokens[i + 1].Type == HttpTokenType.HeaderValue)
                        {
                            headers[token.Value] = tokens[i + 1].Value;
                            i++; // Skip the next token since we've processed it
                        }
                        else if (isInBody)
                        {
                            bodyLines.Add(token.Value);
                        }
                        break;

                    case HttpTokenType.Body:
                        isInBody = true;
                        bodyLines.Add(token.Value);
                        break;

                    case HttpTokenType.FileBody:
                        isInBody = true;
                        fileBodyReference = FileBodyReference.Raw(token.Value, token.LineNumber);
                        break;

                    case HttpTokenType.FileBodyWithVariables:
                        isInBody = true;
                        fileBodyReference = FileBodyReference.WithVariables(token.Value, token.LineNumber);
                        break;

                    case HttpTokenType.FileBodyWithEncoding:
                        isInBody = true;
                        string[] parts = token.Value.Split(['|'], 2);
                        string encodingName = parts.Length > 0 ? parts[0] : "utf-8";
                        string filePath = parts.Length > 1 ? parts[1] : token.Value;

                        try
                        {
                            Encoding encoding = GetEncodingByName(encodingName);
                            fileBodyReference = FileBodyReference.WithVariablesAndEncoding(filePath, encoding, token.LineNumber);
                        }
                        catch (ArgumentException)
                        {
                            // If encoding is not recognized, fall back to UTF-8
                            fileBodyReference = FileBodyReference.WithVariables(filePath, token.LineNumber);
                        }
                        break;

                    case HttpTokenType.LineBreak:
                        if (isInBody)
                        {
                            bodyLines.Add(string.Empty);
                        }
                        else
                        {
                            // Check if this is the transition to body
                            HttpToken? nextNonWhitespaceToken = tokens.Skip(i + 1)
                                .FirstOrDefault(t => t.Type is not HttpTokenType.Whitespace and not HttpTokenType.LineBreak);

                            if (nextNonWhitespaceToken?.Type == HttpTokenType.Body ||
                                (nextNonWhitespaceToken != null &&
                                 nextNonWhitespaceToken.Type != HttpTokenType.HeaderName &&
                                 nextNonWhitespaceToken.Type != HttpTokenType.Method &&
                                 nextNonWhitespaceToken.Type != HttpTokenType.Url))
                            {
                                isInBody = true;
                            }
                        }
                        break;

                    case HttpTokenType.HttpVersion:
                    case HttpTokenType.HeaderValue:
                    case HttpTokenType.Comment:
                    case HttpTokenType.Variable:
                    case HttpTokenType.VariableReference:
                    case HttpTokenType.Metadata:
                    case HttpTokenType.RequestSeparator:
                    case HttpTokenType.RequestName:
                    case HttpTokenType.Whitespace:
                    case HttpTokenType.EndOfFile:
                        // These tokens are handled in other contexts or can be ignored during request parsing
                        break;

                    default:
                        // No action needed for unhandled token types
                        break;
                }
            }

            string? body = bodyLines.Count > 0 ? string.Join(Environment.NewLine, bodyLines).Trim() : null;
            if (string.IsNullOrWhiteSpace(body))
            {
                body = null;
            }

            var request = new HttpRequest
            {
                Name = requestName,
                Method = method,
                Url = url,
                Body = body,
                FileBodyReference = fileBodyReference,
                LineNumber = lineNumber
            };

            // Add headers
            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers[header.Key] = header.Value;
            }

            // Set metadata
            foreach (TestExpectation expectation in metadata.Expectations)
            {
                request.Metadata.Expectations.Add(expectation);
            }

            return request;
        }

        /// <summary>
        /// Gets an encoding by name, supporting common encoding names used in VS Code REST Client
        /// </summary>
        /// <param name="encodingName">The encoding name (e.g., "utf8", "latin1", "ascii")</param>
        /// <returns>The corresponding Encoding instance</returns>
        /// <exception cref="ArgumentException">Thrown when the encoding name is not recognized</exception>
        private static Encoding GetEncodingByName(string encodingName)
        {
            string normalizedName = encodingName.ToLowerInvariant().Replace("-", "").Replace("_", "");

            switch (normalizedName)
            {
                case "utf8":
                case "utf-8":
                    return Encoding.UTF8;
                case "utf16":
                case "utf-16":
                    return Encoding.Unicode;
                case "utf32":
                case "utf-32":
                    return Encoding.UTF32;
                case "ascii":
                case "us-ascii":
                    return Encoding.ASCII;
                case "latin1":
                case "iso-8859-1":
                case "iso88591":
                    return Encoding.GetEncoding("ISO-8859-1");
                case "windows1252":
                case "cp1252":
                    return Encoding.GetEncoding("windows-1252");
                default:
                    throw new ArgumentException($"Unsupported encoding: {encodingName}", nameof(encodingName));
            }
        }

        [GeneratedRegex(@"^@([^\s=]+)\s*=\s*(.*?)\s*$", RegexOptions.Compiled)]
        private static partial Regex VariableDefinitionRegex();
    }
}
