using System.Collections.Generic;
using System.Text.RegularExpressions;
using RESTClient.NET.Core.Models;

namespace RESTClient.NET.Core.Processing
{
    /// <summary>
    /// Processes response variables that reference previous request responses
    /// Supports syntax like {{requestName.response.body.$.token}} and {{requestName.response.header.Authorization}}
    /// </summary>
    public static partial class ResponseVariableProcessor
    {
        // Matches {{requestName.response.body.$.jsonPath}} pattern
        private static readonly Regex _responseBodyJsonPathRegex = MyRegex();

        // Matches {{requestName.response.body}} pattern (full body)
        [GeneratedRegex(@"\{\{([a-zA-Z0-9_-]+)\.response\.body\}\}", RegexOptions.Compiled | RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex ResponseBodyRegex();
        private static readonly Regex _responseBodyRegex = ResponseBodyRegex();

        // Matches {{requestName.response.header.HeaderName}} pattern
        [GeneratedRegex(@"\{\{([a-zA-Z0-9_-]+)\.response\.header\.([^}]+)\}\}", RegexOptions.Compiled | RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex ResponseHeaderRegex();
        private static readonly Regex _responseHeaderRegex = ResponseHeaderRegex();

        // Matches {{requestName.response.status}} pattern
        [GeneratedRegex(@"\{\{([a-zA-Z0-9_-]+)\.response\.status\}\}", RegexOptions.Compiled | RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex ResponseStatusRegex();
        private static readonly Regex _responseStatusRegex = ResponseStatusRegex();

        // Matches {{requestName.response.contentType}} pattern
        [GeneratedRegex(@"\{\{([a-zA-Z0-9_-]+)\.response\.contentType\}\}", RegexOptions.Compiled | RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex ResponseContentTypeRegex();
        private static readonly Regex _responseContentTypeRegex = ResponseContentTypeRegex();

        // Matches {{requestName.response.responseTime}} pattern
        [GeneratedRegex(@"\{\{([a-zA-Z0-9_-]+)\.response\.responseTime\}\}", RegexOptions.Compiled | RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex ResponseTimeRegex();
        private static readonly Regex _responseTimeRegex = ResponseTimeRegex();

        /// <summary>
        /// Resolves all response variables in the given content
        /// </summary>
        /// <param name="content">Content containing response variable references</param>
        /// <param name="responseContext">Context containing stored response data</param>
        /// <returns>Content with response variables resolved</returns>
        public static string? ResolveResponseVariables(string? content, ResponseContext? responseContext)
        {
            if (string.IsNullOrEmpty(content) || responseContext == null)
            {
                return content;
            }

            string? result = content;

            // Process JSONPath body references: {{requestName.response.body.$.path}}
            result = _responseBodyJsonPathRegex.Replace(result, match =>
            {
                string requestName = match.Groups[1].Value;
                string jsonPath = match.Groups[2].Value;

                HttpResponseData? response = responseContext.GetResponse(requestName);
                if (response == null)
                {
                    return match.Value; // Keep original if response not found
                }

                string? value = response.GetJsonPathValue($"$.{jsonPath}");
                return value ?? match.Value; // Keep original if value not found
            });

            // Process full body references: {{requestName.response.body}}
            result = _responseBodyRegex.Replace(result, match =>
            {
                string requestName = match.Groups[1].Value;

                HttpResponseData? response = responseContext.GetResponse(requestName);
                if (response == null)
                {
                    return match.Value; // Keep original if response not found
                }

                return response.BodyContent ?? match.Value; // Keep original if body is null
            });

            // Process header references: {{requestName.response.header.HeaderName}}
            result = _responseHeaderRegex.Replace(result, match =>
            {
                string requestName = match.Groups[1].Value;
                string headerName = match.Groups[2].Value;

                HttpResponseData? response = responseContext.GetResponse(requestName);
                if (response == null)
                {
                    return match.Value; // Keep original if response not found
                }

                string? headerValue = response.GetHeaderValue(headerName);
                return headerValue ?? match.Value; // Keep original if header not found
            });

            // Process status code references: {{requestName.response.status}}
            result = _responseStatusRegex.Replace(result, match =>
            {
                string requestName = match.Groups[1].Value;

                HttpResponseData? response = responseContext.GetResponse(requestName);
                if (response == null)
                {
                    return match.Value; // Keep original if response not found
                }

                return ((int)response.StatusCode).ToString(System.Globalization.CultureInfo.InvariantCulture);
            });

            // Process content type references: {{requestName.response.contentType}}
            result = _responseContentTypeRegex.Replace(result, match =>
            {
                string requestName = match.Groups[1].Value;

                HttpResponseData? response = responseContext.GetResponse(requestName);
                if (response == null)
                {
                    return match.Value; // Keep original if response not found
                }

                return response.ContentType ?? match.Value; // Keep original if content type is null
            });

            // Process response time references: {{requestName.response.responseTime}}
            result = _responseTimeRegex.Replace(result, match =>
            {
                string requestName = match.Groups[1].Value;

                HttpResponseData? response = responseContext.GetResponse(requestName);
                if (response == null)
                {
                    return match.Value; // Keep original if response not found
                }

                return response.ResponseTimeMs.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
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
            return !string.IsNullOrEmpty(content) &&
                (
                    _responseBodyJsonPathRegex.IsMatch(content) ||
                   _responseBodyRegex.IsMatch(content) ||
                   _responseHeaderRegex.IsMatch(content) ||
                   _responseStatusRegex.IsMatch(content) ||
                   _responseContentTypeRegex.IsMatch(content) ||
                   _responseTimeRegex.IsMatch(content)
               );
        }

        /// <summary>
        /// Extracts all response variable references from content
        /// </summary>
        /// <param name="content">Content to analyze</param>
        /// <returns>Array of referenced request names</returns>
        public static string[] ExtractReferencedRequests(string? content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return [];
            }

            var requestNames = new HashSet<string>();

            // Extract from JSONPath body references
            MatchCollection jsonPathMatches = _responseBodyJsonPathRegex.Matches(content);
            foreach (Match match in jsonPathMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from full body references
            MatchCollection bodyMatches = _responseBodyRegex.Matches(content);
            foreach (Match match in bodyMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from header references
            MatchCollection headerMatches = _responseHeaderRegex.Matches(content);
            foreach (Match match in headerMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from status references
            MatchCollection statusMatches = _responseStatusRegex.Matches(content);
            foreach (Match match in statusMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from content type references
            MatchCollection contentTypeMatches = _responseContentTypeRegex.Matches(content);
            foreach (Match match in contentTypeMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            // Extract from response time references
            MatchCollection responseTimeMatches = _responseTimeRegex.Matches(content);
            foreach (Match match in responseTimeMatches)
            {
                requestNames.Add(match.Groups[1].Value);
            }

            return [.. requestNames];
        }

        [GeneratedRegex(@"\{\{([a-zA-Z0-9_-]+)\.response\.body\.\$\.([^}]+)\}\}", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex MyRegex();
    }
}
