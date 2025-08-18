# Troubleshooting Guide

Common issues and solutions when working with RESTClient.NET.

## Table of Contents

- [Installation Issues](#installation-issues)
- [Parsing Errors](#parsing-errors)
- [Variable Resolution Issues](#variable-resolution-issues)
- [System Variables Not Working](#system-variables-not-working)
- [Request Execution Issues](#request-execution-issues)
- [Testing Framework Issues](#testing-framework-issues)
- [Performance Issues](#performance-issues)
- [Common Error Messages](#common-error-messages)
- [Debugging Tips](#debugging-tips)
- [Getting Help](#getting-help)

## Installation Issues

### Package Not Found

**Problem:** NuGet package cannot be found or installed.

**Solutions:**

1. **Verify Package Source:**
   ```xml
   <PackageReference Include="RESTClient.NET.Core" Version="1.0.0" />
   ```

2. **Clear NuGet Cache:**
   ```bash
   dotnet nuget locals all --clear
   ```

3. **Restore Packages:**
   ```bash
   dotnet restore
   ```

4. **Check Target Framework:**
   Ensure your project targets a supported framework:
   ```xml
   <TargetFramework>net8.0</TargetFramework>
   <!-- or -->
   <TargetFramework>netstandard2.0</TargetFramework>
   ```

### Version Conflicts

**Problem:** Dependency version conflicts during installation.

**Solutions:**

1. **Check Dependencies:**
   ```xml
   <!-- RESTClient.NET.Core dependencies -->
   <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
   <PackageReference Include="System.Text.Json" Version="8.0.0" />
   ```

2. **Use Specific Versions:**
   ```xml
   <PackageReference Include="RESTClient.NET.Core" Version="1.0.0" />
   <PackageReference Include="RESTClient.NET.Testing" Version="1.0.0" />
   ```

## Parsing Errors

### HttpParseException: Invalid Request Format

**Problem:** Request format doesn't match expected HTTP syntax.

**Common Causes:**

1. **Missing HTTP Version:**
   ```http
   # ❌ Incorrect
   GET https://api.example.com/users
   
   # ✅ Correct
   GET https://api.example.com/users HTTP/1.1
   ```

2. **Invalid HTTP Method:**
   ```http
   # ❌ Incorrect
   FETCH https://api.example.com/users HTTP/1.1
   
   # ✅ Correct
   GET https://api.example.com/users HTTP/1.1
   ```

3. **Malformed Headers:**
   ```http
   # ❌ Incorrect
   Content-Type application/json
   
   # ✅ Correct
   Content-Type: application/json
   ```

### HttpParseException: Duplicate Request Name

**Problem:** Multiple requests have the same name.

**Solution:**
```http
# ❌ Incorrect - duplicate names
# @name user-request
GET {{baseUrl}}/users HTTP/1.1

# @name user-request  # Duplicate!
POST {{baseUrl}}/users HTTP/1.1

# ✅ Correct - unique names
# @name get-users
GET {{baseUrl}}/users HTTP/1.1

# @name create-user
POST {{baseUrl}}/users HTTP/1.1
```

### Invalid Variable Definition

**Problem:** Variable syntax is incorrect.

**Solutions:**

1. **Missing @ Symbol:**
   ```http
   # ❌ Incorrect
   baseUrl = https://api.example.com
   
   # ✅ Correct
   @baseUrl = https://api.example.com
   ```

2. **Invalid Variable Names:**
   ```http
   # ❌ Incorrect
   @base-url with spaces = https://api.example.com
   
   # ✅ Correct
   @baseUrl = https://api.example.com
   ```

## Variable Resolution Issues

### Variable Not Found

**Problem:** `{{variableName}}` is not being resolved.

**Debugging Steps:**

1. **Check Variable Definition:**
   ```http
   # Define before use
   @baseUrl = https://api.example.com
   
   # Use after definition
   GET {{baseUrl}}/users HTTP/1.1
   ```

2. **Verify Variable Name:**
   ```http
   # ❌ Case mismatch
   @baseUrl = https://api.example.com
   GET {{baseurl}}/users HTTP/1.1  # Wrong case
   
   # ✅ Correct case
   @baseUrl = https://api.example.com
   GET {{baseUrl}}/users HTTP/1.1
   ```

3. **Check for Typos:**
   ```csharp
   // Debug variable resolution
   var httpFile = HttpFileProcessor.Process("path/to/file.http");
   var variables = httpFile.Variables;
   
   foreach (var variable in variables)
   {
       Console.WriteLine($"{variable.Name} = {variable.Value}");
   }
   ```

### Circular Variable References

**Problem:** Variables reference each other in a loop.

**Example of Problem:**
```http
@urlA = {{urlB}}/path
@urlB = {{urlA}}/other
```

**Solution:**
```http
# ✅ Define base variables first
@baseUrl = https://api.example.com
@version = v1

# ✅ Build complex variables from base
@usersUrl = {{baseUrl}}/{{version}}/users
@authUrl = {{baseUrl}}/{{version}}/auth
```

## System Variables Not Working

### {{$guid}} Not Generating Values

**Problem:** System variables appear as literal text.

**Diagnosis:**
```csharp
var request = httpFile.GetRequestByName("test-request");
Console.WriteLine($"URL: {request.Url}");
Console.WriteLine($"Body: {request.Body}");

// Check if variables are resolved
if (request.Url.Contains("{{$guid}}"))
{
    Console.WriteLine("System variables not resolved!");
}
```

**Solutions:**

1. **Verify Syntax:**
   ```http
   # ✅ Correct
   X-Request-ID: {{$guid}}
   
   # ❌ Common mistakes
   X-Request-ID: {$guid}      # Missing second brace
   X-Request-ID: {{guid}}     # Missing $
   X-Request-ID: {{$GUID}}    # Wrong case
   ```

2. **Check Supported Variables:**
   - `{{$guid}}` - UUID v4
   - `{{$randomInt min max}}` - Random integer
   - `{{$timestamp}}` - Unix timestamp
   - `{{$datetime format}}` - Formatted date/time

### {{$randomInt}} Invalid Arguments

**Problem:** Random integer generation fails.

**Common Issues:**
```http
# ❌ Incorrect syntax
{{$randomInt 1-10}}      # Use space, not dash
{{$randomInt(1, 10)}}    # No parentheses
{{$randomInt 10 1}}      # Min > Max

# ✅ Correct syntax
{{$randomInt 1 10}}
{{$randomInt 0 100}}
{{$randomInt 1000 9999}}
```

### {{$datetime}} Format Issues

**Problem:** Date/time formatting not working.

**Solutions:**

1. **Use Predefined Formats:**
   ```http
   # ✅ Supported predefined formats
   {{$datetime iso8601}}    # 2023-12-01T15:30:45.123Z
   {{$datetime rfc1123}}    # Fri, 01 Dec 2023 15:30:45 GMT
   ```

2. **Use .NET Format Strings:**
   ```http
   # ✅ Custom formats
   {{$datetime yyyy-MM-dd}}         # 2023-12-01
   {{$datetime HH:mm:ss}}           # 15:30:45
   {{$datetime yyyy-MM-ddTHH:mm:ssZ}}  # ISO format
   ```

3. **Escape Special Characters:**
   ```http
   # If format contains special characters
   {{$datetime "yyyy-MM-dd HH:mm:ss"}}
   ```

## Request Execution Issues

### Request Not Found

**Problem:** `GetRequestByName()` throws `KeyNotFoundException`.

**Diagnosis:**
```csharp
// Check available requests
var httpFile = HttpFileProcessor.Process("file.http");
Console.WriteLine("Available requests:");
foreach (var request in httpFile.Requests)
{
    Console.WriteLine($"- {request.Metadata.Name}");
}

// Safe lookup
if (httpFile.TryGetRequestByName("my-request", out var request))
{
    // Request found
}
else
{
    Console.WriteLine("Request 'my-request' not found");
}
```

**Solutions:**

1. **Verify Request Name:**
   ```http
   # Make sure name matches exactly
   # @name user-login
   POST {{baseUrl}}/auth/login HTTP/1.1
   ```

2. **Check for Typos:**
   ```csharp
   // ❌ Case mismatch
   var request = httpFile.GetRequestByName("User-Login");  // Wrong case
   
   // ✅ Correct
   var request = httpFile.GetRequestByName("user-login");
   ```

### Empty Request Collection

**Problem:** `httpFile.Requests` is empty.

**Common Causes:**

1. **No Named Requests:**
   ```http
   # ❌ Request without name is ignored
   GET {{baseUrl}}/users HTTP/1.1
   
   # ✅ Named request is included
   # @name get-users
   GET {{baseUrl}}/users HTTP/1.1
   ```

2. **File Not Found:**
   ```csharp
   // Check if file exists and is readable
   if (!File.Exists(filePath))
   {
       throw new FileNotFoundException($"HTTP file not found: {filePath}");
   }
   ```

## Testing Framework Issues

### WebApplicationFactory Not Starting

**Problem:** Integration tests fail to start the web application.

**Solutions:**

1. **Check Test Project Setup:**
   ```xml
   <Project Sdk="Microsoft.NET.Sdk.Web">
     <PropertyGroup>
       <TargetFramework>net8.0</TargetFramework>
       <IsPackable>false</IsPackable>
       <IsTestProject>true</IsTestProject>
     </PropertyGroup>
   
     <ItemGroup>
       <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
       <PackageReference Include="RESTClient.NET.Testing" Version="1.0.0" />
     </ItemGroup>
   
     <ItemGroup>
       <ProjectReference Include="..\MyApi\MyApi.csproj" />
     </ItemGroup>
   </Project>
   ```

2. **Configure Test Host:**
   ```csharp
   public class ApiTestFixture : WebApplicationFactory<Program>
   {
       protected override void ConfigureWebHost(IWebHostBuilder builder)
       {
           builder.UseEnvironment("Testing");
           builder.ConfigureServices(services =>
           {
               // Test-specific service configuration
               services.AddDbContext<TestDbContext>(options =>
                   options.UseInMemoryDatabase("TestDb"));
           });
       }
   }
   ```

### HTTP File Not Loading in Tests

**Problem:** Test cannot find or load the HTTP file.

**Solutions:**

1. **Check File Path:**
   ```csharp
   public class HttpFileTests : HttpFileTestBase<Program>
   {
       protected override string HttpFilePath => 
           Path.Combine(AppContext.BaseDirectory, "HttpFiles", "api-tests.http");
   
       // Or use relative path from test project
       // protected override string HttpFilePath => "api-tests.http";
   }
   ```

2. **Ensure File is Copied:**
   ```xml
   <ItemGroup>
     <None Include="HttpFiles\*.http">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </None>
   </ItemGroup>
   ```

3. **Debug File Loading:**
   ```csharp
   [Fact]
   public void CanLoadHttpFile()
   {
       Console.WriteLine($"Looking for file: {HttpFilePath}");
       Console.WriteLine($"File exists: {File.Exists(HttpFilePath)}");
       Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
       
       // This should not throw
       var httpFile = LoadHttpFile();
       Assert.NotNull(httpFile);
   }
   ```

### Test Expectations Failing

**Problem:** `# @expect` comments are not working as expected.

**Debugging:**

1. **Check Response Details:**
   ```csharp
   [Theory]
   [MemberData(nameof(GetTestData))]
   public async Task TestRequest(string requestName, HttpRequestMessage httpRequest)
   {
       var response = await Client.SendAsync(httpRequest);
       
       // Debug response
       Console.WriteLine($"Status: {response.StatusCode}");
       Console.WriteLine($"Headers: {string.Join(", ", response.Headers)}");
       var content = await response.Content.ReadAsStringAsync();
       Console.WriteLine($"Body: {content}");
       
       // Perform assertions
       await AssertExpectations(requestName, response);
   }
   ```

2. **Verify Expectation Syntax:**
   ```http
   # ✅ Correct expectation syntax
   # @expect status 200
   # @expect header Content-Type application/json
   # @expect body-contains "success"
   # @expect body-path $.id
   
   # ❌ Common mistakes
   # @expect status: 200      # No colon
   # @expect header: Content-Type  # No colon  
   # @expect body-contains: "success"  # No colon
   ```

## Performance Issues

### Slow File Parsing

**Problem:** Large HTTP files take too long to parse.

**Solutions:**

1. **Split Large Files:**
   ```
   # Instead of one large file
   large-api-tests.http (500+ requests)
   
   # Split into smaller files
   users-api.http (50 requests)
   products-api.http (30 requests)
   orders-api.http (40 requests)
   ```

2. **Profile Parsing:**
   ```csharp
   var stopwatch = Stopwatch.StartNew();
   var httpFile = HttpFileProcessor.Process(filePath);
   stopwatch.Stop();
   Console.WriteLine($"Parsing took: {stopwatch.ElapsedMilliseconds}ms");
   ```

3. **Cache Parsed Results:**
   ```csharp
   private static readonly ConcurrentDictionary<string, HttpFile> _cache = new();
   
   public HttpFile GetHttpFile(string filePath)
   {
       return _cache.GetOrAdd(filePath, path => 
           HttpFileProcessor.Process(path));
   }
   ```

### Memory Usage

**Problem:** High memory consumption with large files.

**Solutions:**

1. **Process Files On-Demand:**
   ```csharp
   // Instead of loading all files at startup
   public IEnumerable<HttpFile> GetAllHttpFiles()
   {
       foreach (var filePath in Directory.GetFiles("HttpFiles", "*.http"))
       {
           yield return HttpFileProcessor.Process(filePath);
       }
   }
   ```

2. **Dispose Resources:**
   ```csharp
   public class HttpFileManager : IDisposable
   {
       private readonly Dictionary<string, HttpFile> _loadedFiles = new();
       
       public void Dispose()
       {
           _loadedFiles.Clear();
       }
   }
   ```

## Common Error Messages

### "Request name 'xyz' is invalid"

**Cause:** Request name contains invalid characters.

**Solution:**
```http
# ❌ Invalid characters
# @name user login!
# @name user@login
# @name user login

# ✅ Valid names
# @name user-login
# @name user_login
# @name userLogin
# @name user123
```

### "Variable 'xyz' not found"

**Cause:** Referenced variable is not defined.

**Solution:**
```http
# ✅ Define before use
@baseUrl = https://api.example.com
@version = v1

# ✅ Use after definition
GET {{baseUrl}}/{{version}}/users HTTP/1.1
```

### "Invalid expectation format"

**Cause:** Expectation comment syntax is incorrect.

**Solution:**
```http
# ❌ Invalid
# @expect status: 200
# @expect header Content-Type: application/json

# ✅ Valid
# @expect status 200
# @expect header Content-Type application/json
```

### "No requests found in file"

**Cause:** File contains no named requests.

**Solution:**
```http
# ✅ Add names to requests
# @name test-request
GET https://api.example.com/test HTTP/1.1
```

## Debugging Tips

### Enable Detailed Logging

```csharp
// Configure logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Use logger in processing
var logger = serviceProvider.GetService<ILogger<HttpFileProcessor>>();
var httpFile = HttpFileProcessor.Process(filePath, logger);
```

### Inspect Parsed Objects

```csharp
// Debug parsed HTTP file
var httpFile = HttpFileProcessor.Process("test.http");

Console.WriteLine($"Variables: {httpFile.Variables.Count}");
foreach (var variable in httpFile.Variables)
{
    Console.WriteLine($"  {variable.Name} = {variable.Value}");
}

Console.WriteLine($"Requests: {httpFile.Requests.Count}");
foreach (var request in httpFile.Requests)
{
    Console.WriteLine($"  {request.Metadata.Name}: {request.Method} {request.Url}");
    Console.WriteLine($"    Headers: {request.Headers.Count}");
    Console.WriteLine($"    Expectations: {request.Metadata.Expectations.Count}");
}
```

### Validate HTTP Syntax

```csharp
// Validate individual requests
foreach (var request in httpFile.Requests)
{
    // Check required components
    if (string.IsNullOrEmpty(request.Method))
        Console.WriteLine($"Request {request.Metadata.Name}: Missing HTTP method");
    
    if (string.IsNullOrEmpty(request.Url))
        Console.WriteLine($"Request {request.Metadata.Name}: Missing URL");
    
    // Validate expectations
    foreach (var expectation in request.Metadata.Expectations)
    {
        Console.WriteLine($"Request {request.Metadata.Name}: {expectation.Type} = {expectation.Value}");
    }
}
```

### Test Variable Resolution

```csharp
// Test system variable generation
var testCases = new[]
{
    "{{$guid}}",
    "{{$randomInt 1 100}}",
    "{{$timestamp}}",
    "{{$datetime iso8601}}"
};

foreach (var testCase in testCases)
{
    // Create a simple HTTP file with the variable
    var content = $@"
@test = {testCase}

# @name test-request
GET https://example.com/{{{{test}}}} HTTP/1.1
";
    
    var httpFile = HttpFileProcessor.ProcessContent(content);
    var request = httpFile.GetRequestByName("test-request");
    
    Console.WriteLine($"Input: {testCase}");
    Console.WriteLine($"Resolved URL: {request.Url}");
    Console.WriteLine();
}
```

## Getting Help

### Documentation Resources

- 📖 [Getting Started Guide](GETTING_STARTED.md)
- 📚 [API Reference](API_REFERENCE.md)
- 🧪 [Integration Testing Guide](INTEGRATION_TESTING.md)
- 📝 [HTTP File Reference](HTTP_FILE_REFERENCE.md)

### Community Support

- 🐛 [GitHub Issues](https://github.com/Meir017/vscode-restclient-dotnet/issues) - Bug reports and feature requests
- 💡 [GitHub Discussions](https://github.com/Meir017/vscode-restclient-dotnet/discussions) - Questions and community support
- 📦 [NuGet Package](https://www.nuget.org/packages/RESTClient.NET.Core/) - Package information and download stats

### Reporting Issues

When reporting issues, please include:

1. **Environment Information:**
   - .NET version
   - RESTClient.NET version
   - Operating system

2. **Minimal Reproduction:**
   - Sample HTTP file content
   - Code that demonstrates the issue
   - Expected vs. actual behavior

3. **Error Details:**
   - Full exception messages and stack traces
   - Log output (if available)
   - Steps to reproduce

**Example Issue Report:**

```
**Environment:**
- .NET 8.0
- RESTClient.NET.Core 1.0.0
- Windows 11

**Problem:**
Variable {{baseUrl}} not resolving in request URL.

**HTTP File:**
```http
@baseUrl = https://api.example.com

# @name test-request
GET {{baseUrl}}/users HTTP/1.1
```

**Code:**
```csharp
var httpFile = HttpFileProcessor.Process("test.http");
var request = httpFile.GetRequestByName("test-request");
Console.WriteLine(request.Url); // Expected: https://api.example.com/users
                                // Actual: {{baseUrl}}/users
```

**Expected:** URL should be resolved to `https://api.example.com/users`
**Actual:** URL remains as `{{baseUrl}}/users`
```

This format helps maintainers quickly understand and reproduce issues.
