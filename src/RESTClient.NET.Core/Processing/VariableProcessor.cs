using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RESTClient.NET.Core.Models;

namespace RESTClient.NET.Core.Processing
{
    /// <summary>
    /// Processes and resolves variables in HTTP files using a three-pass approach.
    /// </summary>
    /// <remarks>
    /// <para>Variable Resolution Order:</para>
    /// <list type="number">
    /// <item><description>File variables: <c>{{variable}}</c> defined as <c>@variable = value</c></description></item>
    /// <item><description>Environment variables: <c>${variable}</c> from environment or provided dictionary</description></item>
    /// <item><description>System variables: <c>{{$variable}}</c> for dynamic values like GUIDs and timestamps</description></item>
    /// </list>
    /// <para>Supported system variables:</para>
    /// <list type="bullet">
    /// <item><description><c>{{$guid}}</c> - RFC 4122 v4 UUID</description></item>
    /// <item><description><c>{{$randomInt min max}}</c> - Random integer</description></item>
    /// <item><description><c>{{$timestamp [offset option]}}</c> - UTC timestamp</description></item>
    /// <item><description><c>{{$datetime format [offset option]}}</c> - Formatted datetime</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var fileVariables = new Dictionary&lt;string, string&gt;
    /// {
    ///     ["baseUrl"] = "https://api.example.com",
    ///     ["version"] = "v1"
    /// };
    /// 
    /// var envVariables = new Dictionary&lt;string, string&gt;
    /// {
    ///     ["API_KEY"] = "secret-key"
    /// };
    /// 
    /// var content = "{{baseUrl}}/{{version}}/users?key=${API_KEY}&amp;id={{$guid}}";
    /// var resolved = VariableProcessor.ResolveVariables(content, fileVariables, envVariables);
    /// // Result: "https://api.example.com/v1/users?key=secret-key&amp;id=123e4567-e89b-12d3-a456-426614174000"
    /// </code>
    /// </example>
    public static class VariableProcessor
    {
        private static readonly Regex __variableReferenceRegex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);
        private static readonly Regex __environmentVariableRegex = new Regex(@"\$\{([^}]+)\}", RegexOptions.Compiled);

        /// <summary>
        /// Resolves variables in content using a three-pass approach:
        /// 1. File variables ({{variable}}) - environment variables override file variables when both exist
        /// 2. Environment variables (${variable})
        /// 3. System variables ({{$variable}})
        /// </summary>
        /// <param name="content">The content to process</param>
        /// <param name="fileVariables">File-level variables defined in the HTTP file</param>
        /// <param name="environmentVariables">Environment variables for resolution</param>
        /// <returns>Content with variables resolved, or the original content if null/empty</returns>
        /// <example>
        /// <code>
        /// var content = "GET {{baseUrl}}/api/users/{{userId}}";
        /// var fileVars = new Dictionary&lt;string, string&gt; { ["baseUrl"] = "https://api.com" };
        /// var envVars = new Dictionary&lt;string, string&gt; { ["userId"] = "123" };
        /// var result = VariableProcessor.ResolveVariables(content, fileVars, envVars);
        /// // Result: "GET https://api.com/api/users/123"
        /// </code>
        /// </example>
        public static string? ResolveVariables(
            string? content, 
            IReadOnlyDictionary<string, string>? fileVariables = null,
            IDictionary<string, string>? environmentVariables = null)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            var result = content;

            // First pass: resolve file variables ({{variable}})
            // Environment variables override file variables when both exist
            result = _variableReferenceRegex.Replace(result, match =>
            {
                var variableName = match.Groups[1].Value.Trim();
                
                // Check environment variables first (they override file variables)
                if (environmentVariables?.TryGetValue(variableName, out var envValue) == true)
                {
                    return envValue;
                }
                
                // Fall back to file variables
                if (fileVariables?.TryGetValue(variableName, out var fileValue) == true)
                {
                    // Recursively resolve variables in the value
                    return ResolveVariables(fileValue, fileVariables, environmentVariables) ?? string.Empty;
                }

                // Return original if variable not found
                return match.Value;
            });

