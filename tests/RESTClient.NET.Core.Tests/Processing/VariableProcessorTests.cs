using AwesomeAssertions;
using RESTClient.NET.Core.Processing;
using Xunit;

namespace RESTClient.NET.Core.Tests.Processing
{
    public class VariableProcessorTests
    {
        [Fact]
        public void ResolveVariables_WithFileVariables_ShouldReplaceCorrectly()
        {
            // Arrange
            string content = "GET {{baseUrl}}/api/{{endpoint}}";
            var fileVariables = new Dictionary<string, string>
            {
                { "baseUrl", "http://localhost:5000" },
                { "endpoint", "users" }
            };

            // Act
            string? result = VariableProcessor.ResolveVariables(content, fileVariables);

            // Assert
            result.Should().Be("GET http://localhost:5000/api/users");
        }

        [Fact]
        public void ResolveVariables_WithEnvironmentVariables_ShouldReplaceCorrectly()
        {
            // Arrange
            string content = "Authorization: Bearer ${API_TOKEN}";
            var environmentVariables = new Dictionary<string, string>
            {
                { "API_TOKEN", "secret123" }
            };

            // Act
            string? result = VariableProcessor.ResolveVariables(content, null, environmentVariables);

            // Assert
            result.Should().Be("Authorization: Bearer secret123");
        }

        [Fact]
        public void ResolveVariables_WithNestedVariables_ShouldResolveRecursively()
        {
            // Arrange
            string content = "GET {{fullUrl}}";
            var fileVariables = new Dictionary<string, string>
            {
                { "fullUrl", "{{baseUrl}}/{{endpoint}}" },
                { "baseUrl", "http://localhost:5000" },
                { "endpoint", "api/users" }
            };

            // Act
            string? result = VariableProcessor.ResolveVariables(content, fileVariables);

            // Assert
            result.Should().Be("GET http://localhost:5000/api/users");
        }

        [Fact]
        public void ResolveVariables_WithUnknownVariable_ShouldLeaveUnchanged()
        {
            // Arrange
            string content = "GET {{baseUrl}}/{{unknown}}";
            var fileVariables = new Dictionary<string, string>
            {
                { "baseUrl", "http://localhost:5000" }
            };

            // Act
            string? result = VariableProcessor.ResolveVariables(content, fileVariables);

            // Assert
            result.Should().Be("GET http://localhost:5000/{{unknown}}");
        }

        [Fact]
        public void ResolveVariables_WithNullOrEmptyContent_ShouldReturnOriginal()
        {
            // Act & Assert
            VariableProcessor.ResolveVariables(null).Should().BeNull();
            VariableProcessor.ResolveVariables("").Should().Be("");
            VariableProcessor.ResolveVariables("   ").Should().Be("   ");
        }

        [Fact]
        public void ExtractVariableReferences_ShouldReturnAllVariables()
        {
            // Arrange
            string content = "GET {{baseUrl}}/api/{{endpoint}}?token=${API_TOKEN}";

            // Act
            HashSet<string> variables = VariableProcessor.ExtractVariableReferences(content);

            // Assert
            variables.Should().HaveCount(3);
            variables.Should().Contain("baseUrl");
            variables.Should().Contain("endpoint");
            variables.Should().Contain("${API_TOKEN}");
        }

        [Fact]
        public void ValidateVariableReferences_WithUnresolvedVariables_ShouldReturnUnresolved()
        {
            // Arrange
            string content = "GET {{baseUrl}}/{{unknown}}";
            var fileVariables = new Dictionary<string, string>
            {
                { "baseUrl", "http://localhost:5000" }
            };

            // Act
            List<string> unresolved = VariableProcessor.ValidateVariableReferences(content, fileVariables);

            // Assert
            unresolved.Should().HaveCount(1);
            unresolved.Should().Contain("{{unknown}}");
        }

        [Fact]
        public void DetectCircularReferences_WithCircularDependency_ShouldDetectCorrectly()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                { "var1", "{{var2}}" },
                { "var2", "{{var3}}" },
                { "var3", "{{var1}}" }, // Circular reference
                { "var4", "normal value" }
            };

            // Act
            List<string> circularVariables = VariableProcessor.DetectCircularReferences(fileVariables);

