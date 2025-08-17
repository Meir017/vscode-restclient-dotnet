---
description: "Add support for new VS Code REST Client HTTP syntax features"
mode: agent
tools: ["github", "vscode", "filesystem"]
---

# Add HTTP Syntax Support

You are tasked with adding support for a new VS Code REST Client HTTP syntax feature to RESTClient.NET.

## Context

RESTClient.NET is a C# library that parses VS Code REST Client (.http) files. The parsing pipeline consists of:

1. **HttpTokenizer** - Breaks HTTP file content into tokens
2. **HttpSyntaxParser** - Parses tokens into structured HTTP requests
3. **HttpFileParser** - Orchestrates the parsing process
4. **Models** - Domain objects representing parsed content

## Task Requirements

When adding new HTTP syntax support:

### 1. Analyze the Syntax
- Research the VS Code REST Client feature documentation
- Understand the syntax format and expected behavior
- Identify how it should integrate with existing parsing

### 2. Update the Tokenizer (if needed)
- Add new token types to handle the syntax in `HttpTokenizer.cs`
- Ensure proper token recognition patterns
- Add comprehensive unit tests in `HttpTokenizerTests.cs`

### 3. Enhance the Syntax Parser
- Update `HttpSyntaxParser.cs` to handle new syntax
- Implement parsing logic for the new feature
- Maintain backward compatibility with existing syntax
- Add validation for the new syntax elements

### 4. Update Models (if needed)
- Add properties to `HttpRequest`, `HttpRequestMetadata`, or `VariableDefinition` as appropriate
- Ensure proper serialization and equality implementations
- Follow existing naming conventions

### 5. Add Comprehensive Tests
- Create test cases in `HttpSyntaxParserTests.cs`
- Cover valid syntax variations
- Test error cases and malformed input
- Verify integration with existing features

### 6. Update Documentation
- Add examples to README files
- Update any relevant documentation
- Include usage examples in sample HTTP files

## Implementation Steps

1. **Research**: Study the VS Code REST Client documentation for the new feature
2. **Design**: Plan how the feature fits into the existing architecture
3. **Implement**: Make minimal changes following existing patterns
4. **Test**: Add comprehensive test coverage
5. **Validate**: Ensure no regressions in existing functionality

## Files to Consider

- `src/RESTClient.NET.Core/Parsing/HttpTokenizer.cs`
- `src/RESTClient.NET.Core/Parsing/HttpSyntaxParser.cs` 
- `src/RESTClient.NET.Core/Models/HttpRequest.cs`
- `src/RESTClient.NET.Core/Models/HttpRequestMetadata.cs`
- `tests/RESTClient.NET.Core.Tests/Parsing/HttpSyntaxParserTests.cs`
- `samples/RESTClient.NET.Sample.Tests/HttpFiles/users-api.http`

## Success Criteria

- [ ] New syntax is correctly parsed and represented in the object model
- [ ] Backward compatibility is maintained
- [ ] Comprehensive test coverage (>90%)
- [ ] All existing tests continue to pass
- [ ] Documentation is updated with examples
- [ ] Integration tests demonstrate the feature in action

Please specify which VS Code REST Client syntax feature you'd like to add support for.
