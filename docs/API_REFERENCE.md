# API Reference

This document provides a complete reference for the RESTClient.NET API.

## RESTClient.NET.Core

### HttpFileParser

The main entry point for parsing HTTP files.

#### Constructor

```csharp
public HttpFileParser()
public HttpFileParser(ILogger<HttpFileParser>? logger = null)
```

#### Methods

##### ParseAsync

```csharp
public async Task<HttpFile> ParseAsync(string filePath)
public async Task<HttpFile> ParseAsync(Stream stream)
public async Task<HttpFile> ParseAsync(TextReader reader)
```

Parses an HTTP file and returns an `HttpFile` object containing all requests and variables.

**Parameters:**
- `filePath`: Path to the HTTP file
- `stream`: Stream containing HTTP file content
- `reader`: TextReader containing HTTP file content

**Returns:** `HttpFile` object

**Throws:**
- `HttpParseException`: When parsing fails
- `FileNotFoundException`: When file is not found
- `ArgumentException`: When arguments are invalid

### HttpFile

Represents a parsed HTTP file containing requests and variables.

#### Properties

```csharp
public IReadOnlyList<HttpRequest> Requests { get; }
public IReadOnlyList<VariableDefinition> Variables { get; }
```

#### Methods

##### GetRequestByName

```csharp
public HttpRequest GetRequestByName(string name)
```

Gets a request by its name. Throws `KeyNotFoundException` if not found.

##### TryGetRequestByName

```csharp
public bool TryGetRequestByName(string name, out HttpRequest request)
```

Tries to get a request by name. Returns `false` if not found.

### HttpRequest

Represents a single HTTP request.

#### Properties

```csharp
public string Method { get; }
public string Url { get; }
public IReadOnlyDictionary<string, string> Headers { get; }
public string? Body { get; }
public HttpRequestMetadata Metadata { get; }
```

### HttpRequestMetadata

Contains metadata about an HTTP request.

#### Properties

```csharp
public string Name { get; }
public IReadOnlyList<ExpectationComment> Expectations { get; }
```

### VariableDefinition

Represents a variable definition in an HTTP file.

#### Properties

```csharp
public string Name { get; }
public string Value { get; }
```

### ExpectationComment

Represents an expectation comment for testing.

#### Properties

```csharp
public string Type { get; }
public string Value { get; }
```

## RESTClient.NET.Testing

### HttpFileTestBase<TProgram>

Base class for HTTP file-driven integration tests.

#### Constructor

```csharp
protected HttpFileTestBase()
```

#### Properties

```csharp
protected WebApplicationFactory<TProgram> Factory { get; }
protected HttpFile HttpFile { get; }
```

#### Methods

##### GetHttpFilePath

```csharp
protected abstract string GetHttpFilePath();
```

Override this method to specify the path to your HTTP file.

##### ModifyHttpFile

```csharp
protected virtual void ModifyHttpFile(HttpFile httpFile)
```

Override this method to modify the HTTP file before tests run (e.g., set variables).

##### CreateHttpRequestMessage

```csharp
protected virtual HttpRequestMessage CreateHttpRequestMessage(HttpRequest request)
```

Creates an `HttpRequestMessage` from an `HttpRequest`.

##### AssertResponse

```csharp
protected virtual async Task AssertResponse(HttpResponseMessage response, HttpRequest request)
```

Asserts that the response matches the request's expectations.

## System Variables

RESTClient.NET supports several built-in system variables:

### {{$guid}}

Generates a new GUID (UUID v4).

```http
X-Request-ID: {{$guid}}
```

### {{$randomInt min max}}

Generates a random integer between min and max (inclusive).

```http
{
  "id": {{$randomInt 1000 9999}}
}
```

### {{$timestamp}}

Generates the current Unix timestamp.

```http
X-Timestamp: {{$timestamp}}
```

### {{$datetime format}}

Generates the current date/time in the specified format.

Supported formats:
- `iso8601`: ISO 8601 format (2023-01-01T12:00:00.000Z)
- `rfc1123`: RFC 1123 format (Sun, 01 Jan 2023 12:00:00 GMT)
- Custom .NET DateTime format strings

```http
{
  "createdAt": "{{$datetime iso8601}}",
  "lastModified": "{{$datetime yyyy-MM-dd HH:mm:ss}}"
}
```

