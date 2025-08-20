using System;
using System.Linq;
using System.Text.RegularExpressions;
using RESTClient.NET.Core.Models;

namespace RESTClient.NET.Core.Processing
{
    /// <summary>
    /// Processes response variables that reference previous request responses
    /// Supports syntax like {{requestName.response.body.$.token}} and {{requestName.response.header.Authorization}}
    /// </summary>
    public static class ResponseVariableProcessor
    {
        // Matches {{requestName.response.body.$.jsonPath}} pattern
        private static readonly Regex _responseBodyJsonPathRegex = new Regex(
            @"\{\{([a-zA-Z0-9_-]+)\.response\.body\.\$\.([^}]+)\}\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches {{requestName.response.body}} pattern (full body)
        private static readonly Regex _responseBodyRegex = new Regex(
            @"\{\{([a-zA-Z0-9_-]+)\.response\.body\}\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches {{requestName.response.header.HeaderName}} pattern
        private static readonly Regex _responseHeaderRegex = new Regex(
            @"\{\{([a-zA-Z0-9_-]+)\.response\.header\.([^}]+)\}\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches {{requestName.response.status}} pattern
        private static readonly Regex _responseStatusRegex = new Regex(
            @"\{\{([a-zA-Z0-9_-]+)\.response\.status\}\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches {{requestName.response.contentType}} pattern
        private static readonly Regex _responseContentTypeRegex = new Regex(
            @"\{\{([a-zA-Z0-9_-]+)\.response\.contentType\}\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Matches {{requestName.response.responseTime}} pattern
        private static readonly Regex _responseTimeRegex = new Regex(
            @"\{\{([a-zA-Z0-9_-]+)\.response\.responseTime\}\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Resolves all response variables in the given content
        /// </summary>
        /// <param name="content">Content containing response variable references</param>
        /// <param name="responseContext">Context containing stored response data</param>
        /// <returns>Content with response variables resolved</returns>
        public static string? ResolveResponseVariables(string? content, ResponseContext? responseContext)
        {
            if (string.IsNullOrEmpty(content) || responseContext == null)
                return content;

            var result = content;

            // Process JSONPath body references: {{requestName.response.body.$.path}}
            result = _responseBodyJsonPathRegex.Replace(result, match =>
            {
                var requestName = match.Groups[1].Value;
                var jsonPath = match.Groups[2].Value;
                
                var response = responseContext.GetResponse(requestName);
                if (response == null)
                    return match.Value; // Keep original if response not found

                var value = response.GetJsonPathValue($"$.{jsonPath}");
                return value ?? match.Value; // Keep original if value not found
            });

            // Process full body references: {{requestName.response.body}}
            result = _responseBodyRegex.Replace(result, match =>
            {
                var requestName = match.Groups[1].Value;
                
                var response = responseContext.GetResponse(requestName);
                if (response == null)
                    return match.Value; // Keep original if response not found

                return response.BodyContent ?? match.Value; // Keep original if body is null
            });

            // Process header references: {{requestName.response.header.HeaderName}}
            result = _responseHeaderRegex.Replace(result, match =>
            {
                var requestName = match.Groups[1].Value;
                var headerName = match.Groups[2].Value;
                
                var response = responseContext.GetResponse(requestName);
                if (response == null)
                    return match.Value; // Keep original if response not found

                var headerValue = response.GetHeaderValue(headerName);
                return headerValue ?? match.Value; // Keep original if header not found
            });

            // Process status code references: {{requestName.response.status}}
            result = _responseStatusRegex.Replace(result, match =>
            {
                var requestName = match.Groups[1].Value;
                
                var response = responseContext.GetResponse(requestName);
                if (response == null)
                    return match.Value; // Keep original if response not found

                return ((int)response.StatusCode).ToString();
            });

            // Process content type references: {{requestName.response.contentType}}
            result = _responseContentTypeRegex.Replace(result, match =>
            {
                var requestName = match.Groups[1].Value;
                
                var response = responseContext.GetResponse(requestName);
                if (response == null)
                    return match.Value; // Keep original if response not found

                return response.ContentType ?? match.Value; // Keep original if content type is null
            });

            // Process response time references: {{requestName.response.responseTime}}
            result = _responseTimeRegex.Replace(result, match =>
            {
                var requestName = match.Groups[1].Value;
                
                var response = responseContext.GetResponse(requestName);
                if (response == null)
                    return match.Value; // Keep original if response not found

                return response.ResponseTimeMs.ToString("F2");
            });

            return result;
        }

        /// <summary>
        /// Checks if content contains any response variable references
        /// </summary>
        /// <param name="content">Content to check</param>
        /// <returns>True if response variables are found, false otherwise</returns>
        public static bool ContainsResponseVariables(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            return _responseBodyJsonPathRegex.IsMatch(content) ||
                   _responseBodyRegex.IsMatch(content) ||
                   _responseHeaderRegex.IsMatch(content) ||
                   _responseStatusRegex.IsMatch(content) ||
                   _responseContentTypeRegex.IsMatch(content) ||
                   _responseTimeRegex.IsMatch(content);
        }

        /// <summary>
        /// Extracts all response variable references from content
        /// </summary>
        /// <param name="content">Content to analyze</param>
        /// <returns>Array of referenced request names</returns>
        public static string[] ExtractReferencedRequests(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return Array.Empty<string>();

            var requestNames = new System.Collections.Generic.HashSet<string>();

            // Extract from JSONPath body references
            var jsonPathMatches = _responseBodyJsonPathRegex.Matches(content);
            foreach (Match match in jsonPathMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from full body references
            var bodyMatches = _responseBodyRegex.Matches(content);
            foreach (Match match in bodyMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from header references
            var headerMatches = _responseHeaderRegex.Matches(content);
            foreach (Match match in headerMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from status references
            var statusMatches = _responseStatusRegex.Matches(content);
            foreach (Match match in statusMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from content type references
            var contentTypeMatches = _responseContentTypeRegex.Matches(content);
            foreach (Match match in contentTypeMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from response time references
            var responseTimeMatches = _responseTimeRegex.Matches(content);
            foreach (Match match in responseTimeMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            return requestNames.ToArray();
        }
    }
}
