using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RESTClient.NET.Core.Models;

namespace RESTClient.NET.Core.Parsing
{
    /// <summary>
    /// Default implementation of HTTP file tokenizer
    /// </summary>
    public class HttpTokenizer : IHttpTokenizer
    {
        private static readonly Regex _variableDefinitionRegex = new Regex(@"^@([^\s=]+)\s*=\s*(.*?)\s*$", RegexOptions.Compiled);
        private static readonly Regex _metadataCommentRegex = new Regex(@"^(?:#|\/{2})\s*@([\w-]+)(?:\s+(.*?))?\s*$", RegexOptions.Compiled);
        private static readonly Regex _commentRegex = new Regex(@"^(?:#|\/{2})(.*)$", RegexOptions.Compiled);
        private static readonly Regex _httpMethodRegex = new Regex(@"^(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|CONNECT|TRACE|LOCK|UNLOCK|PROPFIND|PROPPATCH|COPY|MOVE|MKCOL|MKCALENDAR|ACL|SEARCH)\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _fileBodyRegex = new Regex(@"^<(@\s*([a-zA-Z0-9-]+)?\s*)?(.+)$", RegexOptions.Compiled);

        /// <inheritdoc />
        public IEnumerable<HttpToken> Tokenize(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                yield return new HttpToken(HttpTokenType.EndOfFile, string.Empty, 1, 1);
                yield break;
            }

            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var isInBodySection = false;
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var lineNumber = i + 1;
                
                if (string.IsNullOrWhiteSpace(line))
                {
                    yield return new HttpToken(HttpTokenType.LineBreak, line, lineNumber, 1);
                    
                    // Check if this blank line indicates transition to body
                    if (!isInBodySection && i > 0)
                    {
                        // Look ahead to see if next non-empty line looks like body content
                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            var nextLine = lines[j].Trim();
                            if (!string.IsNullOrWhiteSpace(nextLine))
                            {
                                // If it starts with {, [, <@ (file body with variables), < (file body), or other body-like content, we're in body
                                if (nextLine.StartsWith("{") || nextLine.StartsWith("[") || 
                                    nextLine.StartsWith("<@") || nextLine.StartsWith("<") ||
                                    !nextLine.Contains(":"))
                                {
                                    isInBodySection = true;
                                }
                                break;
                            }
                        }
                    }
                    continue;
                }

                var trimmedLine = line.Trim();

                // Check for variable definition (@var = value)
                var variableMatch = _variableDefinitionRegex.Match(trimmedLine);
                if (variableMatch.Success)
                {
                    yield return new HttpToken(HttpTokenType.Variable, trimmedLine, lineNumber, 1);
                    continue;
                }

                // Check for request separator (###)
                if (trimmedLine.StartsWith("###"))
                {
                    isInBodySection = false; // Reset for new request
                    yield return new HttpToken(HttpTokenType.RequestSeparator, trimmedLine, lineNumber, 1);
                    continue;
                }

                // Check for metadata comment (# @name, # @expect, etc.)
                var metadataMatch = _metadataCommentRegex.Match(trimmedLine);
                if (metadataMatch.Success)
                {
                    isInBodySection = false; // Reset for new request
                    yield return new HttpToken(HttpTokenType.Metadata, trimmedLine, lineNumber, 1);
                    continue;
                }

                // Check for regular comment
                var commentMatch = _commentRegex.Match(trimmedLine);
                if (commentMatch.Success)
                {
                    yield return new HttpToken(HttpTokenType.Comment, trimmedLine, lineNumber, 1);
                    continue;
                }

                // Check for HTTP method at start of line
                var methodMatch = _httpMethodRegex.Match(trimmedLine);
                if (methodMatch.Success)
                {
                    isInBodySection = false; // Reset for new request
                    yield return new HttpToken(HttpTokenType.Method, methodMatch.Groups[1].Value, lineNumber, 1);
                    
                    // The rest of the line is the URL and possibly HTTP version
                    var remainder = trimmedLine.Substring(methodMatch.Length).Trim();
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        // Remove HTTP version from URL if present (e.g., "HTTP/1.1", "HTTP/2.0")
                        var httpVersionIndex = remainder.LastIndexOf(" HTTP/", StringComparison.OrdinalIgnoreCase);
                        if (httpVersionIndex > 0)
                        {
                            remainder = remainder.Substring(0, httpVersionIndex).Trim();
                        }
                        
                        if (!string.IsNullOrEmpty(remainder))
                        {
                            yield return new HttpToken(HttpTokenType.Url, remainder, lineNumber, methodMatch.Length + 1);
                        }
                    }
                    continue;
                }

                // If we're in body section, check for file body references first
                if (isInBodySection)
                {
                    // Check for file body reference (< filepath, <@ filepath, <@encoding filepath)
                    var fileBodyMatch = _fileBodyRegex.Match(trimmedLine);
                    if (fileBodyMatch.Success)
                    {
                        var atPart = fileBodyMatch.Groups[1].Value; // The "@..." part including optional encoding
                        var encodingPart = fileBodyMatch.Groups[2].Value; // The encoding part after @
                        var filePath = fileBodyMatch.Groups[3].Value.Trim(); // The file path
                        
                        if (string.IsNullOrEmpty(atPart))
                        {
                            // Raw file body: < filepath
                            yield return new HttpToken(HttpTokenType.FileBody, filePath, lineNumber, 1);
                        }
                        else if (string.IsNullOrEmpty(encodingPart))
                        {
                            // File body with variables: <@ filepath
                            yield return new HttpToken(HttpTokenType.FileBodyWithVariables, filePath, lineNumber, 1);
                        }
                        else
                        {
                            // File body with encoding: <@encoding filepath
                            yield return new HttpToken(HttpTokenType.FileBodyWithEncoding, $"{encodingPart}|{filePath}", lineNumber, 1);
                        }
                        continue;
                    }
                    
                    // Everything else is regular body content
                    yield return new HttpToken(HttpTokenType.Body, line, lineNumber, 1);
                    continue;
                }

                // Check if line contains a colon (likely a header) - only if not in body section
                var colonIndex = trimmedLine.IndexOf(':');
                if (colonIndex > 0 && colonIndex < trimmedLine.Length - 1)
                {
                    var headerName = trimmedLine.Substring(0, colonIndex).Trim();
                    var headerValue = trimmedLine.Substring(colonIndex + 1).Trim();
                    
                    yield return new HttpToken(HttpTokenType.HeaderName, headerName, lineNumber, 1);
                    yield return new HttpToken(HttpTokenType.HeaderValue, headerValue, lineNumber, colonIndex + 2);
                    continue;
                }

                // Check if it might be a URL (starts with http/https or is just a path)
                if (trimmedLine.StartsWith("http", StringComparison.OrdinalIgnoreCase) || 
                    trimmedLine.StartsWith("/") ||
                    trimmedLine.StartsWith("{{"))
                {
                    yield return new HttpToken(HttpTokenType.Url, trimmedLine, lineNumber, 1);
                    continue;
                }

                // Everything else is considered body content
                isInBodySection = true;
                yield return new HttpToken(HttpTokenType.Body, line, lineNumber, 1);
            }

            yield return new HttpToken(HttpTokenType.EndOfFile, string.Empty, lines.Length + 1, 1);
        }
    }
}