            // Assert
            circularVariables.Should().HaveCount(3);
            circularVariables.Should().Contain("var1");
            circularVariables.Should().Contain("var2");
            circularVariables.Should().Contain("var3");
            circularVariables.Should().NotContain("var4");
        }

        [Fact]
        public void DetectCircularReferences_WithSelfReference_ShouldDetectCorrectly()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                { "recursive", "value with {{recursive}} reference" },
                { "normal", "normal value" }
            };

            // Act
            List<string> circularVariables = VariableProcessor.DetectCircularReferences(fileVariables);

            // Assert
            circularVariables.Should().HaveCount(1);
            circularVariables.Should().Contain("recursive");
            circularVariables.Should().NotContain("normal");
        }

        [Fact]
        public void DetectCircularReferences_WithNoCircularReferences_ShouldReturnEmpty()
        {
            // Arrange
            var fileVariables = new Dictionary<string, string>
            {
                { "baseUrl", "http://localhost:5000" },
                { "endpoint", "api/users" },
                { "fullUrl", "{{baseUrl}}/{{endpoint}}" }
            };

            // Act
            List<string> circularVariables = VariableProcessor.DetectCircularReferences(fileVariables);

            // Assert
            circularVariables.Should().BeEmpty();
        }

        [Theory]
        [InlineData("{{variable}}", "variable")]
        [InlineData("{{ variable }}", "variable")]
        [InlineData("{{  variable  }}", "variable")]
        [InlineData("prefix{{variable}}suffix", "variable")]
        [InlineData("{{var1}}{{var2}}", "var1", "var2")]
        public void ExtractVariableReferences_WithVariousFormats_ShouldExtractCorrectly(string content, params string[] expectedVariables)
        {
            // Act
            HashSet<string> variables = VariableProcessor.ExtractVariableReferences(content);

            // Assert
            foreach (string expected in expectedVariables)
            {
                variables.Should().Contain(expected);
            }
        }

        [Theory]
        [InlineData("${ENV_VAR}", "${ENV_VAR}")]
        [InlineData("${ ENV_VAR }", "${ENV_VAR}")]
        [InlineData("${  ENV_VAR  }", "${ENV_VAR}")]
        [InlineData("prefix${ENV_VAR}suffix", "${ENV_VAR}")]
        public void ExtractVariableReferences_WithEnvironmentVariables_ShouldExtractCorrectly(string content, string expectedVariable)
        {
            // Act
            HashSet<string> variables = VariableProcessor.ExtractVariableReferences(content);

            // Assert
            variables.Should().Contain(expectedVariable);
        }

        [Fact]
        public void ResolveVariables_WithMixedVariableTypes_ShouldResolveInCorrectOrder()
        {
            // Arrange
            string content = "{{greeting}} ${USER}, your token is {{token}}";
            var fileVariables = new Dictionary<string, string>
            {
                { "greeting", "Hello" },
                { "token", "abc123" }
            };
            var environmentVariables = new Dictionary<string, string>
            {
                { "USER", "John" }
            };

            // Act
            string? result = VariableProcessor.ResolveVariables(content, fileVariables, environmentVariables);

            // Assert
            result.Should().Be("Hello John, your token is abc123");
        }

        [Fact]
        public void ResolveVariables_WithSystemVariables_ShouldReplaceCorrectly()
        {
            // Arrange
            string content = "X-Request-ID: {{$guid}}";

            // Act
            string? result = VariableProcessor.ResolveVariables(content);

            // Assert
            result.Should().NotBe(content);
            result.Should().StartWith("X-Request-ID: ");
            string guidPart = result.Substring("X-Request-ID: ".Length);
            Guid.TryParse(guidPart, out _).Should().BeTrue();
        }

        [Fact]
        public void ResolveVariables_WithMixedVariableTypes_ShouldResolveAllTypes()
        {
            // Arrange
            string content = "POST {{baseUrl}}/api/test\nAuthorization: Bearer ${API_TOKEN}\nX-Request-ID: {{$guid}}\nX-Timestamp: {{$timestamp}}";
            var fileVariables = new Dictionary<string, string>
            {
                { "baseUrl", "https://api.example.com" }
            };
            var environmentVariables = new Dictionary<string, string>
            {
                { "API_TOKEN", "secret123" }
            };

            // Act
            string? result = VariableProcessor.ResolveVariables(content, fileVariables, environmentVariables);

            // Assert
            result.Should().NotBe(content);
            result.Should().Contain("https://api.example.com/api/test");
            result.Should().Contain("Bearer secret123");
            result.Should().NotContain("{{$guid}}");
            result.Should().NotContain("{{$timestamp}}");
            result.Should().NotContain("{{baseUrl}}");
            result.Should().NotContain("${API_TOKEN}");
        }

        [Fact]
        public void ResolveVariables_WithSystemVariablesInFileVariables_ShouldResolveSystemVariablesInValues()
        {
            // Arrange
            string content = "Authorization: {{authHeader}}";
            var fileVariables = new Dictionary<string, string>
            {
                { "authHeader", "Bearer {{$guid}}" }
            };

            // Act
            string? result = VariableProcessor.ResolveVariables(content, fileVariables);

            // Assert
            result.Should().NotBe(content);
            result.Should().StartWith("Authorization: Bearer ");
            result.Should().NotContain("{{$guid}}");
            result.Should().NotContain("{{authHeader}}");

            string guidPart = result.Substring("Authorization: Bearer ".Length);
            Guid.TryParse(guidPart, out _).Should().BeTrue();
        }

        [Fact]
        public void ResolveVariables_WithComplexSystemVariables_ShouldResolveCorrectly()
        {
            // Arrange
            string content = @"POST /api/test
Content-Type: application/json
X-Random-Value: {{$randomInt 100 200}}
X-Timestamp: {{$timestamp -1 h}}

{
  ""id"": ""{{$guid}}"",
  ""created"": ""{{$datetime iso8601}}""
}";

            // Act
            string? result = VariableProcessor.ResolveVariables(content);

            // Assert
            result.Should().NotBe(content);
            result.Should().NotContain("{{$randomInt");
            result.Should().NotContain("{{$timestamp");
            result.Should().NotContain("{{$guid}}");
            result.Should().NotContain("{{$datetime");
        }
    }
}
