# HTTP File Reference

Complete syntax reference for RESTClient.NET HTTP files, fully compatible with VS Code REST Client format.

## Table of Contents

- [File Structure](#file-structure)
- [Variables](#variables)
- [Request Format](#request-format)
- [Request Metadata](#request-metadata)
- [System Variables](#system-variables)
- [Expectation Comments](#expectation-comments)
- [Comments and Separators](#comments-and-separators)
- [Examples](#examples)
- [Compatibility Notes](#compatibility-notes)

## File Structure

```http
# File variables (global scope)
@variable1 = value1
@variable2 = value2

# Request 1 with metadata
# @name request-name-1
# @expect status 200
HTTP_METHOD {{variable1}}/path HTTP/1.1
Header-Name: Header-Value

Optional request body

###

# Request 2 with different metadata
# @name request-name-2
# @expect status 201
# @expect header Location
HTTP_METHOD {{variable2}}/path HTTP/1.1
Content-Type: application/json

{
  "data": "value"
}
```

## Variables

### File Variables

Variables defined at the file level are available to all requests in the file.

```http
# Basic variable definition
@baseUrl = https://api.example.com
@apiVersion = v1
@contentType = application/json

# Using variables in other variables
@usersEndpoint = {{baseUrl}}/{{apiVersion}}/users
@authEndpoint = {{baseUrl}}/{{apiVersion}}/auth
```

**Syntax Rules:**

- Variables must start with `@`
- Variable names are case-sensitive
- Variable values can reference other variables using `{{variableName}}`
- Variables are resolved in the order they appear

### Variable Usage

```http
# Use variables in URLs
GET {{baseUrl}}/{{apiVersion}}/users HTTP/1.1

# Use variables in headers
Content-Type: {{contentType}}
X-API-Version: {{apiVersion}}

# Use variables in request bodies
{
  "apiUrl": "{{baseUrl}}",
  "version": "{{apiVersion}}"
}
```

## Request Format

### Basic HTTP Request

```http
# @name request-name
HTTP_METHOD URL HTTP/1.1
Header-Name: Header-Value
Another-Header: Another-Value

Optional request body
```

### Supported HTTP Methods

- `GET`
- `POST`
- `PUT`
- `PATCH`
- `DELETE`
- `HEAD`
- `OPTIONS`
- `TRACE`

### URL Formats

```http
# Absolute URLs
GET https://api.example.com/users HTTP/1.1

# URLs with variables
GET {{baseUrl}}/{{apiVersion}}/users HTTP/1.1

# URLs with query parameters
GET {{baseUrl}}/users?page=1&limit=10 HTTP/1.1

# URLs with system variables
GET {{baseUrl}}/users/{{$randomInt 1 100}} HTTP/1.1
```

### Headers

```http
# Standard headers
Content-Type: application/json
Accept: application/json
Authorization: Bearer token123

# Headers with variables
X-API-Key: {{apiKey}}
X-Request-ID: {{$guid}}
X-Timestamp: {{$timestamp}}

# Multiple headers
User-Agent: RESTClient.NET/1.0
Accept-Language: en-US,en;q=0.9
Cache-Control: no-cache
```

### Request Bodies

#### JSON Body

```http
POST {{baseUrl}}/users HTTP/1.1
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "id": {{$randomInt 1000 9999}},
  "createdAt": "{{$datetime iso8601}}"
}
```

#### Form Data

```http
POST {{baseUrl}}/users HTTP/1.1
Content-Type: application/x-www-form-urlencoded

name=John+Doe&email=john%40example.com&age=30
```

#### XML Body

```http
POST {{baseUrl}}/users HTTP/1.1
Content-Type: application/xml

<?xml version="1.0" encoding="UTF-8"?>
<user>
  <name>John Doe</name>
  <email>john@example.com</email>
  <id>{{$randomInt 1000 9999}}</id>
</user>
```

#### Plain Text Body

```http
POST {{baseUrl}}/notes HTTP/1.1
Content-Type: text/plain

This is a plain text note created at {{$datetime}}.
```

## Request Metadata

### Request Names

Every request should have a unique name for identification:

```http
# @name user-create
# @name user-get-by-id
# @name user-update
# @name user-delete
```

**Naming Rules:**

- Names must be unique within the file
- Use descriptive, kebab-case names
- Names are case-sensitive
- Only letters, numbers, hyphens, and underscores allowed

### Multiple Metadata Lines

```http
# @name complex-request
# @expect status 201
# @expect header Content-Type application/json
# @expect header Location
# @expect body-path $.id
# @expect body-contains "success"
POST {{baseUrl}}/complex HTTP/1.1
Content-Type: application/json

{"data": "value"}
```

## System Variables

RESTClient.NET provides built-in system variables for dynamic content generation.

### {{$guid}}

Generates a new GUID (UUID v4).

```http
# In headers
X-Request-ID: {{$guid}}
X-Correlation-ID: {{$guid}}

# In JSON body
{
  "id": "{{$guid}}",
  "sessionId": "{{$guid}}"
}
```

### {{$randomInt min max}}

Generates a random integer between min and max (inclusive).

```http
# Basic usage
{
  "id": {{$randomInt 1 1000}},
  "priority": {{$randomInt 1 5}},
  "timeout": {{$randomInt 5000 30000}}
}

# In URLs
GET {{baseUrl}}/users/{{$randomInt 1 100}} HTTP/1.1

# In headers
X-Retry-Count: {{$randomInt 1 3}}
```

### {{$timestamp}}

Generates the current Unix timestamp (seconds since epoch).

```http
# In headers
X-Timestamp: {{$timestamp}}

# In JSON body
{
  "createdAt": {{$timestamp}},
  "expiresAt": {{$timestamp}}
}

# In query parameters
GET {{baseUrl}}/data?since={{$timestamp}} HTTP/1.1
```

### {{$datetime format}}

Generates the current date/time in the specified format.

#### Predefined Formats

```http
# ISO 8601 format: 2023-12-01T15:30:45.123Z
"timestamp": "{{$datetime iso8601}}"

# RFC 1123 format: Fri, 01 Dec 2023 15:30:45 GMT
"lastModified": "{{$datetime rfc1123}}"
```

#### Custom Formats

Using .NET DateTime format strings:

```http
# Year-Month-Day: 2023-12-01
"date": "{{$datetime yyyy-MM-dd}}"

# Full date and time: 2023-12-01 15:30:45
"timestamp": "{{$datetime yyyy-MM-dd HH:mm:ss}}"

# Month/Day/Year: 12/01/2023
"dateUS": "{{$datetime MM/dd/yyyy}}"

# ISO format with timezone: 2023-12-01T15:30:45-05:00
"timestampTZ": "{{$datetime yyyy-MM-ddTHH:mm:sszzz}}"
```

## Expectation Comments

Expectation comments define assertions for automated testing.

### @expect status

Expects a specific HTTP status code.

```http
# Single status code
# @expect status 200
# @expect status 201
# @expect status 404
# @expect status 500

# Multiple expectations (all must pass)
# @expect status 200
# @expect status 201  # This would fail - only one can match
```

### @expect header

Expects a response header to be present or have a specific value.

```http
# Header presence only
# @expect header Content-Type
# @expect header Location
# @expect header X-Total-Count

# Header with specific value
# @expect header Content-Type application/json
# @expect header Cache-Control no-cache
# @expect header X-Rate-Limit-Remaining 100
```

### @expect body-contains

Expects the response body to contain a specific string.

```http
# Simple text matching
# @expect body-contains "success"
# @expect body-contains "error"
# @expect body-contains "John Doe"

# JSON property matching
# @expect body-contains "\"status\":\"active\""
# @expect body-contains "\"id\":123"

# Multiple contains (all must match)
# @expect body-contains "success"
# @expect body-contains "user"
# @expect body-contains "created"
```

### @expect body-path

Expects a specific JSON path to exist in the response body.

```http
# Root level properties
# @expect body-path $.id
# @expect body-path $.name
# @expect body-path $.status

# Nested properties
# @expect body-path $.user.profile.email
# @expect body-path $.data.attributes.createdAt

# Array elements
# @expect body-path $.users[0].id
# @expect body-path $.errors[0].message

# Array length
# @expect body-path $.users[*]

# Complex paths
# @expect body-path $.data[?(@.type=='user')].id
```

## Comments and Separators

### Comment Syntax

```http
# This is a comment
// This is also a comment

# Comments can be used for documentation
# @name user-login
# This endpoint authenticates a user and returns a JWT token
POST {{baseUrl}}/auth/login HTTP/1.1
```

### Request Separators

Use `###` to separate requests:

```http
# @name request-1
GET {{baseUrl}}/users HTTP/1.1

###

# @name request-2
POST {{baseUrl}}/users HTTP/1.1
Content-Type: application/json

{"name": "John"}

###

# @name request-3
DELETE {{baseUrl}}/users/1 HTTP/1.1
```

**Note:** Separators are optional but recommended for clarity.

## Examples

### Complete API Testing File

```http
@baseUrl = https://jsonplaceholder.typicode.com
@contentType = application/json

# @name get-all-posts
# @expect status 200
# @expect header Content-Type application/json
# @expect body-path $[0].id
GET {{baseUrl}}/posts HTTP/1.1
Accept: {{contentType}}

###

# @name get-post-by-id
# @expect status 200
# @expect body-path $.id
# @expect body-path $.title
# @expect body-path $.body
# @expect body-path $.userId
GET {{baseUrl}}/posts/1 HTTP/1.1
Accept: {{contentType}}

###

# @name create-post
# @expect status 201
# @expect header Content-Type application/json
# @expect body-path $.id
POST {{baseUrl}}/posts HTTP/1.1
Content-Type: {{contentType}}

{
  "title": "My Test Post {{$randomInt 1 1000}}",
  "body": "This is a test post created at {{$datetime iso8601}}",
  "userId": {{$randomInt 1 10}},
  "requestId": "{{$guid}}"
}

###

# @name update-post
# @expect status 200
# @expect body-path $.id
PUT {{baseUrl}}/posts/1 HTTP/1.1
Content-Type: {{contentType}}

{
  "id": 1,
  "title": "Updated Post Title",
  "body": "Updated post content",
  "userId": 1,
  "lastModified": "{{$datetime iso8601}}"
}

###

# @name delete-post
# @expect status 200
DELETE {{baseUrl}}/posts/1 HTTP/1.1

###

# @name get-nonexistent-post
# @expect status 404
GET {{baseUrl}}/posts/999999 HTTP/1.1
Accept: {{contentType}}
```

### Authentication Flow

```http
@authBaseUrl = https://auth.example.com
@apiBaseUrl = https://api.example.com

# @name register-user
# @expect status 201
# @expect body-path $.id
# @expect body-path $.email
POST {{authBaseUrl}}/register HTTP/1.1
Content-Type: application/json

{
  "email": "user{{$randomInt 1000 9999}}@example.com",
  "password": "securePassword123",
  "name": "Test User",
  "registeredAt": "{{$datetime iso8601}}"
}

###

# @name login-user
# @expect status 200
# @expect body-path $.token
# @expect body-path $.expiresIn
POST {{authBaseUrl}}/login HTTP/1.1
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "securePassword123"
}

###

# @name get-protected-resource
# @expect status 200
# @expect body-path $.data
GET {{apiBaseUrl}}/protected/profile HTTP/1.1
Authorization: Bearer YOUR_TOKEN_HERE
Accept: application/json
X-Request-ID: {{$guid}}

###

# @name refresh-token
# @expect status 200
# @expect body-path $.token
POST {{authBaseUrl}}/refresh HTTP/1.1
Content-Type: application/json

{
  "refreshToken": "YOUR_REFRESH_TOKEN_HERE"
}

###

# @name logout-user
# @expect status 204
POST {{authBaseUrl}}/logout HTTP/1.1
Authorization: Bearer YOUR_TOKEN_HERE
```

### Error Testing

```http
@baseUrl = https://api.example.com

# @name successful-request
# @expect status 200
# @expect body-contains "success"
GET {{baseUrl}}/valid-endpoint HTTP/1.1

###

# @name not-found-error
# @expect status 404
# @expect body-contains "Not Found"
GET {{baseUrl}}/nonexistent HTTP/1.1

###

# @name validation-error
# @expect status 400
# @expect body-contains "validation"
# @expect body-path $.errors[0].message
POST {{baseUrl}}/users HTTP/1.1
Content-Type: application/json

{
  "email": "invalid-email",
  "age": -5
}

###

# @name unauthorized-error
# @expect status 401
# @expect header WWW-Authenticate
GET {{baseUrl}}/protected HTTP/1.1

###

# @name forbidden-error
# @expect status 403
# @expect body-contains "insufficient permissions"
GET {{baseUrl}}/admin/users HTTP/1.1
Authorization: Bearer limited-token

###

# @name rate-limit-error
# @expect status 429
# @expect header Retry-After
# @expect body-contains "rate limit"
GET {{baseUrl}}/heavy-endpoint HTTP/1.1
```

## Compatibility Notes

### VS Code REST Client Compatibility

RESTClient.NET is 100% compatible with VS Code REST Client files:

- ✅ Variable syntax (`@var = value`)
- ✅ Request format (`METHOD URL HTTP/1.1`)
- ✅ Header syntax (`Header: Value`)
- ✅ Request separators (`###`)
- ✅ Comment syntax (`#` and `//`)
- ✅ Variable interpolation (`{{variableName}}`)

### Extensions and Enhancements

RESTClient.NET adds these enhancements while maintaining compatibility:

- ✅ Request naming (`# @name request-name`)
- ✅ Expectation comments (`# @expect status 200`)
- ✅ System variables (`{{$guid}}`, `{{$randomInt}}`, etc.)
- ✅ Enhanced error handling and validation

### Migration from VS Code REST Client

Existing VS Code REST Client files work without modification. To take advantage of RESTClient.NET features:

1. Add `# @name` comments to identify requests
2. Add `# @expect` comments for automated testing
3. Use system variables for dynamic content
4. Leverage the testing framework for integration tests

### File Encoding

- UTF-8 encoding recommended
- BOM (Byte Order Mark) supported but not required
- Line endings: CRLF (Windows) and LF (Unix/Mac) both supported

### Performance Considerations

- Files are parsed completely into memory
- Variable resolution is done at parse time
- System variables are evaluated when accessed
- Large files (>1MB) may impact performance
- Consider splitting large files into smaller, feature-specific files
