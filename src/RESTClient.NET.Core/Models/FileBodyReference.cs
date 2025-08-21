using System;
using System.Text;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents a reference to an external file used as request body content
    /// </summary>
    /// <remarks>
    /// <para>FileBodyReference supports VS Code REST Client file body syntax:</para>
    /// <list type="bullet">
    /// <item><c>&lt; filepath</c> - Raw file content, no variable processing</item>
    /// <item><c>&lt;@ filepath</c> - File content with variable processing (UTF-8)</item>
    /// <item><c>&lt;@encoding filepath</c> - File content with variable processing and custom encoding</item>
    /// </list>
    /// <para>File paths can be absolute or relative to the workspace root or HTTP file location.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Raw file content
    /// var rawRef = new FileBodyReference("./data.json", false, null);
    ///
    /// // With variable processing
    /// var processedRef = new FileBodyReference("./template.xml", true, Encoding.UTF8);
    ///
    /// // With custom encoding
    /// var latinRef = new FileBodyReference("./data.txt", true, Encoding.Latin1);
    /// </code>
    /// </example>
    public class FileBodyReference
    {
        /// <summary>
        /// Gets the file path for the body content
        /// </summary>
        /// <remarks>
        /// Path can be absolute or relative. Relative paths are resolved relative to:
        /// <list type="number">
        /// <item>The directory containing the HTTP file</item>
        /// <item>The workspace root directory</item>
        /// </list>
        /// </remarks>
        public string FilePath { get; }

        /// <summary>
        /// Gets a value indicating whether variables should be processed in the file content
        /// </summary>
        /// <remarks>
        /// When true, variable references like {{baseUrl}} in the file content will be resolved.
        /// When false, the file content is used as-is without any processing.
        /// </remarks>
        public bool ProcessVariables { get; }

        /// <summary>
        /// Gets the encoding to use when reading the file
        /// </summary>
        /// <remarks>
        /// If null, UTF-8 encoding will be used as the default.
        /// Common encodings include UTF-8, UTF-16, ASCII, and Latin1.
        /// </remarks>
        public Encoding? Encoding { get; }

        /// <summary>
        /// Gets the original line where this file body reference was defined
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the FileBodyReference class
        /// </summary>
        /// <param name="filePath">The file path for the body content</param>
        /// <param name="processVariables">Whether to process variables in the file content</param>
        /// <param name="encoding">The encoding to use when reading the file (null for UTF-8)</param>
        /// <param name="lineNumber">The line number where this reference was defined</param>
        /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace</exception>
        public FileBodyReference(string filePath, bool processVariables, Encoding? encoding, int lineNumber = 0)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or whitespace", nameof(filePath));
            }

            FilePath = filePath.Trim();
            ProcessVariables = processVariables;
            Encoding = encoding;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Creates a FileBodyReference for raw file content without variable processing
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="lineNumber">The line number where this reference was defined</param>
        /// <returns>A new FileBodyReference instance</returns>
        public static FileBodyReference Raw(string filePath, int lineNumber = 0)
        {
            return new FileBodyReference(filePath, false, null, lineNumber);
        }

        /// <summary>
        /// Creates a FileBodyReference with variable processing using UTF-8 encoding
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="lineNumber">The line number where this reference was defined</param>
        /// <returns>A new FileBodyReference instance</returns>
        public static FileBodyReference WithVariables(string filePath, int lineNumber = 0)
        {
            return new FileBodyReference(filePath, true, Encoding.UTF8, lineNumber);
        }

        /// <summary>
        /// Creates a FileBodyReference with variable processing using custom encoding
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="encoding">The encoding to use</param>
        /// <param name="lineNumber">The line number where this reference was defined</param>
        /// <returns>A new FileBodyReference instance</returns>
        public static FileBodyReference WithVariablesAndEncoding(string filePath, Encoding encoding, int lineNumber = 0)
        {
            return new FileBodyReference(filePath, true, encoding, lineNumber);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = ProcessVariables ? (Encoding?.EncodingName != "Unicode (UTF-8)" && Encoding != null ? $"<@{Encoding.WebName}" : "<@") : "<";
            return $"{prefix} {FilePath}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is FileBodyReference other &&
                   FilePath == other.FilePath &&
                   ProcessVariables == other.ProcessVariables &&
                   Encoding?.WebName == other.Encoding?.WebName;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (FilePath?.GetHashCode() ?? 0);
                hash = hash * 23 + ProcessVariables.GetHashCode();
                hash = hash * 23 + (Encoding?.WebName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
