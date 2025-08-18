# Integration Testing Guide

This guide shows you how to use RESTClient.NET for integration testing with ASP.NET Core applications.

## Table of Contents

- [Overview](#overview)
- [Setup](#setup)
- [Basic Integration Testing](#basic-integration-testing)
- [Advanced Testing Patterns](#advanced-testing-patterns)
- [Data-Driven Testing](#data-driven-testing)
- [Authentication Testing](#authentication-testing)
- [Environment Configuration](#environment-configuration)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Overview

RESTClient.NET makes integration testing easier by:
- Using HTTP files as test data sources
- Automatically validating responses against expectations
- Supporting complex scenarios with variables and assertions
- Integrating seamlessly with ASP.NET Core's `WebApplicationFactory`

## Setup

### 1. Install Required Packages

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="RESTClient.NET.Core" Version="1.0.0" />
    <PackageReference Include="RESTClient.NET.Testing" Version="1.0.0" />
    <PackageReference Include="xunit" Version="2.4.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../YourApi/YourApi.csproj" />
  </ItemGroup>
</Project>
```

### 2. Create Test HTTP Files

Create a directory structure for your HTTP files:

```
/YourApi.Tests
  /HttpFiles
    auth.http
    users.http
    products.http
  /IntegrationTests
    AuthTests.cs
    UserTests.cs
    ProductTests.cs
```

### 3. Example HTTP File

Create `HttpFiles/users.http`:

```http
@baseUrl = http://localhost:5000
@contentType = application/json

# @name get-all-users
# @expect status 200
# @expect header Content-Type application/json
GET {{baseUrl}}/api/users HTTP/1.1
Accept: {{contentType}}

###

# @name get-user-by-id
# @expect status 200
# @expect body-path $.id
# @expect body-path $.email
GET {{baseUrl}}/api/users/1 HTTP/1.1
Accept: {{contentType}}

###

# @name create-user
# @expect status 201
# @expect header Location
# @expect body-path $.id
POST {{baseUrl}}/api/users HTTP/1.1
Content-Type: {{contentType}}

{
  "name": "John Doe",
  "email": "john.doe+{{$randomInt 1000 9999}}@example.com",
  "createdAt": "{{$datetime iso8601}}"
}

###

# @name update-user
# @expect status 200
# @expect body-path $.id
PUT {{baseUrl}}/api/users/1 HTTP/1.1
Content-Type: {{contentType}}

{
  "name": "Jane Doe",
  "email": "jane.doe@example.com"
}

###

# @name delete-user
# @expect status 204
DELETE {{baseUrl}}/api/users/1 HTTP/1.1

###

# @name get-nonexistent-user
# @expect status 404
# @expect body-contains "User not found"
GET {{baseUrl}}/api/users/999999 HTTP/1.1
Accept: {{contentType}}
```

## Basic Integration Testing

### 1. Simple Test Class

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using RESTClient.NET.Core;
using System.Text.Json;
using Xunit;

public class UserIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpFileParser _parser;

    public UserIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _parser = new HttpFileParser();
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var httpFile = await _parser.ParseAsync("HttpFiles/users.http");
        var request = httpFile.GetRequestByName("get-all-users");

        // Act
        var httpRequest = CreateHttpRequestMessage(request);
        var response = await client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/json", contentType);
    }

    private HttpRequestMessage CreateHttpRequestMessage(HttpRequest request)
    {
        var httpRequest = new HttpRequestMessage(
            new HttpMethod(request.Method),
            request.Url.Replace("http://localhost:5000", ""));

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                continue; // Content-Type is set on content, not request

            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (!string.IsNullOrEmpty(request.Body))
        {
            httpRequest.Content = new StringContent(
                request.Body,
                System.Text.Encoding.UTF8,
                request.Headers.GetValueOrDefault("Content-Type", "application/json"));
        }

        return httpRequest;
    }
}
```

### 2. Using HttpFileTestBase

```csharp
using RESTClient.NET.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class UserIntegrationTests : HttpFileTestBase<Program>
{
    protected override string GetHttpFilePath() => "HttpFiles/users.http";

    protected override void ModifyHttpFile(HttpFile httpFile)
    {
        // Set the base URL to the test server
        var baseAddress = Factory.Server.BaseAddress.ToString().TrimEnd('/');
        httpFile.SetVariable("baseUrl", baseAddress);
    }

    [Theory]
    [MemberData(nameof(GetUserTestCases))]
    public async Task ExecuteUserRequest_ShouldMatchExpectations(string requestName)
    {
        // Arrange
        var request = HttpFile.GetRequestByName(requestName);
        var client = Factory.CreateClient();

        // Act
        var httpRequest = CreateHttpRequestMessage(request);
        var response = await client.SendAsync(httpRequest);

        // Assert
        await AssertResponse(response, request);
    }

    public static IEnumerable<object[]> GetUserTestCases()
    {
        var parser = new HttpFileParser();
        var httpFile = parser.ParseAsync("HttpFiles/users.http").Result;
        
        return httpFile.Requests
            .Where(r => !r.Metadata.Name.Contains("create")) // Skip create for this example
            .Select(r => new object[] { r.Metadata.Name });
    }
}
```

## Advanced Testing Patterns

### 1. Sequential Request Testing

Some tests require executing requests in sequence (e.g., create then read):

```csharp
[Fact]
public async Task CreateThenReadUser_ShouldWork()
{
    var client = Factory.CreateClient();
    var httpFile = await _parser.ParseAsync("HttpFiles/users.http");

    // Step 1: Create user
    var createRequest = httpFile.GetRequestByName("create-user");
    var createHttpRequest = CreateHttpRequestMessage(createRequest);
    var createResponse = await client.SendAsync(createHttpRequest);
    
    Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    
    var locationHeader = createResponse.Headers.Location?.ToString();
    Assert.NotNull(locationHeader);

    // Step 2: Read the created user
    var userId = ExtractUserIdFromLocation(locationHeader);
    var readRequest = httpFile.GetRequestByName("get-user-by-id");
    
    // Modify the URL to use the actual user ID
    var readHttpRequest = CreateHttpRequestMessage(readRequest);
    readHttpRequest.RequestUri = new Uri(readHttpRequest.RequestUri.ToString().Replace("/1", $"/{userId}"));
    
    var readResponse = await client.SendAsync(readHttpRequest);
    Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
}

private string ExtractUserIdFromLocation(string location)
{
    // Extract user ID from location header
    // Example: /api/users/123 -> 123
    return location.Split('/').Last();
}
```

### 2. Database Seeding and Cleanup

```csharp
public class UserIntegrationTests : HttpFileTestBase<Program>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private readonly ApplicationDbContext _dbContext;

    public UserIntegrationTests()
    {
        _scope = Factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected override string GetHttpFilePath() => "HttpFiles/users.http";

    protected override void ModifyHttpFile(HttpFile httpFile)
    {
        var baseAddress = Factory.Server.BaseAddress.ToString().TrimEnd('/');
        httpFile.SetVariable("baseUrl", baseAddress);
    }

    public async Task InitializeAsync()
    {
        // Seed test data
        await _dbContext.Users.AddAsync(new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com"
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        // Cleanup
        _dbContext.Users.RemoveRange(_dbContext.Users);
        await _dbContext.SaveChangesAsync();
        _scope.Dispose();
    }

    [Fact]
    public async Task GetSeededUser_ShouldReturnUser()
    {
        var request = HttpFile.GetRequestByName("get-user-by-id");
        var client = Factory.CreateClient();

        var httpRequest = CreateHttpRequestMessage(request);
        var response = await client.SendAsync(httpRequest);

        await AssertResponse(response, request);
    }
}
```

## Data-Driven Testing

### 1. Test All Requests Automatically

```csharp
[Theory]
[MemberData(nameof(GetAllTestCases))]
public async Task ExecuteAllRequests_ShouldMatchExpectations(string fileName, string requestName)
{
    // Arrange
    var httpFile = await _parser.ParseAsync($"HttpFiles/{fileName}");
    var request = httpFile.GetRequestByName(requestName);
    var client = Factory.CreateClient();

    // Apply test server base URL
    ModifyRequestForTesting(request);

    // Act
    var httpRequest = CreateHttpRequestMessage(request);
    var response = await client.SendAsync(httpRequest);

    // Assert
    await AssertResponse(response, request);
}

public static IEnumerable<object[]> GetAllTestCases()
{
    var httpFiles = new[] { "users.http", "products.http", "auth.http" };
    var parser = new HttpFileParser();

    foreach (var fileName in httpFiles)
    {
        var httpFile = parser.ParseAsync($"HttpFiles/{fileName}").Result;
        foreach (var request in httpFile.Requests)
        {
            yield return new object[] { fileName, request.Metadata.Name };
        }
    }
}
```

### 2. Parameterized Testing

```http
# In your HTTP file
@userId = 1
@userName = TestUser

# @name get-user-parameterized
# @expect status 200
# @expect body-contains {{userName}}
GET {{baseUrl}}/api/users/{{userId}} HTTP/1.1
Accept: application/json
```

```csharp
[Theory]
[InlineData(1, "TestUser")]
[InlineData(2, "AnotherUser")]
public async Task GetUser_WithDifferentParameters_ShouldWork(int userId, string userName)
{
    var httpFile = await _parser.ParseAsync("HttpFiles/users.http");
    
    // Set parameters
    httpFile.SetVariable("userId", userId.ToString());
    httpFile.SetVariable("userName", userName);
    
    var request = httpFile.GetRequestByName("get-user-parameterized");
    var client = Factory.CreateClient();

    var httpRequest = CreateHttpRequestMessage(request);
    var response = await client.SendAsync(httpRequest);

    await AssertResponse(response, request);
}
```

## Authentication Testing

### 1. JWT Authentication

```http
# auth.http
@baseUrl = http://localhost:5000

# @name login
# @expect status 200
# @expect body-path $.token
POST {{baseUrl}}/auth/login HTTP/1.1
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "admin123"
}

###

# @name get-protected-resource
# @expect status 200
GET {{baseUrl}}/api/admin/users HTTP/1.1
Authorization: Bearer {{token}}
Accept: application/json
```

```csharp
[Fact]
public async Task AccessProtectedResource_WithValidToken_ShouldSucceed()
{
    var client = Factory.CreateClient();
    var httpFile = await _parser.ParseAsync("HttpFiles/auth.http");

    // Step 1: Login to get token
    var loginRequest = httpFile.GetRequestByName("login");
    var loginHttpRequest = CreateHttpRequestMessage(loginRequest);
    var loginResponse = await client.SendAsync(loginHttpRequest);
    
    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    
    var loginContent = await loginResponse.Content.ReadAsStringAsync();
    var loginJson = JsonDocument.Parse(loginContent);
    var token = loginJson.RootElement.GetProperty("token").GetString();

    // Step 2: Use token to access protected resource
    var protectedRequest = httpFile.GetRequestByName("get-protected-resource");
    var protectedHttpRequest = CreateHttpRequestMessage(protectedRequest);
    
    // Replace the token placeholder
    protectedHttpRequest.Headers.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    var protectedResponse = await client.SendAsync(protectedHttpRequest);
    Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
}
```

### 2. Test Authentication Handler

For easier testing, you can create a test authentication handler:

```csharp
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// In test setup
public class AuthenticatedTestsFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", options => { });
        });
    }
}
```

## Environment Configuration

### 1. Multiple Environment Testing

```csharp
public class MultiEnvironmentTests
{
    [Theory]
    [InlineData("development")]
    [InlineData("staging")]
    public async Task TestApi_InDifferentEnvironments(string environment)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(environment);
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile($"appsettings.{environment}.json", optional: true);
                });
            });

        var client = factory.CreateClient();
        var httpFile = await new HttpFileParser().ParseAsync("HttpFiles/users.http");
        
        // Set environment-specific base URL
        var baseUrl = GetBaseUrlForEnvironment(environment);
        httpFile.SetVariable("baseUrl", baseUrl);

        // Execute tests...
    }

    private string GetBaseUrlForEnvironment(string environment)
    {
        return environment switch
        {
            "development" => "http://localhost:5000",
            "staging" => "https://staging-api.example.com",
            _ => throw new ArgumentException($"Unknown environment: {environment}")
        };
    }
}
```

### 2. Configuration Override

```csharp
public class ConfigurationOverrideTests : HttpFileTestBase<Program>
{
    protected override WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=true;",
                        ["ApiSettings:MaxPageSize"] = "10",
                        ["Logging:LogLevel:Default"] = "Warning"
                    });
                });
            });
    }

    protected override string GetHttpFilePath() => "HttpFiles/users.http";
}
```

## Best Practices

### 1. Organize HTTP Files by Feature

```
/HttpFiles
  /Users
    crud.http
    validation.http
    authentication.http
  /Products
    catalog.http
    search.http
    reviews.http
  /Orders
    checkout.http
    payment.http
    fulfillment.http
```

### 2. Use Descriptive Request Names

```http
# Good
# @name user-create-valid-data
# @name user-create-duplicate-email
# @name user-get-by-valid-id
# @name user-get-by-invalid-id

# Avoid
# @name test1
# @name request2
# @name api-call
```

### 3. Include Comprehensive Expectations

```http
# @name create-user-comprehensive
# @expect status 201
# @expect header Content-Type application/json
# @expect header Location
# @expect body-path $.id
# @expect body-path $.email
# @expect body-path $.createdAt
# @expect body-contains "success"
POST {{baseUrl}}/api/users HTTP/1.1
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com"
}
```

### 4. Use Fixtures for Complex Scenarios

```csharp
public class UserTestFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; private set; }
    public ApplicationDbContext DbContext { get; private set; }
    public List<User> TestUsers { get; private set; }

    public async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>();
        
        var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Seed test data
        TestUsers = new List<User>
        {
            new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
        };
        
        DbContext.Users.AddRange(TestUsers);
        await DbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        DbContext?.Dispose();
        await Factory?.DisposeAsync();
    }
}

public class UserIntegrationTests : IClassFixture<UserTestFixture>
{
    private readonly UserTestFixture _fixture;

    public UserIntegrationTests(UserTestFixture fixture)
    {
        _fixture = fixture;
    }

    // Tests use _fixture.Factory and _fixture.TestUsers
}
```

## Troubleshooting

### Common Issues

#### 1. Base URL Resolution

**Problem**: Tests fail because URLs in HTTP files don't match test server URLs.

**Solution**: Always override the base URL in tests:

```csharp
protected override void ModifyHttpFile(HttpFile httpFile)
{
    var testServerUrl = Factory.Server.BaseAddress.ToString().TrimEnd('/');
    httpFile.SetVariable("baseUrl", testServerUrl);
}
```

#### 2. Database State Issues

**Problem**: Tests fail due to database state from previous tests.

**Solution**: Use proper cleanup and isolation:

```csharp
public async Task InitializeAsync()
{
    // Clear database
    await _dbContext.Database.EnsureDeletedAsync();
    await _dbContext.Database.EnsureCreatedAsync();
    
    // Seed test data
    await SeedTestData();
}
```

#### 3. Authentication in Tests

**Problem**: Protected endpoints return 401 in tests.

**Solution**: Use test authentication handlers or properly authenticate:

```csharp
// Option 1: Test authentication handler (recommended)
services.AddAuthentication("Test")
    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

// Option 2: Real authentication in tests
var token = await GetAuthTokenAsync();
httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
```

#### 4. Request Body Issues

**Problem**: POST/PUT requests fail due to content type issues.

**Solution**: Ensure proper content type handling:

```csharp
private HttpRequestMessage CreateHttpRequestMessage(HttpRequest request)
{
    var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), request.Url);

    foreach (var header in request.Headers)
    {
        if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            continue; // Handle separately
            
        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
    }

    if (!string.IsNullOrEmpty(request.Body))
    {
        var contentType = request.Headers.GetValueOrDefault("Content-Type", "application/json");
        httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, contentType);
    }

    return httpRequest;
}
```

### Debugging Tips

#### 1. Enable Detailed Logging

```csharp
var factory = new WebApplicationFactory<Program>()
    .WithWebHostBuilder(builder =>
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });
    });
```

#### 2. Inspect HTTP Requests

```csharp
[Fact]
public async Task DebugRequest()
{
    var request = HttpFile.GetRequestByName("debug-request");
    var httpRequest = CreateHttpRequestMessage(request);
    
    // Log request details
    _output.WriteLine($"Method: {httpRequest.Method}");
    _output.WriteLine($"URI: {httpRequest.RequestUri}");
    _output.WriteLine("Headers:");
    foreach (var header in httpRequest.Headers)
    {
        _output.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
    }
    
    if (httpRequest.Content != null)
    {
        var content = await httpRequest.Content.ReadAsStringAsync();
        _output.WriteLine($"Body: {content}");
    }
}
```

#### 3. Test Individual Requests

```csharp
[Theory]
[InlineData("get-all-users")]
[InlineData("get-user-by-id")]
[InlineData("create-user")]
public async Task TestIndividualRequest(string requestName)
{
    var request = HttpFile.GetRequestByName(requestName);
    
    // Test just this request with detailed assertions
    // ...
}
```

This comprehensive integration testing guide should help you implement robust API testing using RESTClient.NET with ASP.NET Core applications.
