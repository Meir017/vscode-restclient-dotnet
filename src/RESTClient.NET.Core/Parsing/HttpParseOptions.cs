using System;

namespace RESTClient.NET.Core.Parsing
{
    /// <summary>
    /// Options for HTTP file parsing
    /// </summary>
    public class HttpParseOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to validate request names for uniqueness and format
        /// </summary>
        public bool ValidateRequestNames { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to process variable references
        /// </summary>
        public bool ProcessVariables { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use strict parsing mode
        /// </summary>
        public bool StrictMode { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to parse test expectations
        /// </summary>
        public bool ParseExpectations { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to require request names for all requests
        /// </summary>
        public bool RequireRequestNames { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to allow empty request bodies
        /// </summary>
        public bool AllowEmptyBodies { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to normalize line endings
        /// </summary>
        public bool NormalizeLineEndings { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum allowed request name length
        /// </summary>
        public int MaxRequestNameLength { get; set; } = 50;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore unknown metadata comments
        /// </summary>
        public bool IgnoreUnknownMetadata { get; set; } = true;

        /// <summary>
        /// Creates default parsing options
        /// </summary>
        /// <returns>Default parsing options</returns>
        public static HttpParseOptions Default()
        {
            return new HttpParseOptions();
        }

        /// <summary>
        /// Creates strict parsing options
        /// </summary>
        /// <returns>Strict parsing options</returns>
        public static HttpParseOptions Strict()
        {
            return new HttpParseOptions
            {
                StrictMode = true,
                RequireRequestNames = true,
                IgnoreUnknownMetadata = false,
                AllowEmptyBodies = false
            };
        }

        /// <summary>
        /// Creates lenient parsing options
        /// </summary>
        /// <returns>Lenient parsing options</returns>
        public static HttpParseOptions Lenient()
        {
            return new HttpParseOptions
            {
                ValidateRequestNames = false,
                RequireRequestNames = false,
                StrictMode = false,
                IgnoreUnknownMetadata = true
            };
        }
    }
}
