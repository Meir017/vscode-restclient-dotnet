# Getting Started with RESTClient.NET

Welcome to RESTClient.NET! This guide will help you get up and running with parsing HTTP files and using them for API testing in your .NET applications.

## Table of Contents

- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [HTTP File Format](#http-file-format)
- [Core Features](#core-features)
- [Integration Testing](#integration-testing)
- [Advanced Examples](#advanced-examples)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Installation

### Prerequisites

- .NET 8.0 or later (recommended)
- .NET Standard 2.0 compatible frameworks supported
- A project where you want to parse HTTP files or perform API testing

### Install via NuGet

#### Core Library (Required)

```bash
# Package Manager Console
Install-Package RESTClient.NET.Core

# .NET CLI
dotnet add package RESTClient.NET.Core

# PackageReference in .csproj
<PackageReference Include="RESTClient.NET.Core" Version="1.0.0" />
```

#### Testing Framework (Optional)

```bash
# Package Manager Console
Install-Package RESTClient.NET.Testing

# .NET CLI  
dotnet add package RESTClient.NET.Testing

# PackageReference in .csproj
<PackageReference Include="RESTClient.NET.Testing" Version="1.0.0" />
```

## Basic Usage

### 1. Create Your First HTTP File

Create a file named `api-requests.http`:

```http
@baseUrl = https://jsonplaceholder.typicode.com

# @name get-all-posts
GET {{baseUrl}}/posts HTTP/1.1
Accept: application/json

# @name get-single-post
GET {{baseUrl}}/posts/1 HTTP/1.1
Accept: application/json

# @name create-post
# @expect status 201
# @expect header Content-Type application/json
POST {{baseUrl}}/posts HTTP/1.1
Content-Type: application/json

{
  "title": "My New Post",
  "body": "This is the content of my post",
  "userId": 1
}
```

### 2. Parse the HTTP File

```csharp
using RESTClient.NET.Core;

// Parse the HTTP file
var parser = new HttpFileParser();
var httpFile = await parser.ParseAsync("api-requests.http");

// Get a specific request by name
var getAllPostsRequest = httpFile.GetRequestByName("get-all-posts");

Console.WriteLine($"Method: {getAllPostsRequest.Method}");
Console.WriteLine($"URL: {getAllPostsRequest.Url}");
Console.WriteLine($"Headers: {string.Join(", ", getAllPostsRequest.Headers.Select(h => $"{h.Key}: {h.Value}"))}");
```

### 3. Working with Variables

```csharp
// Access file variables
foreach (var variable in httpFile.Variables)
{
    Console.WriteLine($"{variable.Name} = {variable.Value}");
}

// The baseUrl variable will be: "https://jsonplaceholder.typicode.com"
```

## HTTP File Format

RESTClient.NET fully supports the VS Code REST Client format with additional enhancements.

### Basic Structure

```http
# File variables (global to all requests)
@variable1 = value1
@variable2 = value2

# Request with metadata
# @name request-identifier
# @expect status 200
# @expect header Content-Type application/json
HTTP_METHOD {{variable1}}/endpoint HTTP/1.1
Header-Name: Header-Value

Optional request body
```

### Supported Elements

#### 1. Variables

```http
# File variables
@apiUrl = https://api.example.com
@apiVersion = v1
@timeout = 30

# Use in requests
GET {{apiUrl}}/{{apiVersion}}/users HTTP/1.1
```

#### 2. System Variables

```http
# Generate unique values
POST /api/users HTTP/1.1
Content-Type: application/json
X-Request-ID: {{$guid}}
X-Timestamp: {{$timestamp}}

{
  "id": {{$randomInt 1000 9999}},
  "createdAt": "{{$datetime iso8601}}"
}
```

Available system variables:
- `{{$guid}}` - Generate a new GUID
- `{{$randomInt min max}}` - Random integer between min and max
- `{{$timestamp}}` - Current Unix timestamp
- `{{$datetime format}}` - Current date/time in specified format

#### 3. Request Metadata

```http
# @name unique-request-identifier
# @expect status 200|201|204
# @expect header Content-Type application/json
# @expect header Location
# @expect body-contains "success"
# @expect body-path $.data.id
```

#### 4. Comments

```http
# This is a comment
// This is also a comment

# @name login-user
# This request logs in a user and returns a JWT token
POST /auth/login HTTP/1.1
```

## Core Features

### 1. Request Lookup

```csharp
var httpFile = await parser.ParseAsync("requests.http");

// Get request by name (throws if not found)
var loginRequest = httpFile.GetRequestByName("login-user");

// Try get request by name (returns false if not found)
if (httpFile.TryGetRequestByName("optional-request", out var request))
{
    // Use the request
    Console.WriteLine($"Found request: {request.Method} {request.Url}");
}

// Get all requests
foreach (var req in httpFile.Requests)
{
    Console.WriteLine($"{req.Metadata.Name}: {req.Method} {req.Url}");
}
```

### 2. Working with Expectations

```csharp
var request = httpFile.GetRequestByName("create-user");

foreach (var expectation in request.Metadata.Expectations)
{
    Console.WriteLine($"Expect {expectation.Type}: {expectation.Value}");
    
    switch (expectation.Type)
    {
        case "status":
            Console.WriteLine($"Expected status code: {expectation.Value}");
            break;
        case "header":
            Console.WriteLine($"Expected header: {expectation.Value}");
            break;
        case "body-contains":
            Console.WriteLine($"Expected body to contain: {expectation.Value}");
            break;
    }
}
```

### 3. Error Handling

```csharp
try
{
    var httpFile = await parser.ParseAsync("requests.http");
    var request = httpFile.GetRequestByName("my-request");
}
catch (HttpParseException ex)
{
    Console.WriteLine($"Parse error at line {ex.LineNumber}: {ex.Message}");
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"Request not found: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"HTTP file not found: {ex.Message}");
}
```

## Integration Testing

### Basic Integration Test Setup

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using RESTClient.NET.Core;
using RESTClient.NET.Testing;
using Xunit;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpFileParser _parser;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _parser = new HttpFileParser();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var httpFile = await _parser.ParseAsync("test-requests.http");
        var request = httpFile.GetRequestByName("get-users");

        // Act
        var httpRequest = new HttpRequestMessage(
            new HttpMethod(request.Method),
            request.Url);

        foreach (var header in request.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var response = await client.SendAsync(httpRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify against expectations
        foreach (var expectation in request.Metadata.Expectations)
        {
            switch (expectation.Type)
            {
                case "status":
                    Assert.Equal(int.Parse(expectation.Value), (int)response.StatusCode);
                    break;
                case "header":
                    Assert.True(response.Headers.Contains(expectation.Value) || 
                               response.Content.Headers.Contains(expectation.Value));
                    break;
            }
        }
    }
}
```

## Advanced Examples

### 1. Complex Variable Usage

```http
@baseUrl = https://api.example.com
@apiVersion = v2
@clientId = my-client-id

# @name oauth-token
POST {{baseUrl}}/oauth/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials&client_id={{clientId}}&scope=read:users

###

# @name get-protected-resource
# Note: In future versions, this will support response chaining
GET {{baseUrl}}/{{apiVersion}}/users/me HTTP/1.1
Authorization: Bearer TOKEN_HERE
Accept: application/json
X-API-Version: {{apiVersion}}
X-Request-ID: {{$guid}}
```

### 2. Testing Different Scenarios

```http
@apiUrl = http://localhost:5000

# @name successful-login
# @expect status 200
# @expect body-contains "token"
POST {{apiUrl}}/auth/login HTTP/1.1
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "validpassword"
}

###

# @name failed-login
# @expect status 401
# @expect body-contains "Invalid credentials"
POST {{apiUrl}}/auth/login HTTP/1.1
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "wrongpassword"
}

###

# @name create-user
# @expect status 201
# @expect header Location
# @expect body-path $.id
POST {{apiUrl}}/users HTTP/1.1
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john.doe+{{$randomInt 1000 9999}}@example.com",
  "createdAt": "{{$datetime iso8601}}"
}
```

## Best Practices

### 1. File Organization

```
/project-root
  /tests
    /http-files
      auth.http          # Authentication related requests
      users.http         # User management requests
      products.http      # Product API requests
      admin.http         # Admin operations
```

### 2. Naming Conventions

```http
# Use descriptive, kebab-case names
# @name get-user-profile
# @name create-product-review
# @name delete-user-account

# Group related requests with prefixes
# @name auth-login
# @name auth-logout
# @name auth-refresh-token

# @name user-create
# @name user-update
# @name user-delete
```

### 3. Variable Management

```http
# Keep variables at the top of the file
@baseUrl = https://api.example.com
@apiVersion = v1
@contentType = application/json

# Use descriptive variable names
@userServiceUrl = {{baseUrl}}/{{apiVersion}}/users
@authServiceUrl = {{baseUrl}}/{{apiVersion}}/auth

# Use system variables for unique values
# @name create-unique-user
POST {{userServiceUrl}} HTTP/1.1
Content-Type: {{contentType}}

{
  "id": "{{$guid}}",
  "email": "test-{{$randomInt 1000 9999}}@example.com",
  "timestamp": {{$timestamp}}
}
```

### 4. Comprehensive Testing

```http
# Test happy path
# @name user-create-success
# @expect status 201
# @expect header Content-Type application/json
# @expect body-path $.id

# Test validation errors
# @name user-create-invalid-email
# @expect status 400
# @expect body-contains "Invalid email format"

# Test authentication
# @name user-create-unauthorized
# @expect status 401
# @expect body-contains "Authentication required"
```

## Troubleshooting

### Common Issues

#### 1. File Not Found
```
Error: Could not find file 'requests.http'
```
**Solution**: Ensure the file path is correct and the file exists. Use absolute paths or ensure the working directory is correct.

#### 2. Request Not Found
```
KeyNotFoundException: Request 'my-request' not found in HTTP file
```
**Solution**: Check that the request name in `# @name my-request` matches exactly (case-sensitive).

#### 3. Parse Errors
```
HttpParseException: Invalid HTTP method on line 5
```
**Solution**: Ensure your HTTP syntax is correct. Common issues:
- Missing HTTP version (add `HTTP/1.1`)
- Invalid method names
- Malformed headers

#### 4. Variable Resolution
```
Error: Variable 'baseUrl' not found
```
**Solution**: Ensure variables are defined before they're used and use correct syntax `{{variableName}}`.

### Debugging Tips

#### 1. Enable Detailed Logging

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<HttpFileParser>();
var parser = new HttpFileParser(logger);
```

#### 2. Validate HTTP File Syntax

```csharp
try
{
    var httpFile = await parser.ParseAsync("requests.http");
    Console.WriteLine($"Successfully parsed {httpFile.Requests.Count} requests");
    
    foreach (var request in httpFile.Requests)
    {
        Console.WriteLine($"✓ {request.Metadata.Name}: {request.Method} {request.Url}");
    }
}
catch (HttpParseException ex)
{
    Console.WriteLine($"✗ Parse error: {ex.Message} at line {ex.LineNumber}");
}
```

#### 3. Inspect Parsed Content

```csharp
var httpFile = await parser.ParseAsync("requests.http");

// Check variables
Console.WriteLine("Variables:");
foreach (var variable in httpFile.Variables)
{
    Console.WriteLine($"  {variable.Name} = {variable.Value}");
}

// Check requests
Console.WriteLine("\nRequests:");
foreach (var request in httpFile.Requests)
{
    Console.WriteLine($"  {request.Metadata.Name}:");
    Console.WriteLine($"    Method: {request.Method}");
    Console.WriteLine($"    URL: {request.Url}");
    Console.WriteLine($"    Headers: {request.Headers.Count}");
    Console.WriteLine($"    Expectations: {request.Metadata.Expectations.Count}");
}
```

## Next Steps

- [Integration Testing Guide](INTEGRATION_TESTING.md) - Deep dive into testing with ASP.NET Core
- [HTTP File Reference](HTTP_FILE_REFERENCE.md) - Complete syntax reference
- [Advanced Features](ADVANCED_FEATURES.md) - System variables, expectations, and more
- [API Reference](API_REFERENCE.md) - Complete API documentation

## Need Help?

- 📖 Check the [examples directory](../samples/) for complete working examples
- 🐛 [Report issues](https://github.com/Meir017/vscode-restclient-dotnet/issues) on GitHub
- 💬 [Start a discussion](https://github.com/Meir017/vscode-restclient-dotnet/discussions) for questions and ideas
- 📚 Read the [Product Requirements Document](../PRD.md) for detailed specifications
