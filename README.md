# RESTClient.NET

[![CI](https://github.com/Meir017/vscode-restclient-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/Meir017/vscode-restclient-dotnet/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Meir017/vscode-restclient-dotnet/branch/main/graph/badge.svg)](https://codecov.io/gh/Meir017/vscode-restclient-dotnet)
[![RESTClient.NET.Core](https://img.shields.io/nuget/v/RESTClient.NET.Core.svg?label=RESTClient.NET.Core)](https://www.nuget.org/packages/RESTClient.NET.Core/)
[![RESTClient.NET.Testing](https://img.shields.io/nuget/v/RESTClient.NET.Testing.svg?label=RESTClient.NET.Testing)](https://www.nuget.org/packages/RESTClient.NET.Testing/)

A comprehensive C# library for parsing HTTP files with full [VS Code REST Client](https://github.com/Huachao/vscode-restclient) compatibility and ASP.NET Core integration testing capabilities.

## ✨ Features

- **🔄 VS Code REST Client Compatibility** - Full support for standard `# @name` format
- **🎲 System Variables** - Built-in variables (`{{$guid}}`, `{{$randomInt}}`, `{{$timestamp}}`, `{{$datetime}}`)
- **✅ Enhanced Expectations** - Parse `# @expect-*` comments for automated testing
- **🔍 Name-Based API** - Complete request lookup and validation
- **🛡️ Robust Error Handling** - Detailed validation with clear error messages
- **🧪 ASP.NET Core Integration** - Full testing framework with WebApplicationFactory support

## 📦 Packages

| Package | Status | Description |
|---------|--------|-------------|
| **RESTClient.NET.Core** | ✅ v1.0.0 | Core HTTP file parsing library |
| **RESTClient.NET.Testing** | ✅ v1.0.0 | ASP.NET Core integration testing framework |

## 🚀 Quick Start

### Installation

```bash
# Core library
dotnet add package RESTClient.NET.Core

# Testing framework  
dotnet add package RESTClient.NET.Testing
```

### Basic Usage

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

### HTTP File Example
```http
@baseUrl = https://api.example.com

# @name login
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "token"
POST {{baseUrl}}/auth/login HTTP/1.1
Content-Type: application/json
X-Request-ID: {{$guid}}

{
    "username": "user@example.com",
    "password": "secure_password",
    "sessionId": "{{$randomInt 1000 9999}}"
}
```

## 📖 Documentation

### Getting Started
- **[📖 Getting Started Guide](docs/GETTING_STARTED.md)** - Complete introduction and setup
- **[🧪 Integration Testing Guide](docs/INTEGRATION_TESTING.md)** - ASP.NET Core testing patterns
- **[📚 API Reference](docs/API_REFERENCE.md)** - Complete API documentation

### Reference
- **[📝 HTTP File Reference](docs/HTTP_FILE_REFERENCE.md)** - Complete syntax reference
- **[🔧 Troubleshooting Guide](docs/TROUBLESHOOTING.md)** - Common issues and solutions
- **[💼 Sample Projects](samples/)** - Working examples and templates

### Additional Resources
- **[📋 Implementation Status](docs/IMPLEMENTATION_STATUS.md)** - Current development status
- **[🔬 Integration Testing Details](docs/INTEGRATION_TESTING_DETAILED.md)** - Comprehensive testing specifications
- **[📄 Product Requirements (PRD)](PRD.md)** - Detailed project specifications

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Build entire solution
dotnet build vscode-restclient-dotnet.slnx --configuration Release
```

## 📋 Requirements

- **.NET 8+** (primary target)
- **.NET Standard 2.0** (for broader compatibility)
- **VS Code REST Client** format compatibility

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes and add tests
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with ❤️ for the .NET community**