            // Second pass: resolve environment variables (${variable})
            if (environmentVariables != null && environmentVariables.Count > 0)
            {
                result = _environmentVariableRegex.Replace(result, match =>
                {
                    var variableName = match.Groups[1].Value.Trim();
                    
                    if (environmentVariables.TryGetValue(variableName, out var value))
                    {
                        return value ?? string.Empty;
                    }

                    // Try system environment variables as fallback
                    var envValue = Environment.GetEnvironmentVariable(variableName);
                    return envValue ?? match.Value;
                });
            }
            else
            {
                // If no environment variables provided, try system environment variables
                result = _environmentVariableRegex.Replace(result, match =>
                {
                    var variableName = match.Groups[1].Value.Trim();
                    var envValue = Environment.GetEnvironmentVariable(variableName);
                    return envValue ?? match.Value;
                });
            }

            // Third pass: resolve system variables ({{$variable}})
            result = SystemVariableProcessor.ResolveSystemVariables(result);

            return result;
        }

        /// <summary>
        /// Processes all variables in an HTTP request
        /// </summary>
        /// <param name="request">The request to process</param>
        /// <param name="fileVariables">File-level variables</param>
        /// <param name="environmentVariables">Environment variables</param>
        /// <returns>A new request with variables resolved</returns>
        public static HttpRequest ProcessRequest(
            HttpRequest request,
            IReadOnlyDictionary<string, string>? fileVariables = null,
            IDictionary<string, string>? environmentVariables = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var processedRequest = new HttpRequest
            {
                Name = request.Name,
                Method = ResolveVariables(request.Method, fileVariables, environmentVariables) ?? string.Empty,
                Url = ResolveVariables(request.Url, fileVariables, environmentVariables) ?? string.Empty,
                Body = ResolveVariables(request.Body, fileVariables, environmentVariables),
                LineNumber = request.LineNumber
            };

            // Process headers
            foreach (var header in request.Headers)
            {
                var processedName = ResolveVariables(header.Key, fileVariables, environmentVariables) ?? header.Key;
                var processedValue = ResolveVariables(header.Value, fileVariables, environmentVariables) ?? header.Value;
                processedRequest.Headers[processedName] = processedValue;
            }

            return processedRequest;
        }

        /// <summary>
        /// Processes all requests in an HTTP file, resolving variables
        /// </summary>
        /// <param name="httpFile">The HTTP file to process</param>
        /// <param name="environmentVariables">Additional environment variables</param>
        /// <returns>A new HTTP file with variables resolved</returns>
        public static HttpFile ProcessHttpFile(
            HttpFile httpFile,
            IDictionary<string, string>? environmentVariables = null)
        {
            if (httpFile == null)
                throw new ArgumentNullException(nameof(httpFile));

            var processedRequests = new List<HttpRequest>();

            foreach (var request in httpFile.Requests)
            {
                var processedRequest = ProcessRequest(request, httpFile.FileVariables, environmentVariables);
                processedRequests.Add(processedRequest);
            }

            return new HttpFile(processedRequests, httpFile.FileVariables);
        }

        /// <summary>
        /// Extracts all variable references from content
        /// </summary>
        /// <param name="content">The content to analyze</param>
        /// <returns>Set of variable names referenced in the content</returns>
        public static HashSet<string> ExtractVariableReferences(string content)
        {
            var variables = new HashSet<string>();

            if (string.IsNullOrEmpty(content))
                return variables;

            // Extract file variable references
            var matches = _variableReferenceRegex.Matches(content);
            foreach (Match match in matches)
            {
                var variableName = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(variableName))
                {
                    variables.Add(variableName);
                }
            }

            // Extract environment variable references
            var envMatches = _environmentVariableRegex.Matches(content);
            foreach (Match match in envMatches)
            {
                var variableName = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(variableName))
                {
                    variables.Add($"${{{variableName}}}"); // Keep environment variable format
                }
            }

            return variables;
        }

        /// <summary>
        /// Validates that all variable references can be resolved
        /// </summary>
        /// <param name="content">The content to validate</param>
        /// <param name="fileVariables">Available file variables</param>
        /// <param name="environmentVariables">Available environment variables</param>
        /// <returns>List of unresolved variable names</returns>
        public static List<string> ValidateVariableReferences(
            string? content,
            IReadOnlyDictionary<string, string>? fileVariables = null,
            IDictionary<string, string>? environmentVariables = null)
        {
            var unresolvedVariables = new List<string>();

            if (string.IsNullOrEmpty(content))
                return unresolvedVariables;

            // Check file variable references
            var matches = _variableReferenceRegex.Matches(content);
            foreach (Match match in matches)
            {
                var variableName = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(variableName))
                {
                    if (fileVariables == null || !fileVariables.ContainsKey(variableName))
                    {
                        unresolvedVariables.Add($"{{{{{variableName}}}}}");
                    }
                }
            }

            // Check environment variable references
            var envMatches = _environmentVariableRegex.Matches(content);
            foreach (Match match in envMatches)
            {
                var variableName = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(variableName))
                {
                    var hasValue = (environmentVariables != null && environmentVariables.ContainsKey(variableName)) ||
                                   !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variableName));

                    if (!hasValue)
                    {
                        unresolvedVariables.Add($"${{{variableName}}}");
                    }
                }
            }

            return unresolvedVariables;
        }

        /// <summary>
        /// Validates variable references in an HTTP request
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <param name="fileVariables">Available file variables</param>
        /// <param name="environmentVariables">Available environment variables</param>
        /// <returns>Dictionary of property names to unresolved variables</returns>
        public static Dictionary<string, List<string>> ValidateRequestVariables(
            HttpRequest request,
            IReadOnlyDictionary<string, string>? fileVariables = null,
            IDictionary<string, string>? environmentVariables = null)
        {
            var result = new Dictionary<string, List<string>>();

            // Validate URL
            var urlUnresolved = ValidateVariableReferences(request.Url, fileVariables, environmentVariables);
            if (urlUnresolved.Count > 0)
                result["Url"] = urlUnresolved;

            // Validate method
            var methodUnresolved = ValidateVariableReferences(request.Method, fileVariables, environmentVariables);
            if (methodUnresolved.Count > 0)
                result["Method"] = methodUnresolved;

            // Validate body
            var bodyUnresolved = ValidateVariableReferences(request.Body, fileVariables, environmentVariables);
            if (bodyUnresolved.Count > 0)
                result["Body"] = bodyUnresolved;

            // Validate headers
            foreach (var header in request.Headers)
            {
                var headerNameUnresolved = ValidateVariableReferences(header.Key, fileVariables, environmentVariables);
                var headerValueUnresolved = ValidateVariableReferences(header.Value, fileVariables, environmentVariables);

                var headerUnresolved = new List<string>();
                headerUnresolved.AddRange(headerNameUnresolved);
                headerUnresolved.AddRange(headerValueUnresolved);

                if (headerUnresolved.Count > 0)
                    result[$"Header[{header.Key}]"] = headerUnresolved;
            }

            return result;
        }

        /// <summary>
        /// Detects circular variable references
        /// </summary>
        /// <param name="fileVariables">File variables to check</param>
        /// <returns>List of variables involved in circular references</returns>
        public static List<string> DetectCircularReferences(IReadOnlyDictionary<string, string>? fileVariables)
        {
            var circularVariables = new List<string>();

            if (fileVariables == null || fileVariables.Count == 0)
                return circularVariables;

            foreach (var variable in fileVariables)
            {
                if (HasCircularReference(variable.Key, variable.Value, fileVariables, new HashSet<string>()))
                {
                    circularVariables.Add(variable.Key);
                }
            }

            return circularVariables;
        }

        private static bool HasCircularReference(
            string currentVariable, 
            string value, 
            IReadOnlyDictionary<string, string> allVariables,
            HashSet<string> visitedVariables)
        {
            if (visitedVariables.Contains(currentVariable))
                return true;

            visitedVariables.Add(currentVariable);

            var referencedVariables = ExtractVariableReferences(value);
            foreach (var referencedVar in referencedVariables)
            {
                // Skip environment variables
                if (referencedVar.StartsWith("${"))
                    continue;

                // Extract variable name from {{variableName}} format
                var variableName = referencedVar.Trim('{', '}');
                
                if (allVariables.TryGetValue(variableName, out var referencedValue))
                {
                    if (HasCircularReference(variableName, referencedValue, allVariables, new HashSet<string>(visitedVariables)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
