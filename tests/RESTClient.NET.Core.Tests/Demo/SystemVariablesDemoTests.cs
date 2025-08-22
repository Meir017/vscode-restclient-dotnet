using AwesomeAssertions;
using RESTClient.NET.Core.Processing;
using Xunit;
using Xunit.Abstractions;

namespace RESTClient.NET.Core.Tests.Demo
{
    public class SystemVariablesDemoTests
    {
        private readonly ITestOutputHelper _output;

        public SystemVariablesDemoTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task SystemVariables_Integration_Demo()
        {
            // Arrange
            string httpContent = @"
@baseUrl = https://api.example.com

# @name create-user
POST {{baseUrl}}/users HTTP/1.1
Content-Type: application/json
X-Request-ID: {{$guid}}
X-Timestamp: {{$timestamp}}
X-Random-Value: {{$randomInt 1000 9999}}

{
  ""id"": ""{{$guid}}"",
  ""username"": ""user_{{$randomInt 100 999}}"",
  ""created"": ""{{$datetime iso8601}}"",
  ""timestamp"": {{$timestamp}}
}

###

# @name get-user
GET {{baseUrl}}/users/{{$guid}} HTTP/1.1
Authorization: Bearer token-{{$randomInt 10000 99999}}
";

            _output.WriteLine("Original HTTP content:");
            _output.WriteLine(httpContent);
            _output.WriteLine("");

            // Act - Parse the HTTP file
            var httpFileProcessor = new HttpFileProcessor();
            Core.Models.HttpFile httpFile = await httpFileProcessor.ParseContentAsync(httpContent);

            // Process variables
            Core.Models.HttpFile processedFile = VariableProcessor.ProcessHttpFile(httpFile);

            // Assert and demonstrate
            processedFile.Requests.Should().HaveCount(3); // includes an empty request from ###

            Core.Models.HttpRequest createUserRequest = processedFile.Requests[0];
            Core.Models.HttpRequest getUserRequest = processedFile.Requests[2]; // Skip the empty request at index 1

            _output.WriteLine("=== Processed Requests ===");
            _output.WriteLine("");

            // Create User Request
            _output.WriteLine($"1. {createUserRequest.Name}");
            _output.WriteLine($"   Method: {createUserRequest.Method}");
            _output.WriteLine($"   URL: {createUserRequest.Url}");

            foreach (KeyValuePair<string, string> header in createUserRequest.Headers)
            {
                _output.WriteLine($"   {header.Key}: {header.Value}");
            }

            _output.WriteLine($"   Body: {createUserRequest.Body}");
            _output.WriteLine("");

            // Get User Request
            _output.WriteLine($"2. {getUserRequest.Name}");
            _output.WriteLine($"   Method: {getUserRequest.Method}");
            _output.WriteLine($"   URL: {getUserRequest.Url}");

            foreach (KeyValuePair<string, string> header in getUserRequest.Headers)
            {
                _output.WriteLine($"   {header.Key}: {header.Value}");
            }
            _output.WriteLine("");

            // Verify system variables were resolved
            createUserRequest.Url.Should().StartWith("https://api.example.com/users");
            createUserRequest.Headers["X-Request-ID"].Should().MatchRegex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
            createUserRequest.Headers["X-Timestamp"].Should().MatchRegex(@"^\d{10}$");
            createUserRequest.Headers["X-Random-Value"].Should().MatchRegex(@"^[1-9][0-9]{3}$");

            getUserRequest.Url.Should().MatchRegex(@"^https://api\.example\.com/users/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}");
            getUserRequest.Headers["Authorization"].Should().MatchRegex(@"^Bearer token-[1-9][0-9]{4}$");

            // Verify body contains resolved variables
            createUserRequest.Body.Should().NotContain("{{$");
            createUserRequest.Body.Should().MatchRegex(@"""id"":\s*""[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}""");
            createUserRequest.Body.Should().MatchRegex(@"""username"":\s*""user_[1-9][0-9]{2}""");
            createUserRequest.Body.Should().MatchRegex(@"""created"":\s*""\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z""");
            createUserRequest.Body.Should().MatchRegex(@"""timestamp"":\s*\d{10}");

            _output.WriteLine("✅ All system variables were successfully resolved!");
        }

        [Fact]
        public void SystemVariables_AllTypes_Demo()
        {
            // Arrange
            (string, string)[] testCases = new[]
            {
                ("{{$guid}}", @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"),
                ("{{$randomInt 100 200}}", @"^1[0-9]{2}$"),
                ("{{$timestamp}}", @"^\d{10}$"),
                ("{{$datetime iso8601}}", @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$"),
                ("{{$datetime rfc1123}}", @"^[A-Za-z]{3}, \d{2} [A-Za-z]{3} \d{4} \d{2}:\d{2}:\d{2} GMT$"),
                ("{{$localDatetime}}", @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$")
            };

            _output.WriteLine("=== System Variables Resolution Demo ===");
            _output.WriteLine("");

            foreach ((string input, string expectedPattern) in testCases)
            {
                // Act
                string? result = SystemVariableProcessor.ResolveSystemVariables(input);

                // Assert
                result.Should().NotBe(input);
                result.Should().MatchRegex(expectedPattern);

                _output.WriteLine($"✅ {input,-30} → {result}");
            }

            _output.WriteLine("");
            _output.WriteLine("All system variable types work correctly!");
        }
    }
}
