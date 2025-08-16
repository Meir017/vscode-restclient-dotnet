using System;
using System.Collections.Generic;
using RESTClient.NET.Core.Models;

namespace RESTClient.NET.Core.Parsing
{
    /// <summary>
    /// Represents an HTTP token produced by the tokenizer
    /// </summary>
    public class HttpToken
    {
        /// <summary>
        /// Gets the token type
        /// </summary>
        public HttpTokenType Type { get; }

        /// <summary>
        /// Gets the token value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the line number where this token appears
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the column number where this token appears
        /// </summary>
        public int ColumnNumber { get; }

        /// <summary>
        /// Initializes a new instance of the HttpToken class
        /// </summary>
        /// <param name="type">The token type</param>
        /// <param name="value">The token value</param>
        /// <param name="lineNumber">The line number</param>
        /// <param name="columnNumber">The column number</param>
        public HttpToken(HttpTokenType type, string value, int lineNumber, int columnNumber)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Type}: {Value} ({LineNumber}:{ColumnNumber})";
        }
    }

    /// <summary>
    /// Defines the types of HTTP tokens
    /// </summary>
    public enum HttpTokenType
    {
        /// <summary>
        /// Request ID separator (###)
        /// </summary>
        RequestSeparator,

        /// <summary>
        /// Request name
        /// </summary>
        RequestName,

        /// <summary>
        /// Request ID
        /// </summary>
        [Obsolete("Use RequestName instead. This value will be removed in a future version.")]
        RequestId = RequestName,

        /// <summary>
        /// HTTP method
        /// </summary>
        Method,

        /// <summary>
        /// URL
        /// </summary>
        Url,

        /// <summary>
        /// HTTP version
        /// </summary>
        HttpVersion,

        /// <summary>
        /// Header name
        /// </summary>
        HeaderName,

        /// <summary>
        /// Header value
        /// </summary>
        HeaderValue,

        /// <summary>
        /// Request body content
        /// </summary>
        Body,

        /// <summary>
        /// Comment line
        /// </summary>
        Comment,

        /// <summary>
        /// Variable definition
        /// </summary>
        Variable,

        /// <summary>
        /// Variable reference
        /// </summary>
        VariableReference,

        /// <summary>
        /// Metadata comment (e.g., @name, @expect)
        /// </summary>
        Metadata,

        /// <summary>
        /// Line break
        /// </summary>
        LineBreak,

        /// <summary>
        /// Whitespace
        /// </summary>
        Whitespace,

        /// <summary>
        /// End of file
        /// </summary>
        EndOfFile
    }

    /// <summary>
    /// Interface for HTTP file tokenization
    /// </summary>
    public interface IHttpTokenizer
    {
        /// <summary>
        /// Tokenizes the HTTP file content
        /// </summary>
        /// <param name="content">The HTTP file content</param>
        /// <returns>A sequence of HTTP tokens</returns>
        IEnumerable<HttpToken> Tokenize(string content);
    }

    /// <summary>
    /// Interface for HTTP file syntax parsing
    /// </summary>
    public interface IHttpSyntaxParser
    {
        /// <summary>
        /// Parses HTTP tokens into an HttpFile
        /// </summary>
        /// <param name="tokens">The HTTP tokens</param>
        /// <param name="options">The parsing options</param>
        /// <returns>The parsed HTTP file</returns>
        HttpFile Parse(IEnumerable<HttpToken> tokens, HttpParseOptions? options = null);
    }
}
