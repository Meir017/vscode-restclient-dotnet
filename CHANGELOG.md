# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-08-20

### Fixed

- Resolved duplicate HttpFileTestBaseErrorHandlingTests compilation errors ([#30](https://github.com/Meir017/vscode-restclient-dotnet/pull/30))

### Testing Improvements

- Comprehensive testing infrastructure improvements ([#29](https://github.com/Meir017/vscode-restclient-dotnet/pull/29))
- Significantly improved code coverage for HttpFileParser and RESTClient.NET.Testing ([#28](https://github.com/Meir017/vscode-restclient-dotnet/pull/28))
- Comprehensive HttpFileValidator test coverage ([#27](https://github.com/Meir017/vscode-restclient-dotnet/pull/27))
- Improved code coverage from 79.3% to 87.3% ([#26](https://github.com/Meir017/vscode-restclient-dotnet/pull/26))
- Significantly improved test coverage across core components ([#25](https://github.com/Meir017/vscode-restclient-dotnet/pull/25))

### Features

- File body support for VS Code REST Client compatibility ([#24](https://github.com/Meir017/vscode-restclient-dotnet/pull/24))

### Documentation

- Enhanced XML documentation and cleanup redundant files ([#23](https://github.com/Meir017/vscode-restclient-dotnet/pull/23))

## [1.0.0] - 2025-08-19

### Major Release

This represents the first stable release of RESTClient.NET with comprehensive VS Code REST Client compatibility.

### Core Features

- Complete HTTP file parsing compatible with VS Code REST Client format
- Enhanced `# @name` metadata for request identification
- Support for `# @expect-*` comments for automated testing
- Variable resolution and environment support
- ASP.NET Core integration testing capabilities
- Multi-targeting support (.NET 9.0 and .NET Standard 2.0)
- Comprehensive validation and error handling

## [0.2.0] - 2025-08-19

### Documentation & Project Structure

- Enhanced README with dynamic NuGet badges and integration testing examples ([#22](https://github.com/Meir017/vscode-restclient-dotnet/pull/22))
- Streamlined README and reorganized documentation structure ([#21](https://github.com/Meir017/vscode-restclient-dotnet/pull/21))
- Comprehensive documentation suite for v1.0.0 release ([#15](https://github.com/Meir017/vscode-restclient-dotnet/pull/15))
- NuGet badges and VS Code REST Client link ([#14](https://github.com/Meir017/vscode-restclient-dotnet/pull/14))

### Development Tools & Dependencies

- MCP server configuration for enhanced development tooling ([#20](https://github.com/Meir017/vscode-restclient-dotnet/pull/20))
- Migration to NuGet Central Package Management ([#19](https://github.com/Meir017/vscode-restclient-dotnet/pull/19))
- Migration from FluentAssertions to AwesomeAssertions ([#18](https://github.com/Meir017/vscode-restclient-dotnet/pull/18))

### New Features

- Request response chaining support ([#16](https://github.com/Meir017/vscode-restclient-dotnet/pull/16))

### Build & Release

- Modernized GitHub release workflow to use GitHub CLI ([#13](https://github.com/Meir017/vscode-restclient-dotnet/pull/13))

## [0.1.0] - 2025-08-17

### Release Management

- Release management prompt template ([#12](https://github.com/Meir017/vscode-restclient-dotnet/pull/12))
- Automated NuGet publishing workflow and release automation ([#11](https://github.com/Meir017/vscode-restclient-dotnet/pull/11))
- MCP integration and updated project status ([#10](https://github.com/Meir017/vscode-restclient-dotnet/pull/10))

### Project Setup & Tooling

- Initial CI/CD pipeline setup
- GitHub Actions workflows for automated testing and publishing
- Project structure and basic tooling

## [0.0.1] - 2025-08-16

### Initial Release

- Initial project setup and repository structure
- Basic .NET project scaffolding
- MIT License
- Initial README and .gitignore

### Core Components

- HttpFileProcessor - Main facade for parsing HTTP files
- HttpFileParser - Core parsing orchestrator
- HttpSyntaxParser - VS Code REST Client syntax implementation
- HttpTokenizer - HTTP file tokenization
- Basic model classes (HttpFile, HttpRequest, HttpRequestMetadata)
- Initial validation framework

---

## Release Links

- [v1.1.0](https://github.com/Meir017/vscode-restclient-dotnet/releases/tag/v1.1.0)
- [v1.0.0](https://github.com/Meir017/vscode-restclient-dotnet/releases/tag/v1.0.0)
- [v0.2.0](https://github.com/Meir017/vscode-restclient-dotnet/releases/tag/v0.2.0)
- [v0.1.0](https://github.com/Meir017/vscode-restclient-dotnet/releases/tag/v0.1.0)

## Contributing

This project follows [Conventional Commits](https://www.conventionalcommits.org/) for commit messages and [Semantic Versioning](https://semver.org/) for version numbers.

For more information on contributing, please see the project's README and documentation.
