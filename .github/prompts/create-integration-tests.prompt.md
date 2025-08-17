---
description: "Create comprehensive integration tests using RESTClient.NET.Testing"
mode: agent  
tools: ["github", "vscode", "filesystem"]
---

# Create Integration Tests

You are tasked with creating integration tests for an ASP.NET Core API using RESTClient.NET.Testing framework.

## Context

RESTClient.NET.Testing provides a framework for creating integration tests that use `.http` files to define test scenarios. The framework integrates with ASP.NET Core's `WebApplicationFactory` for testing.

## Current Project Structure

- **API**: `samples/RESTClient.NET.Sample.Api/` - The API to test
- **Tests**: `samples/RESTClient.NET.Sample.Tests/` - Integration test project
- **HTTP Files**: `samples/RESTClient.NET.Sample.Tests/HttpFiles/` - Test scenario definitions

## Task Requirements

### 1. Analyze the API
- Review the controllers in `samples/RESTClient.NET.Sample.Api/Controllers/`
- Understand the data models in `samples/RESTClient.NET.Sample.Api/Models/`
- Identify the API endpoints and their expected behavior

### 2. Create HTTP Test Files
- Design comprehensive test scenarios in `.http` files
- Use `# @name` comments for test identification
- Include `# @expect-*` comments for automated assertions
- Cover happy path, error cases, and edge cases
- Use variables for test data management

### 3. Implement Test Classes
- Extend `HttpFileTestBase<TStartup>` for the test class
- Use `WebApplicationFactory` for test server setup
- Implement test methods that use `ExecuteRequestAsync()`
- Add custom assertions using FluentAssertions
- Handle test data setup and cleanup

### 4. Test Scenarios to Cover
- **Authentication/Authorization** - Login, token validation, access control
- **CRUD Operations** - Create, read, update, delete for each entity
- **Validation** - Input validation, business rule validation
- **Error Handling** - 404, 400, 500 responses
- **Performance** - Response times for critical endpoints

## Implementation Pattern

```csharp
public class ApiIntegrationTests : HttpFileTestBase<Program>
{
    public ApiIntegrationTests(WebApplicationFactory<Program> factory) 
        : base(factory, "HttpFiles/api-tests.http")
    {
    }

    [Fact]
    public async Task ExecuteLoginTest()
    {
        var result = await ExecuteRequestAsync("login-user");
        
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        // Additional assertions...
    }
}
```

## HTTP File Pattern

```http
@baseUrl = {{$dotenv HOST}}/api
@authToken = 

### Login user
# @name login-user
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "token"
POST {{baseUrl}}/auth/login
Content-Type: application/json

{
    "username": "testuser",
    "password": "testpass"
}

### Get user profile (authenticated)
# @name get-user-profile  
# @expect status 200
# @expect header Content-Type application/json
GET {{baseUrl}}/users/me
Authorization: Bearer {{authToken}}
```

## Implementation Steps

1. **API Analysis**: Review existing controllers and models
2. **Test Planning**: Design test scenarios covering all endpoints
3. **HTTP Files**: Create `.http` files with comprehensive test cases
4. **Test Classes**: Implement integration test classes
5. **Assertions**: Add detailed assertions for responses
6. **Test Data**: Set up test data and cleanup procedures
7. **Documentation**: Add usage examples and best practices

## Files to Create/Modify

- `samples/RESTClient.NET.Sample.Tests/HttpFiles/{feature}-tests.http`
- `samples/RESTClient.NET.Sample.Tests/IntegrationTests/{Feature}IntegrationTests.cs`
- `samples/RESTClient.NET.Sample.Tests/TestFixtures/` (if new fixtures needed)

## Success Criteria

- [ ] All API endpoints have corresponding test scenarios
- [ ] Tests cover both success and failure cases
- [ ] HTTP files use proper `# @name` and `# @expect-*` syntax
- [ ] Test classes follow established patterns
- [ ] All tests pass consistently
- [ ] Test data is properly managed (setup/cleanup)
- [ ] Documentation includes usage examples

Please specify which API endpoints or features you'd like to create integration tests for.
