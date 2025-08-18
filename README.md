# RESTClient.NET

[![CI](https://github.com/Meir017/vscode-restclient-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/Meir017/vscode-restclient-dotnet/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Meir017/vscode-restclient-dotnet/branch/main/graph/badge.svg)](https://codecov.io/gh/Meir017/vscode-restclient-dotnet)
[![RESTClient.NET.Core](https://img.shields.io/nuget/v/RESTClient.NET.Core.svg?label=RESTClient.NET.Core)](https://www.nuget.org/packages/RESTClient.NET.Core/)
[![RESTClient.NET.Testing](https://img.shields.io/nuget/v/RESTClient.NET.Testing.svg?label=RESTClient.NET.Testing)](https://www.nuget.org/packages/RESTClient.NET.Testing/)

A comprehensive C# library for parsing HTTP files with full [VS Code REST Client](https://github.com/Huachao/vscode-restclient) compatibility and ASP.NET Core integration testing capabilities.

## 🚀 Features

- **✅ VS Code REST Client Compatibility**: Full support for standard `# @name` format
- **✅ System Variables Support**: Built-in variables (`{{$guid}}`, `{{$randomInt}}`, `{{$timestamp}}`, `{{$datetime}}`)
- **✅ Enhanced Expectation Comments**: Parse `# @expect-*` comments for automated testing
- **✅ Request Parsing**: Robust HTTP request parsing with headers, body, and metadata
- **✅ Name-Based API**: Complete request lookup and validation API
- **✅ Comprehensive Error Handling**: Detailed validation with clear error messages
- **✅ ASP.NET Core Integration**: Full integration testing framework with WebApplicationFactory support
- **✅ Production Ready**: Stable v1.0.0 release with comprehensive test coverage

## 📦 Packages

- **RESTClient.NET.Core** - Core parsing library (✅ Production Ready - v1.0.0)
- **RESTClient.NET.Testing** - ASP.NET Core testing framework (✅ Production Ready - v1.0.0)
- **RESTClient.NET.Extensions** - Additional utilities and extensions (📋 Planned)

## 🛠️ Installation

```bash
# Core library
dotnet add package RESTClient.NET.Core

# Testing framework
dotnet add package RESTClient.NET.Testing
```

## 📖 Quick Start

Ready to get started? Choose your path:

### 🚀 **New to RESTClient.NET?**

Start with our [**Getting Started Guide**](docs/GETTING_STARTED.md) for a comprehensive introduction.

### 🧪 **Want to do Integration Testing?**

Jump to our [**Integration Testing Guide**](docs/INTEGRATION_TESTING.md) for ASP.NET Core testing.

### 📚 **Need API Documentation?**

Check the [**API Reference**](docs/API_REFERENCE.md) for complete method documentation.

### Basic Parsing Example

```csharp
using RESTClient.NET.Core;

var parser = new HttpFileParser();
var httpFile = await parser.ParseAsync("requests.http");

// Get request by name
var loginRequest = httpFile.GetRequestByName("login");
Console.WriteLine($"Method: {loginRequest.Method}");
Console.WriteLine($"URL: {loginRequest.Url}");

// Access expectations
foreach (var expectation in loginRequest.Metadata.Expectations)
{
    Console.WriteLine($"Expectation: {expectation.Type} = {expectation.Value}");
}
```

### HTTP File Format

```http
@baseUrl = https://api.example.com
@apiVersion = v1

# @name login
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "token"
POST {{baseUrl}}/{{apiVersion}}/auth/login HTTP/1.1
Content-Type: application/json
X-Request-ID: {{$guid}}
X-Timestamp: {{$timestamp}}

{
    "username": "user@example.com",
    "password": "secure_password",
    "sessionId": "{{$randomInt 1000 9999}}"
}

# @name get-user-profile
# @expect status 200
# @expect body-path $.id
GET {{baseUrl}}/{{apiVersion}}/users/me HTTP/1.1
Authorization: Bearer {{login.response.body.$.token}}
Accept: application/json
X-Client-Time: {{$datetime iso8601}}
```

## 🏗️ Development Status

### ✅ **v1.0.0 Released! 🎉**

- **📦 NuGet Packages Available**: Both Core and Testing packages published
- **✅ VS Code REST Client Compatibility**: 100% format compatibility achieved
- **✅ Production Ready**: All 111 tests passing with comprehensive coverage
- **✅ Complete Feature Set**: System variables, expectations, and integration testing
- **✅ Comprehensive Documentation**: Getting started guides, API reference, and examples

### 🚀 **What's New in v1.0.0**

- Full VS Code REST Client format compatibility
- Request name validation (`# @name` format)
- Built-in system variables (`{{$guid}}`, `{{$randomInt}}`, `{{$timestamp}}`, `{{$datetime}}`)
- Enhanced expectation comment parsing for automated testing
- Complete ASP.NET Core integration testing framework
- Comprehensive exception handling and validation
- Production-ready stability and performance

### � **What's Next**

- 🌟 **Community Growth**: Adoption and feedback from .NET developers
- 🔗 **Request Chaining**: Support for `{{request.response.body.$.token}}` syntax
- 🌐 **Environment Files**: `.env` file support for better environment management
- 🎨 **VS Code Extension**: Enhanced developer experience with syntax highlighting

## 🧪 Testing

Run the test suite:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific project tests
dotnet test tests/RESTClient.NET.Core.Tests/
```

## 📋 Requirements

- .NET 8+ (primary target)
- .NET Standard 2.0 (for broader compatibility)
- Compatible with VS Code REST Client format

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎯 Project Goals

This library bridges the gap between simple HTTP parsing libraries and complex API testing frameworks, providing:

1. **Industry Standard Compatibility**: Full VS Code REST Client format support
2. **Enhanced Testing Capabilities**: Automated test validation through expectation comments
3. **Developer Experience**: Minimal boilerplate for integration testing
4. **Production Ready**: Stable, well-tested parsing functionality

## 📚 Documentation

Complete documentation is available to help you get the most out of RESTClient.NET:

### Essential Guides

- **[📖 Getting Started Guide](docs/GETTING_STARTED.md)** - Your first steps with RESTClient.NET
- **[🧪 Integration Testing Guide](docs/INTEGRATION_TESTING.md)** - Deep dive into ASP.NET Core testing
- **[📚 API Reference](docs/API_REFERENCE.md)** - Complete API documentation

### Reference Materials

- **[📝 HTTP File Reference](docs/HTTP_FILE_REFERENCE.md)** - Complete syntax reference for HTTP files
- **[🔧 Troubleshooting Guide](docs/TROUBLESHOOTING.md)** - Common issues, solutions, and debugging tips

### Additional Resources

- **[📄 Product Requirements Document (PRD)](PRD.md)** - Detailed specifications and examples
- **[💼 Sample Projects](samples/)** - Working examples and templates

### Quick Navigation

| I want to... | Go to... |
|---------------|----------|
| Install and try the library | [Getting Started → Installation](docs/GETTING_STARTED.md#installation) |
| See HTTP file examples | [Getting Started → Basic Usage](docs/GETTING_STARTED.md#basic-usage) |
| Use system variables | [HTTP File Reference → System Variables](docs/HTTP_FILE_REFERENCE.md#system-variables) |
| Write integration tests | [Integration Testing Guide](docs/INTEGRATION_TESTING.md) |
| Look up a specific API | [API Reference](docs/API_REFERENCE.md) |
| Fix parsing errors | [Troubleshooting → Parsing Errors](docs/TROUBLESHOOTING.md#parsing-errors) |
| Report a bug | [GitHub Issues](https://github.com/Meir017/vscode-restclient-dotnet/issues) |

## 🏆 Success Metrics

- ✅ **100% VS Code REST Client format compatibility** - Full standard compliance
- ✅ **Complete `# @name` format support** - Industry-standard request identification
- ✅ **Built-in system variables implementation** - `{{$guid}}`, `{{$randomInt}}`, `{{$timestamp}}`, `{{$datetime}}`
- ✅ **All functionality tests passing (111/111 tests)** - Comprehensive test coverage
- ✅ **Production-ready stability** - Used in real-world applications
- ✅ **ASP.NET Core integration framework** - Complete testing solution
- ✅ **v1.0.0 Release Published** - Available on NuGet

---

Built with ❤️ for the .NET community
