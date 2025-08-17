# RESTClient.NET Solution

This solution contains the complete RESTClient.NET framework for HTTP file-based integration testing in .NET applications.

## Projects

### Core Library (`/src/`)
- **RESTClient.NET.Core** - Core HTTP file parsing and processing functionality
- **RESTClient.NET.Testing** - ASP.NET Core integration testing framework using WebApplicationFactory

### Unit Tests (`/tests/`)
- **RESTClient.NET.Core.Tests** - Unit tests for the core library (47 tests ✅)
- **RESTClient.NET.Testing.Tests** - Unit tests for the testing framework

### Sample Implementation (`/samples/`)
- **RESTClient.NET.Sample.Api** - Complete ASP.NET Core Web API demonstrating:
  - JWT authentication and authorization
  - User management with role-based access control
  - Product catalog CRUD operations
  - Order processing with status tracking
  - Entity Framework Core with SQLite
  - Swagger/OpenAPI documentation

- **RESTClient.NET.Sample.Tests** - Integration test project showcasing:
  - `WebApiTestFixture` extending `HttpFileTestBase<Program>`
  - HTTP file-driven test automation
  - Database seeding and test data management
  - xUnit integration with comprehensive test scenarios

## Build Status

✅ **All projects build successfully**  
✅ **Core tests pass** (47/47)  
✅ **Multi-target framework support** (.NET 8.0, .NET 9.0, .NET Standard 2.0)  
✅ **Complete sample implementation** demonstrating real-world usage

## Getting Started

```bash
# Build the entire solution
dotnet build vscode-restclient-dotnet.slnx --configuration Release

# Run core tests
dotnet test tests/RESTClient.NET.Core.Tests/

# Run the sample API
dotnet run --project samples/RESTClient.NET.Sample.Api/

# Open HTTP files in VS Code with REST Client extension
# samples/RESTClient.NET.Sample.Tests/HttpFiles/*.http
```

## Framework Capabilities

- **HTTP File Parsing**: VS Code REST Client compatible format
- **Variable Resolution**: Support for file variables and request chaining
- **Enhanced Expectations**: `@expect-status`, `@expect-header`, `@expect-body-*` comments
- **WebApplicationFactory Integration**: Self-contained testing with in-memory databases
- **Request Name-Based Testing**: Metadata-driven approach using `# @name` comments
- **Comprehensive Assertions**: Automated validation based on expectation comments

This implementation provides a complete demonstration of HTTP file-based integration testing patterns for .NET applications.
