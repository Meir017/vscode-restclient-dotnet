using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RESTClient.NET.Core.Processing
{
    /// <summary>
    /// Processes built-in system variables ({{$variableName}}) that provide dynamic values.
    /// Compatible with VS Code REST Client system variables.
    /// </summary>
    /// <remarks>
    /// <para>Supported system variables:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Variable</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><c>{{$guid}}</c></term>
    /// <description>Generates a RFC 4122 v4 UUID</description>
    /// </item>
    /// <item>
    /// <term><c>{{$randomInt min max}}</c></term>
    /// <description>Random integer between min (inclusive) and max (exclusive)</description>
    /// </item>
    /// <item>
    /// <term><c>{{$timestamp [offset]}}</c></term>
    /// <description>UTC timestamp in seconds, with optional time offset</description>
    /// </item>
    /// <item>
    /// <term><c>{{$datetime format [offset]}}</c></term>
    /// <description>Formatted datetime string (iso8601, rfc1123, or custom format)</description>
    /// </item>
    /// <item>
    /// <term><c>{{$localDatetime format [offset]}}</c></term>
    /// <description>Local timezone datetime string</description>
    /// </item>
    /// </list>
    /// <para>Time offsets support: y (year), M (month), w (week), d (day), h (hour), m (minute), s (second), ms (millisecond)</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var content = @"
    /// POST /api/users HTTP/1.1
    /// X-Request-ID: {{$guid}}
    /// Content-Type: application/json
    /// 
    /// {
    ///   ""id"": ""{{$guid}}"",
    ///   ""timestamp"": {{$timestamp}},
    ///   ""score"": {{$randomInt 1 100}},
    ///   ""created"": ""{{$datetime iso8601}}"",
    ///   ""expires"": ""{{$datetime iso8601 1 d}}""
    /// }";
    /// 
    /// var resolved = SystemVariableProcessor.ResolveSystemVariables(content);
    /// // All {{$...}} variables will be replaced with actual values
    /// </code>
    /// </example>
    public static class SystemVariableProcessor
    {
        private static readonly Regex __systemVariableRegex = new Regex(
            @"\{\{\$([a-zA-Z]+)(?:\s+([^}]+))?\}\}", 
            RegexOptions.Compiled);

        private static readonly Random __randomGenerator = new Random();

        /// <summary>
        /// Resolves all system variables in the given content
        /// </summary>
        /// <param name="content">Content containing system variable references</param>
        /// <returns>Content with system variables resolved</returns>
        public static string? ResolveSystemVariables(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            return _systemVariableRegex.Replace(content, match =>
            {
                var variableName = match.Groups[1].Value.ToLowerInvariant();
                var parameters = match.Groups[2].Success ? match.Groups[2].Value.Trim() : string.Empty;

                try
                {
                    return variableName switch
                    {
                        "guid" => ResolveGuid(),
                        "randomint" => ResolveRandomInt(parameters),
                        "timestamp" => ResolveTimestamp(parameters),
                        "datetime" => ResolveDatetime(parameters, useLocalTime: false),
                        "localdatetime" => ResolveDatetime(parameters, useLocalTime: true),
                        _ => match.Value // Return original if unknown variable
                    };
                }
                catch (Exception)
                {
                    // Return original value if resolution fails
                    return match.Value;
                }
            });
        }

        /// <summary>
        /// Generates a new RFC 4122 v4 UUID
        /// </summary>
        /// <returns>UUID string</returns>
        private static string ResolveGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generates a random integer between min (inclusive) and max (exclusive)
        /// </summary>
        /// <param name="parameters">Parameters in format "min max"</param>
        /// <returns>Random integer as string</returns>
        private static string ResolveRandomInt(string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
                throw new ArgumentException("randomInt requires min and max parameters");

            var parts = parameters.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new ArgumentException("randomInt requires exactly two parameters: min max");

            if (!int.TryParse(parts[0], out var min) || !int.TryParse(parts[1], out var max))
                throw new ArgumentException("randomInt parameters must be valid integers");

            if (min >= max)
                throw new ArgumentException("randomInt min parameter must be less than max parameter");

            return _randomGenerator.Next(min, max).ToString();
        }

        /// <summary>
        /// Generates a UTC timestamp with optional offset
        /// </summary>
        /// <param name="parameters">Optional offset specification like "-1 d" or "2 h"</param>
        /// <returns>Unix timestamp as string</returns>
        private static string ResolveTimestamp(string parameters)
        {
            var baseTime = DateTimeOffset.UtcNow;

            if (!string.IsNullOrEmpty(parameters))
            {
                baseTime = ApplyTimeOffset(baseTime, parameters);
            }

            return baseTime.ToUnixTimeSeconds().ToString();
        }

        /// <summary>
        /// Generates a formatted datetime string with optional offset
        /// </summary>
        /// <param name="parameters">Format and optional offset like "iso8601" or "rfc1123 -2 h" or "\"yyyy-MM-dd\" 1 w"</param>
        /// <param name="useLocalTime">True to use local time, false for UTC</param>
        /// <returns>Formatted datetime string</returns>
        private static string ResolveDatetime(string parameters, bool useLocalTime)
        {
            var baseTime = useLocalTime ? DateTimeOffset.Now : DateTimeOffset.UtcNow;
            var format = "iso8601";

            if (!string.IsNullOrEmpty(parameters))
            {
                var parts = SplitParametersRespectingQuotes(parameters);
                
                if (parts.Length > 0)
                {
                    format = parts[0];
                }

                if (parts.Length > 1)
                {
                    // Apply time offset
                    var offsetSpec = string.Join(" ", parts, 1, parts.Length - 1);
                    baseTime = ApplyTimeOffset(baseTime, offsetSpec);
                }
            }

            return FormatDateTime(baseTime, format);
        }

        /// <summary>
        /// Applies a time offset to a base datetime
        /// </summary>
        /// <param name="baseTime">Base datetime</param>
        /// <param name="offsetSpec">Offset specification like "-3 h" or "2 d"</param>
        /// <returns>Datetime with offset applied</returns>
        private static DateTimeOffset ApplyTimeOffset(DateTimeOffset baseTime, string offsetSpec)
        {
            var parts = offsetSpec.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new ArgumentException($"Invalid offset specification: {offsetSpec}. Expected format: 'offset unit'");

            if (!int.TryParse(parts[0], out var offset))
                throw new ArgumentException($"Invalid offset value: {parts[0]}. Must be an integer");

            var unit = parts[1].ToLowerInvariant();
            return unit switch
            {
                "y" => baseTime.AddYears(offset),
                "m" when parts[1] == "M" => baseTime.AddMonths(offset), // Capital M for months
                "w" => baseTime.AddDays(offset * 7),
                "d" => baseTime.AddDays(offset),
                "h" => baseTime.AddHours(offset),
                "m" => baseTime.AddMinutes(offset), // Lowercase m for minutes
                "s" => baseTime.AddSeconds(offset),
                "ms" => baseTime.AddMilliseconds(offset),
                _ => throw new ArgumentException($"Unknown time unit: {parts[1]}. Supported units: y, M, w, d, h, m, s, ms")
            };
        }

        /// <summary>
        /// Formats a datetime according to the specified format
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="format">Format specification</param>
        /// <returns>Formatted datetime string</returns>
        private static string FormatDateTime(DateTimeOffset dateTime, string format)
        {
            return format.ToLowerInvariant() switch
            {
                "rfc1123" => dateTime.ToString("R", CultureInfo.InvariantCulture),
                "iso8601" => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                _ when IsQuotedString(format) => dateTime.ToString(UnquoteString(format), CultureInfo.InvariantCulture),
                _ => throw new ArgumentException($"Unknown datetime format: {format}. Supported formats: rfc1123, iso8601, or custom format in quotes")
            };
        }

        /// <summary>
        /// Splits parameters while respecting quoted strings
        /// </summary>
        /// <param name="parameters">Parameter string</param>
        /// <returns>Array of parameter parts</returns>
        private static string[] SplitParametersRespectingQuotes(string parameters)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            var quoteChar = '\0';

            for (int i = 0; i < parameters.Length; i++)
            {
                var c = parameters[i];

                if (!inQuotes && (c == '"' || c == '\''))
                {
                    inQuotes = true;
                    quoteChar = c;
                    current.Append(c);
                }
                else if (inQuotes && c == quoteChar)
                {
                    inQuotes = false;
                    current.Append(c);
                }
                else if (!inQuotes && char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        parts.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                parts.Add(current.ToString());
            }

            return parts.ToArray();
        }

        /// <summary>
        /// Checks if a string is quoted (starts and ends with quotes)
        /// </summary>
        /// <param name="value">String to check</param>
        /// <returns>True if quoted</returns>
        private static bool IsQuotedString(string value)
        {
            return value.Length >= 2 &&
                   ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'")));
        }

        /// <summary>
        /// Removes surrounding quotes from a string
        /// </summary>
        /// <param name="value">Quoted string</param>
        /// <returns>Unquoted string</returns>
        private static string UnquoteString(string value)
        {
            if (IsQuotedString(value))
            {
                return value.Substring(1, value.Length - 2);
            }
            return value;
        }
    }
}
