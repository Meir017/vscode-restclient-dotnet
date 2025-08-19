# File Body Support Implementation

This document describes the implementation of file body support in RESTClient.NET, enabling VS Code REST Client compatibility for external file references as request bodies.

## Feature Overview

The implementation adds support for three file body reference formats:

### 1. Raw File Body (`< filepath`)
- Loads file content as-is without any processing
- No variable substitution or transformation
- Suitable for binary files, static content, or when exact file content is needed

```http
POST https://api.example.com/upload
Content-Type: application/xml

< ./static-data.xml
```

### 2. File Body with Variables (`<@ filepath`)
- Loads file content with variable processing enabled
- Variables like `{{baseUrl}}`, `{{token}}`, etc. will be resolved
- Uses UTF-8 encoding by default
- Suitable for template files that need dynamic content

```http
POST https://api.example.com/process
Content-Type: application/json

<@ ./request-template.json
```

### 3. File Body with Custom Encoding (`<@encoding filepath`)
- Loads file content with variable processing and custom encoding
- Supports various encoding formats
- Useful for legacy files or specific character encoding requirements

```http
POST https://api.example.com/legacy
Content-Type: text/plain

<@latin1 ./legacy-data.txt
```

## Supported Encodings

The implementation supports the following encodings:

- **UTF-8**: `utf8`, `utf-8` (default)
- **UTF-16**: `utf16`, `utf-16`
- **UTF-32**: `utf32`, `utf-32`
- **ASCII**: `ascii`, `us-ascii`
- **Latin1**: `latin1`, `iso-8859-1`, `iso88591`
- **Windows-1252**: `windows1252`, `cp1252`

## File Path Resolution

File paths can be specified in multiple formats:

- **Relative paths**: `./data.json`, `../shared/template.xml`
- **Absolute paths**: `C:\Users\Data\file.xml`, `/home/user/data.json`
- **Paths with spaces**: `./my data file.json`

## Implementation Architecture

### Core Components

#### 1. FileBodyReference Model
- **Location**: `src/RESTClient.NET.Core/Models/FileBodyReference.cs`
- **Purpose**: Represents metadata about file body references
- **Properties**:
  - `FilePath`: The path to the file
  - `ProcessVariables`: Whether to process variables in the file content
  - `Encoding`: The encoding to use when reading the file
  - `LineNumber`: Source location for error reporting

#### 2. Enhanced Token Types
- **Location**: `src/RESTClient.NET.Core/Parsing/HttpTokens.cs`
- **New Token Types**:
  - `FileBody`: Raw file reference
  - `FileBodyWithVariables`: File reference with variable processing
  - `FileBodyWithEncoding`: File reference with custom encoding

#### 3. Updated Tokenizer
- **Location**: `src/RESTClient.NET.Core/Parsing/HttpTokenizer.cs`
- **Enhancement**: Regex pattern to detect file body syntax
- **Pattern**: `^<(@\s*([a-zA-Z0-9-]+)?\s*)?(.+)$`

#### 4. Enhanced Syntax Parser
- **Location**: `src/RESTClient.NET.Core/Parsing/HttpSyntaxParser.cs`
- **Enhancements**: 
  - Processing of file body tokens
  - Encoding resolution with fallback logic
  - FileBodyReference creation

#### 5. HttpRequest Model Extension
- **Location**: `src/RESTClient.NET.Core/Models/HttpRequest.cs`
- **Addition**: `FileBodyReference` property
- **Behavior**: When `FileBodyReference` is set, `Body` should be null

## Testing Coverage

### Unit Tests
- **FileBodyReferenceTests**: Model behavior and validation
- **HttpTokenizerFileBodyTests**: Tokenization of file body syntax
- **HttpSyntaxParserFileBodyTests**: Parsing and FileBodyReference creation

### Integration Tests
- **FileBodyIntegrationTests**: End-to-end parsing scenarios
- **HttpFileParserFileBodyTests**: Complete file parsing with multiple requests

### Test Coverage Areas
1. **Syntax Recognition**: All file body formats are correctly tokenized
2. **Encoding Support**: Various encoding formats work correctly
3. **Path Handling**: Relative, absolute, and paths with spaces
4. **Error Handling**: Invalid encodings fallback to UTF-8
5. **Metadata Integration**: File bodies work with expectations and headers
6. **Compatibility**: Traditional `###` separators work with file bodies

## Examples

### Complete HTTP File Example

```http
# File Body Demonstration
@baseUrl = https://api.example.com
@authToken = your-token-here

# @name upload-static-xml
# @expect status 201
POST {{baseUrl}}/upload/xml
Authorization: Bearer {{authToken}}
Content-Type: application/xml

< ./static-data.xml

# @name process-template
# @expect status 200
# @expect body-contains "processed"
POST {{baseUrl}}/process
Authorization: Bearer {{authToken}}
Content-Type: application/json

<@ ./request-template.json

# @name upload-legacy-data
# @expect status 202
POST {{baseUrl}}/legacy
Authorization: Bearer {{authToken}}
Content-Type: text/plain; charset=iso-8859-1

<@latin1 ./legacy-data.txt
```

### Template File Example (`request-template.json`)

```json
{
  "timestamp": "{{$timestamp}}",
  "baseUrl": "{{baseUrl}}",
  "authToken": "{{authToken}}",
  "requestId": "{{$guid}}",
  "data": {
    "message": "Template with variables"
  }
}
```

## Compatibility

### VS Code REST Client Compatibility
- ✅ Full syntax compatibility with VS Code REST Client
- ✅ Same file path resolution behavior
- ✅ Same encoding support patterns
- ✅ Integration with existing variable system

### .NET Framework Compatibility
- ✅ .NET 9.0: Full feature support
- ✅ .NET Standard 2.0: Full feature support with compatibility shims
- ✅ Multi-targeting: Build succeeds for both target frameworks

## Future Enhancements

### Potential Improvements
1. **File Watching**: Automatic refresh when referenced files change
2. **File Content Caching**: Performance optimization for repeated references
3. **Base64 Encoding**: Support for `<@base64 filepath` syntax
4. **Remote File Support**: Support for HTTP/HTTPS file references
5. **File Size Validation**: Configurable limits for file body sizes

### Integration Points
1. **RESTClient.NET.Testing**: File content resolution in test scenarios
2. **Sample Projects**: Examples demonstrating file body usage
3. **Documentation**: Comprehensive guide for file body patterns

## Migration Notes

### For Existing Users
- **No Breaking Changes**: Existing HTTP files continue to work unchanged
- **Opt-in Feature**: File body support is used only when syntax is present
- **Backward Compatibility**: Traditional body content works alongside file references

### For New Users
- **Recommended Patterns**: Use file bodies for large payloads or templates
- **Best Practices**: Organize file bodies in dedicated directories
- **Error Handling**: Check file paths and encoding specifications

## Performance Considerations

### Memory Usage
- File content is loaded into memory during parsing
- Large files will increase memory footprint
- Consider file size limits for production scenarios

### Parsing Performance
- Regex pattern matching adds minimal overhead
- File I/O operations are deferred (not implemented in parsing phase)
- Tokenization performance impact is negligible

### Error Handling
- Invalid file paths are captured as parsing metadata
- Encoding errors fallback gracefully to UTF-8
- Line number tracking for debugging file body references
