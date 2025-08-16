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
        private static readonly Regex VariableDefinitionRegex = new Regex(@"^@([^\s=]+)\s*=\s*(.*?)\s*$", RegexOptions.Compiled);
        private static readonly Regex MetadataCommentRegex = new Regex(@"^(?:#|\/{2})\s*@([\w-]+)(?:\s+(.*?))?\s*$", RegexOptions.Compiled);
        private static readonly Regex CommentRegex = new Regex(@"^(?:#|\/{2})(.*)$", RegexOptions.Compiled);
        private static readonly Regex HttpMethodRegex = new Regex(@"^(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|CONNECT|TRACE|LOCK|UNLOCK|PROPFIND|PROPPATCH|COPY|MOVE|MKCOL|MKCALENDAR|ACL|SEARCH)\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                                // If it starts with {, [, or other body-like content, we're in body
                                if (nextLine.StartsWith("{") || nextLine.StartsWith("[") || 
                                    nextLine.StartsWith("<") || !nextLine.Contains(":"))
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
                var variableMatch = VariableDefinitionRegex.Match(trimmedLine);
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
                var metadataMatch = MetadataCommentRegex.Match(trimmedLine);
                if (metadataMatch.Success)
                {
                    isInBodySection = false; // Reset for new request
                    yield return new HttpToken(HttpTokenType.Metadata, trimmedLine, lineNumber, 1);
                    continue;
                }

                // Check for regular comment
                var commentMatch = CommentRegex.Match(trimmedLine);
                if (commentMatch.Success)
                {
                    yield return new HttpToken(HttpTokenType.Comment, trimmedLine, lineNumber, 1);
                    continue;
                }

                // Check for HTTP method at start of line
                var methodMatch = HttpMethodRegex.Match(trimmedLine);
                if (methodMatch.Success)
                {
                    isInBodySection = false; // Reset for new request
                    yield return new HttpToken(HttpTokenType.Method, methodMatch.Groups[1].Value, lineNumber, 1);
                    
                    // The rest of the line is likely the URL
                    var remainder = trimmedLine.Substring(methodMatch.Length).Trim();
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        yield return new HttpToken(HttpTokenType.Url, remainder, lineNumber, methodMatch.Length + 1);
                    }
                    continue;
                }

                // If we're in body section, everything is body content
                if (isInBodySection)
                {
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
