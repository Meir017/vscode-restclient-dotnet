# RESTClient.NET Copilot Instructions

## Project Overview

RESTClient.NET is a C# library that parses VS Code REST Client (`.http`) files and provides integration testing capabilities for ASP.NET Core applications. The project follows a metadata-driven architecture centered on the `# @name` format for request identification.

## Architecture Components

### Core Library Structure (`src/RESTClient.NET.Core/`)

- **HttpFileProcessor**: Main facade entry point for parsing HTTP files
- **HttpFileParser**: Core parsing orchestrator that coordinates tokenization and syntax parsing  
- **HttpSyntaxParser**: Implements VS Code REST Client syntax with enhanced `# @expect-*` comments
- **HttpTokenizer**: Breaks down HTTP file content into structured tokens
- **Models/**: Domain objects (`HttpFile`, `HttpRequest`, `HttpRequestMetadata`, `VariableDefinition`)
- **Validation/**: Request name validation and comprehensive error handling

### Key Design Patterns

- **Metadata-First Parsing**: Requests are identified by `# @name` comments, not traditional `###` separators
- **Name-Based Request Lookup**: `HttpFile.GetRequestByName()` and `TryGetRequestByName()` for request retrieval
- **Enhanced Comments**: `# @expect-status`, `# @expect-header`, `# @expect-body-*` for test automation
- **Dependency Injection Ready**: All components accept optional logger and validator instances

## Development Workflows

### Building and Testing
```bash
# Build entire solution (uses .slnx format)
dotnet build vscode-restclient-dotnet.slnx --configuration Release

# Run all tests with coverage
dotnet test vscode-restclient-dotnet.slnx --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/RESTClient.NET.Core.Tests/
```

### Multi-Targeting Strategy
- **Primary Target**: .NET 9.0 for latest features
- **Compatibility**: .NET Standard 2.0 for broad framework support
- **CI Matrix**: Tests run on .NET 9.0 with Ubuntu runners

## Project-Specific Conventions

### HTTP File Format Support
```http
# File variables (processed first)
@baseUrl = https://api.example.com

# Request with enhanced metadata
# @name login-user
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "token"
POST {{baseUrl}}/auth/login HTTP/1.1
Content-Type: application/json

{"username": "user", "password": "pass"}
```

### Request Name Validation
- **Pattern**: `^[a-zA-Z0-9_-]+$` (enforced by `RequestNameValidationRegex`)
- **Uniqueness**: First occurrence wins for duplicate names in `HttpFile._requestsByName`
- **Case Sensitive**: Request names are case-sensitive for lookups

### Error Handling Strategy
- **Parsing Errors**: `HttpParseException` with detailed line/position information
- **Validation Errors**: `ValidationResult` objects with specific error categories
- **Missing Requests**: `KeyNotFoundException` from `GetRequestByName()`

### Test Patterns
- **FluentAssertions**: Primary assertion library (`result.Should().NotBeNull()`)
- **xUnit Theory**: Data-driven tests with `[MemberData]` for HTTP file scenarios
- **Arrange-Act-Assert**: Consistent test structure across all test files

## Integration Points

### ASP.NET Core Testing (In Progress)
- **WebApplicationFactory**: Standard integration test base class pattern
- **HttpFileTestBase**: Abstract base for HTTP file-driven integration tests
- **Test Data Generation**: `_httpFile.GetTestData()` extension method approach

### Logging Integration
- **Microsoft.Extensions.Logging**: Optional logging throughout parsing pipeline
- **Structured Logging**: Uses interpolated strings with named parameters

## File Organization Rules

### Core Library Layout
- `Models/`: Domain objects, no business logic
- `Parsing/`: Input processing and syntax analysis
- `Processing/`: Data transformation (variable resolution)
- `Validation/`: Business rule enforcement
- `Exceptions/`: Custom exception types

### Test Structure Mirrors Source
- `tests/RESTClient.NET.Core.Tests/` mirrors `src/RESTClient.NET.Core/`
- Test classes named `{ClassName}Tests.cs`
- One test class per source class, grouped by namespace

## Critical Implementation Details

### Variable Processing Order
1. File variables (`@name = value`) processed before requests
2. Environment variables (`{{variable}}`) resolved during request parsing  
3. Request variables (`{{request.response.body.$.token}}`) planned for future

### Package Management
- **Preview Releases**: Uses `1.0.0-preview.1` semantic versioning
- **Auto-Pack**: `GeneratePackageOnBuild=true` creates NuGet packages on build
- **Documentation**: XML docs required (`GenerateDocumentationFile=true`)

### Compatibility Requirements
- **VS Code REST Client**: 100% format compatibility maintained
- **Backward Compatibility**: Traditional `###` separators still supported
- **Standards Compliance**: HTTP/1.1 request format adherence

## Agentic Development Instructions

### Terminal-First Development Workflow
Always leverage terminal commands for development tasks rather than manual processes:

```bash
# Check project status and recent changes
git status
git log --oneline -10

# Validate changes with full build cycle
dotnet clean vscode-restclient-dotnet.slnx
dotnet restore vscode-restclient-dotnet.slnx
dotnet build vscode-restclient-dotnet.slnx --configuration Release

# Run comprehensive test validation
dotnet test vscode-restclient-dotnet.slnx --verbosity normal
```

### Change Validation Protocol
After making any code changes, always validate using this sequence:

1. **Build Validation**: Ensure compilation succeeds across all target frameworks
2. **Test Execution**: Run full test suite to verify no regressions
3. **Package Generation**: Confirm NuGet package builds correctly (auto-generated on build)
4. **Git Status**: Check for unintended file changes

### Testing Strategy for Agents
- **Use Built-in Test Tools**: Leverage VS Code's integrated test runner for immediate feedback
- **Focus on Core Tests**: `HttpFileParserTests` and `HttpSyntaxParserTests` are critical validation points
- **Regression Testing**: Always run full test suite after parser modifications
- **Coverage Validation**: Use `--collect:"XPlat Code Coverage"` for coverage analysis

### Git Workflow Integration
```bash
# Create feature branch for changes
git checkout -b feature/your-change-name

# Stage and commit with descriptive messages
git add .
git commit -m "feat: add parsing support for new VS Code REST Client feature"

# Validate before push
dotnet test vscode-restclient-dotnet.slnx
git push origin feature/your-change-name
```

### Pull Request Creation
When your branch is ready for review, create a pull request using the GitHub tool:

- **Use Tool**: `mcp_github_create_pull_request` for automated PR creation
- **Title Format**: Follow conventional commits (e.g., "feat: add enhanced expectation parsing")
- **Description**: Include implementation details, breaking changes, and test coverage
- **Base Branch**: Always target `main` unless specified otherwise
- **Draft Status**: Use draft PRs for work-in-progress that needs early feedback

### Critical Validation Points
- **Parser Changes**: Always test with `HttpFileParserTests.cs` variations
- **Model Updates**: Verify `HttpFileTests.cs` for request lookup functionality  
- **Syntax Changes**: Validate against `HttpSyntaxParserTests.cs` parsing scenarios
- **Breaking Changes**: Ensure backward compatibility with existing `.http` files