## Expectation Comments

Expectation comments are used to define test assertions.

### @expect status

Expects a specific HTTP status code.

```http
# @expect status 200
# @expect status 201
# @expect status 404
```

### @expect header

Expects a specific header to be present.

```http
# @expect header Content-Type
# @expect header Content-Type application/json
# @expect header Location
```

### @expect body-contains

Expects the response body to contain a specific string.

```http
# @expect body-contains "success"
# @expect body-contains "token"
```

### @expect body-path

Expects a specific JSON path to exist in the response body.

```http
# @expect body-path $.data
# @expect body-path $.data.id
# @expect body-path $.errors[0].message
```

## Error Handling

### HttpParseException

Thrown when parsing an HTTP file fails.

#### Properties

```csharp
public int LineNumber { get; }
public int ColumnNumber { get; }
public string FileName { get; }
```

#### Example

```csharp
try
{
    var httpFile = await parser.ParseAsync("requests.http");
}
catch (HttpParseException ex)
{
    Console.WriteLine($"Parse error in {ex.FileName} at line {ex.LineNumber}, column {ex.ColumnNumber}: {ex.Message}");
}
```

## Configuration

### Logging

You can provide an `ILogger` instance to enable detailed logging:

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<HttpFileParser>();
var parser = new HttpFileParser(logger);
```

### Validation

The parser includes built-in validation for:
- Request name uniqueness
- Valid HTTP methods
- Proper HTTP syntax
- Variable references

## Best Practices

### Error Handling

Always wrap parsing operations in try-catch blocks:

```csharp
try
{
    var httpFile = await parser.ParseAsync("requests.http");
    // Use httpFile...
}
catch (HttpParseException ex)
{
    // Handle parsing errors
}
catch (FileNotFoundException ex)
{
    // Handle missing file
}
catch (Exception ex)
{
    // Handle other errors
}
```

### Resource Management

The parser properly disposes of resources, but you can manually dispose if needed:

```csharp
using var fileStream = File.OpenRead("requests.http");
var httpFile = await parser.ParseAsync(fileStream);
```

### Thread Safety

`HttpFileParser` is thread-safe and can be used as a singleton. The parsed `HttpFile` and its components are immutable and thread-safe.

### Performance

For better performance with large files:
- Use `Stream` or `TextReader` overloads when possible
- Cache parsed `HttpFile` instances if parsing the same file multiple times
- Consider using async/await patterns properly

## Migration Guide

### From 0.x to 1.0

1. **Namespace Changes**: No breaking namespace changes
2. **API Changes**: All APIs are backward compatible
3. **New Features**: System variables and enhanced expectations are available
4. **Dependencies**: Minimum .NET version remains .NET Standard 2.0

### Upgrading NuGet Packages

```xml
<!-- Before -->
<PackageReference Include="RESTClient.NET.Core" Version="0.9.0" />

<!-- After -->
<PackageReference Include="RESTClient.NET.Core" Version="1.0.0" />
```

## Examples

### Complete Integration Test Example

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using RESTClient.NET.Core;
using RESTClient.NET.Testing;
using System.Net;
using Xunit;

public class ApiIntegrationTests : HttpFileTestBase<Program>
{
    protected override string GetHttpFilePath() => "test-requests.http";

    protected override void ModifyHttpFile(HttpFile httpFile)
    {
        // Set base URL to test server
        httpFile.SetVariable("baseUrl", Factory.Server.BaseAddress.ToString().TrimEnd('/'));
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public async Task ExecuteRequest_ShouldMatchExpectations(string requestName)
    {
        // Arrange
        var request = HttpFile.GetRequestByName(requestName);
        var httpRequest = CreateHttpRequestMessage(request);
        var client = Factory.CreateClient();

        // Act
        var response = await client.SendAsync(httpRequest);

        // Assert
        await AssertResponse(response, request);
    }

    public static IEnumerable<object[]> GetTestCases()
    {
        var parser = new HttpFileParser();
        var httpFile = parser.ParseAsync("test-requests.http").Result;
        
        return httpFile.Requests
            .Select(r => new object[] { r.Metadata.Name });
    }
}
```

This example shows how to create data-driven integration tests that automatically test all requests in an HTTP file.
