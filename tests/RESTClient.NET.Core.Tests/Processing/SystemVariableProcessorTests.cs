using System;
using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using RESTClient.NET.Core.Processing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Processing
{
    public class SystemVariableProcessorTests
    {
        [Fact]
        public void ResolveSystemVariables_WithNull_ReturnsNull()
        {
            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ResolveSystemVariables_WithEmpty_ReturnsEmpty()
        {
            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(string.Empty);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void ResolveSystemVariables_WithNoVariables_ReturnsOriginal()
        {
            // Arrange
            var content = "This is plain text without variables";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().Be(content);
        }

        [Fact]
        public void ResolveSystemVariables_WithUnknownVariable_ReturnsOriginal()
        {
            // Arrange
            var content = "{{$unknownVariable}}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().Be(content);
        }

        [Fact]
        public void ResolveSystemVariables_WithGuid_ReturnsValidGuid()
        {
            // Arrange
            var content = "{{$guid}}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            Guid.TryParse(result, out _).Should().BeTrue();
        }

        [Fact]
        public void ResolveSystemVariables_WithMultipleGuids_ReturnsDifferentGuids()
        {
            // Arrange
            var content = "{{$guid}} and {{$guid}}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            var parts = result!.Split(new[] { " and " }, StringSplitOptions.None);
            parts.Should().HaveCount(2);
            parts[0].Should().NotBe(parts[1]);
            Guid.TryParse(parts[0], out _).Should().BeTrue();
            Guid.TryParse(parts[1], out _).Should().BeTrue();
        }

        [Theory]
        [InlineData("{{$randomInt 1 10}}", 1, 10)]
        [InlineData("{{$randomInt 0 100}}", 0, 100)]
        [InlineData("{{$randomInt -5 5}}", -5, 5)]
        public void ResolveSystemVariables_WithRandomInt_ReturnsValueInRange(string content, int min, int max)
        {
            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            int.TryParse(result, out var value).Should().BeTrue();
            value.Should().BeInRange(min, max - 1);
        }

        [Theory]
        [InlineData("{{$randomInt}}")]
        [InlineData("{{$randomInt 5}}")]
        [InlineData("{{$randomInt abc def}}")]
        [InlineData("{{$randomInt 10 5}}")]
        public void ResolveSystemVariables_WithInvalidRandomInt_ReturnsOriginal(string content)
        {
            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().Be(content);
        }

        [Fact]
        public void ResolveSystemVariables_WithTimestamp_ReturnsValidUnixTimestamp()
        {
            // Arrange
            var content = "{{$timestamp}}";
            var beforeTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);
            var afterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Assert
            result.Should().NotBe(content);
            long.TryParse(result, out var timestamp).Should().BeTrue();
            timestamp.Should().BeInRange(beforeTime, afterTime);
        }

        [Theory]
        [InlineData("{{$timestamp -1 d}}", -1, "d")]
        [InlineData("{{$timestamp 2 h}}", 2, "h")]
        [InlineData("{{$timestamp -30 m}}", -30, "m")]
        public void ResolveSystemVariables_WithTimestampOffset_ReturnsAdjustedTimestamp(string content, int offset, string unit)
        {
            // Arrange
            var expectedBase = DateTimeOffset.UtcNow;
            var expectedTime = unit switch
            {
                "d" => expectedBase.AddDays(offset),
                "h" => expectedBase.AddHours(offset),
                "m" => expectedBase.AddMinutes(offset),
                _ => expectedBase
            };

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            long.TryParse(result, out var timestamp).Should().BeTrue();
            
            var actualTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var timeDifference = Math.Abs((actualTime - expectedTime).TotalSeconds);
            timeDifference.Should().BeLessThan(2); // Allow 2 seconds tolerance
        }

        [Fact]
        public void ResolveSystemVariables_WithDatetime_ReturnsISO8601Format()
        {
            // Arrange
            var content = "{{$datetime}}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            DateTime.TryParseExact(result, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                .Should().BeTrue();
        }

        [Fact]
        public void ResolveSystemVariables_WithLocalDatetime_ReturnsISO8601Format()
        {
            // Arrange
            var content = "{{$localDatetime}}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            DateTime.TryParseExact(result, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                .Should().BeTrue();
        }

        [Theory]
        [InlineData("{{$datetime rfc1123}}", "R")]
        [InlineData("{{$datetime iso8601}}", "yyyy-MM-ddTHH:mm:ss.fffZ")]
        [InlineData("{{$datetime \"yyyy-MM-dd\"}}", "yyyy-MM-dd")]
        [InlineData("{{$datetime 'HH:mm:ss'}}", "HH:mm:ss")]
        public void ResolveSystemVariables_WithDatetimeFormat_ReturnsCorrectFormat(string content, string expectedFormat)
        {
            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            
            if (expectedFormat == "R")
            {
                // RFC 1123 format
                DateTime.TryParseExact(result, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    .Should().BeTrue();
            }
            else if (expectedFormat == "yyyy-MM-ddTHH:mm:ss.fffZ")
            {
                // ISO 8601 format
                DateTime.TryParseExact(result, expectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    .Should().BeTrue();
            }
            else
            {
                // Custom format
                DateTime.TryParseExact(result, expectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    .Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("{{$datetime iso8601 -1 d}}")]
        [InlineData("{{$datetime rfc1123 2 h}}")]
        public void ResolveSystemVariables_WithDatetimeFormatAndOffset_ReturnsCorrectDateTime(string content)
        {
            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            
            // Parse result based on expected format
            DateTime parsedDateTime;
            if (content.Contains("rfc1123"))
            {
                DateTime.TryParseExact(result, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime)
                    .Should().BeTrue();
            }
            else if (content.Contains("iso8601"))
            {
                DateTime.TryParseExact(result, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime)
                    .Should().BeTrue();
            }
            else
            {
                DateTime.TryParseExact(result, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime)
                    .Should().BeTrue();
            }

            // For simple validation, just ensure the result is properly formatted
            result.Should().NotContain("{{$datetime");
        }

        [Fact]
        public void ResolveSystemVariables_WithComplexContent_ResolvesAllVariables()
        {
            // Arrange
            var content = @"POST /api/test
Authorization: Bearer {{$guid}}
Content-Type: application/json
X-Timestamp: {{$timestamp}}
X-Random: {{$randomInt 100 200}}

{
  ""id"": ""{{$guid}}"",
  ""date"": ""{{$datetime iso8601}}"",
  ""value"": {{$randomInt 1 1000}}
}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            result.Should().NotContain("{{$guid}}");
            result.Should().NotContain("{{$timestamp}}");
            result.Should().NotContain("{{$randomInt");
            result.Should().NotContain("{{$datetime");
        }

        [Theory]
        [InlineData("{{$timestamp invalid offset}}", "{{$timestamp invalid offset}}")]
        [InlineData("{{$datetime unknownformat}}", "{{$datetime unknownformat}}")]
        [InlineData("{{$datetime iso8601 invalid offset}}", "{{$datetime iso8601 invalid offset}}")]
        public void ResolveSystemVariables_WithInvalidParameters_ReturnsOriginal(string content, string expected)
        {
            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ResolveSystemVariables_CaseInsensitive_ResolvesCorrectly()
        {
            // Arrange
            var content = "{{$GUID}} {{$randomINT 1 10}} {{$TIMESTAMP}} {{$DateTime}} {{$LocalDateTime}}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            result.Should().NotContain("{{$");
        }

        [Theory]
        [InlineData("y", 1)]
        [InlineData("M", 1)]
        [InlineData("w", 1)]
        [InlineData("d", 1)]
        [InlineData("h", 1)]
        [InlineData("m", 1)]
        [InlineData("s", 1)]
        [InlineData("ms", 1000)]
        public void ResolveSystemVariables_WithAllTimeUnits_ReturnsValidTimestamp(string unit, int offset)
        {
            // Arrange
            var content = $"{{{{$timestamp {offset} {unit}}}}}";

            // Act
            var result = SystemVariableProcessor.ResolveSystemVariables(content);

            // Assert
            result.Should().NotBe(content);
            long.TryParse(result, out _).Should().BeTrue();
        }
    }
}
