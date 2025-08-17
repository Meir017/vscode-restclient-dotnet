# Integration Testing PRD

## RESTClient.NET Integration Testing Framework

### 1. Executive Summary

This document outlines the comprehensive integration testing strategy for RESTClient.NET, which includes building a complete demonstration ecosystem with:

1. **Sample ASP.NET Core Web API** - A realistic web application showcasing common API patterns
2. **HTTP File Test Suite** - Comprehensive `.http` files covering all API endpoints with expectations
3. **Integration Test Project** - xUnit-based tests leveraging RESTClient.NET.Testing framework with `WebApplicationFactory`
4. **In-Memory Testing** - Self-contained tests using in-memory databases and test servers

**Objectives:**

- Demonstrate RESTClient.NET's capabilities with real-world scenarios
- Provide comprehensive examples for developers adopting the library
- Validate the testing framework against actual ASP.NET Core applications using `WebApplicationFactory`
- Create a reference implementation for best practices in HTTP file-based integration testing
- Showcase built-in system variables for dynamic test data generation

### 2. Project Structure

```text
samples/
├── RESTClient.NET.Sample.Api/              # Sample ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── UsersController.cs
│   │   ├── ProductsController.cs
│   │   ├── OrdersController.cs
│   │   └── AuthController.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Product.cs
│   │   ├── Order.cs
│   │   └── AuthModels.cs
│   ├── Services/
│   │   ├── IUserService.cs
│   │   ├── UserService.cs
│   │   ├── IProductService.cs
│   │   └── ProductService.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── SeedData.cs
│   ├── Program.cs
│   └── RESTClient.NET.Sample.Api.csproj
│
├── RESTClient.NET.Sample.Tests/            # Integration test project
│   ├── HttpFiles/
│   │   ├── users-api.http                  # User management endpoints
│   │   ├── products-api.http               # Product catalog endpoints
│   │   ├── orders-api.http                 # Order processing endpoints
│   │   └── auth-flow.http                  # Authentication flow
│   ├── TestFixtures/
│   │   ├── WebApiTestFixture.cs
│   │   └── DatabaseTestFixture.cs
│   ├── IntegrationTests/
│   │   ├── UserApiTests.cs
│   │   ├── ProductApiTests.cs
│   │   ├── OrderApiTests.cs
│   │   └── AuthFlowTests.cs
│   ├── Utilities/
│   │   ├── TestDataBuilder.cs
│   │   └── HttpFileTestHelper.cs
│   └── RESTClient.NET.Sample.Tests.csproj
│
└── README.md                                # Sample documentation
```

### 3. Sample ASP.NET Core Web API Specification

#### 3.1 API Overview

The sample API represents a simplified e-commerce system with the following domains:

- **Authentication**: JWT-based authentication and authorization
- **User Management**: User registration, profile management, and role-based access
- **Product Catalog**: Product CRUD operations with search and filtering
- **Order Processing**: Order creation, status tracking, and history

#### 3.2 API Endpoints

**Authentication Endpoints:**

```http
POST   /api/auth/register          # User registration
POST   /api/auth/login             # User authentication
POST   /api/auth/refresh           # Token refresh
POST   /api/auth/logout            # User logout
```

**User Management Endpoints:**

```http
GET    /api/users                  # List users (admin only)
GET    /api/users/me               # Current user profile
GET    /api/users/{id}             # Get user by ID
PUT    /api/users/me               # Update current user profile
PUT    /api/users/{id}             # Update user (admin only)
DELETE /api/users/{id}             # Delete user (admin only)
```

**Product Catalog Endpoints:**

```http
GET    /api/products               # List products with filtering
GET    /api/products/{id}          # Get product by ID
POST   /api/products               # Create product (admin only)
PUT    /api/products/{id}          # Update product (admin only)
DELETE /api/products/{id}          # Delete product (admin only)
GET    /api/products/search        # Search products
```

**Order Processing Endpoints:**

```http
GET    /api/orders                 # List user's orders
GET    /api/orders/{id}            # Get order by ID
POST   /api/orders                 # Create new order
PUT    /api/orders/{id}/status     # Update order status (admin only)
DELETE /api/orders/{id}            # Cancel order
```

#### 3.3 Data Models

**User Model:**
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}

public enum UserRole
{
    Customer = 0,
    Admin = 1
}
```

**Product Model:**
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Order Model:**
```csharp
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
```

#### 3.4 Technical Implementation

**Framework and Dependencies:**

- ASP.NET Core 8.0+
- Entity Framework Core with SQLite (for simplicity)
- JWT Bearer Authentication
- Swagger/OpenAPI documentation
- Serilog for structured logging
- FluentValidation for input validation

**Database Setup:**

- In-memory SQLite database for tests
- Entity Framework migrations
- Seed data for testing scenarios

**Authentication:**

- JWT token-based authentication
- Role-based authorization (Customer, Admin)
- Token refresh mechanism
- Secure password hashing

### 4. HTTP File Test Suite Specification

#### 4.1 Authentication Flow (`auth-flow.http`)

```http
@baseUrl = https://localhost:5001
@contentType = application/json

### Variables for authentication flow
@adminEmail = admin@example.com
@adminPassword = Admin123!
@customerEmail = customer@example.com
@customerPassword = Customer123!

# @name register-customer
# @expect status 201
# @expect header Location /api/users/*
# @expect body-contains "id"
# @expect body-path $.email customer@example.com
POST {{baseUrl}}/api/auth/register HTTP/1.1
Content-Type: {{contentType}}
X-Request-ID: {{$guid}}
X-Timestamp: {{$timestamp}}

{
    "username": "customer_{{$randomInt 100 999}}",
    "email": "{{customerEmail}}",
    "password": "{{customerPassword}}",
    "firstName": "John",
    "lastName": "Customer",
    "registrationId": "{{$guid}}",
    "registrationDate": "{{$datetime iso8601}}"
}

# @name login-customer
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "token"
# @expect body-path $.user.email customer@example.com
POST {{baseUrl}}/api/auth/login HTTP/1.1
Content-Type: {{contentType}}

{
    "email": "{{customerEmail}}",
    "password": "{{customerPassword}}"
}

# @name login-admin
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "token"
# @expect body-path $.user.role Admin
POST {{baseUrl}}/api/auth/login HTTP/1.1
Content-Type: {{contentType}}

{
    "email": "{{adminEmail}}",
    "password": "{{adminPassword}}"
}

# @name refresh-token
# @expect status 200
# @expect body-contains "token"
POST {{baseUrl}}/api/auth/refresh HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{login-customer.response.body.$.token}}

{
    "refreshToken": "{{login-customer.response.body.$.refreshToken}}"
}

# @name logout
# @expect status 200
POST {{baseUrl}}/api/auth/logout HTTP/1.1
Authorization: Bearer {{login-customer.response.body.$.token}}
```

#### 4.2 User Management (`users-api.http`)

```http
@baseUrl = https://localhost:5001
@contentType = application/json

### Admin token for privileged operations
@adminToken = {{login-admin.response.body.$.token}}
@customerToken = {{login-customer.response.body.$.token}}

# @name get-current-user
# @expect status 200
# @expect header Content-Type application/json
# @expect body-path $.email customer@example.com
GET {{baseUrl}}/api/users/me HTTP/1.1
Authorization: Bearer {{customerToken}}

# @name update-current-user
# @expect status 200
# @expect body-path $.firstName UpdatedJohn
PUT {{baseUrl}}/api/users/me HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{customerToken}}

{
    "firstName": "UpdatedJohn",
    "lastName": "Customer"
}

# @name list-users-admin
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "users"
GET {{baseUrl}}/api/users HTTP/1.1
Authorization: Bearer {{adminToken}}

# @name get-user-by-id
# @expect status 200
# @expect body-path $.id 1
GET {{baseUrl}}/api/users/1 HTTP/1.1
Authorization: Bearer {{adminToken}}

# @name list-users-unauthorized
# @expect status 403
GET {{baseUrl}}/api/users HTTP/1.1
Authorization: Bearer {{customerToken}}

# @name delete-user-admin
# @expect status 204
DELETE {{baseUrl}}/api/users/999 HTTP/1.1
Authorization: Bearer {{adminToken}}
```

#### 4.3 Product Catalog (`products-api.http`)

```http
@baseUrl = https://localhost:5001
@contentType = application/json
@adminToken = {{login-admin.response.body.$.token}}
@customerToken = {{login-customer.response.body.$.token}}

# @name list-products
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "products"
GET {{baseUrl}}/api/products HTTP/1.1

# @name list-products-with-filter
# @expect status 200
# @expect body-contains "Electronics"
GET {{baseUrl}}/api/products?category=Electronics&minPrice=100&maxPrice=1000 HTTP/1.1

# @name search-products
# @expect status 200
# @expect body-contains "laptop"
GET {{baseUrl}}/api/products/search?q=laptop HTTP/1.1

# @name get-product-by-id
# @expect status 200
# @expect body-path $.id 1
GET {{baseUrl}}/api/products/1 HTTP/1.1

# @name get-product-not-found
# @expect status 404
GET {{baseUrl}}/api/products/99999 HTTP/1.1

# @name create-product-admin
# @expect status 201
# @expect header Location /api/products/*
# @expect body-path $.name Test Product
POST {{baseUrl}}/api/products HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{adminToken}}
X-Request-ID: {{$guid}}
X-Creation-Time: {{$datetime iso8601}}

{
    "name": "Test Product {{$randomInt 1000 9999}}",
    "description": "Dynamic test product created at {{$datetime iso8601}}",
    "price": {{$randomInt 10 100}}.99,
    "category": "Test",
    "sku": "SKU-{{$guid}}",
    "stockQuantity": {{$randomInt 1 100}}
}

# @name create-product-unauthorized
# @expect status 403
POST {{baseUrl}}/api/products HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{customerToken}}

{
    "name": "Unauthorized Product",
    "price": 100.00
}

# @name update-product-admin
# @expect status 200
# @expect body-path $.name Updated Test Product
PUT {{baseUrl}}/api/products/{{create-product-admin.response.body.$.id}} HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{adminToken}}

{
    "name": "Updated Test Product",
    "description": "Updated description",
    "price": 349.99,
    "category": "Test",
    "stockQuantity": 75
}

# @name delete-product-admin
# @expect status 204
DELETE {{baseUrl}}/api/products/{{create-product-admin.response.body.$.id}} HTTP/1.1
Authorization: Bearer {{adminToken}}
```

#### 4.4 Order Processing (`orders-api.http`)

```http
@baseUrl = https://localhost:5001
@contentType = application/json
@customerToken = {{login-customer.response.body.$.token}}
@adminToken = {{login-admin.response.body.$.token}}

# @name create-order
# @expect status 201
# @expect header Location /api/orders/*
# @expect body-path $.totalAmount 599.98
# @expect body-path $.status Pending
POST {{baseUrl}}/api/orders HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{customerToken}}
X-Request-ID: {{$guid}}
X-Order-Time: {{$datetime iso8601}}

{
    "orderId": "{{$guid}}",
    "orderNumber": "ORD-{{$randomInt 100000 999999}}",
    "orderDate": "{{$datetime iso8601}}",
    "items": [
        {
            "productId": 1,
            "quantity": {{$randomInt 1 5}},
            "requestId": "{{$guid}}"
        },
        {
            "productId": 2,
            "quantity": {{$randomInt 1 3}},
            "requestId": "{{$guid}}"
        }
    ]
}

# @name list-user-orders
# @expect status 200
# @expect header Content-Type application/json
# @expect body-contains "orders"
GET {{baseUrl}}/api/orders HTTP/1.1
Authorization: Bearer {{customerToken}}

# @name get-order-by-id
# @expect status 200
# @expect body-path $.id {{create-order.response.body.$.id}}
GET {{baseUrl}}/api/orders/{{create-order.response.body.$.id}} HTTP/1.1
Authorization: Bearer {{customerToken}}

# @name get-order-unauthorized
# @expect status 404
GET {{baseUrl}}/api/orders/999 HTTP/1.1
Authorization: Bearer {{customerToken}}

# @name update-order-status-admin
# @expect status 200
# @expect body-path $.status Processing
PUT {{baseUrl}}/api/orders/{{create-order.response.body.$.id}}/status HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{adminToken}}

{
    "status": "Processing"
}

# @name update-order-status-unauthorized
# @expect status 403
PUT {{baseUrl}}/api/orders/{{create-order.response.body.$.id}}/status HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{customerToken}}

{
    "status": "Delivered"
}

# @name cancel-order
# @expect status 200
# @expect body-path $.status Cancelled
DELETE {{baseUrl}}/api/orders/{{create-order.response.body.$.id}} HTTP/1.1
Authorization: Bearer {{customerToken}}

# @name create-order-invalid-product
# @expect status 400
# @expect body-contains "Product not found"
POST {{baseUrl}}/api/orders HTTP/1.1
Content-Type: {{contentType}}
Authorization: Bearer {{customerToken}}

{
    "items": [
        {
            "productId": 99999,
            "quantity": 1
        }
    ]
}
```

### 5. Integration Test Project Specification

#### 5.1 Test Project Structure

**Dependencies:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Bogus" Version="34.0.2" />
<PackageReference Include="RESTClient.NET.Core" />
<PackageReference Include="RESTClient.NET.Testing" />
```

#### 5.2 Base Test Fixture

```csharp
public class WebApiTestFixture : HttpFileTestBase<Program>
{
    private readonly DatabaseTestFixture _databaseFixture;
    
    public WebApiTestFixture(WebApplicationFactory<Program> factory) : base(factory)
    {
        _databaseFixture = new DatabaseTestFixture();
    }
    
    protected override WebApplicationFactory<Program> ConfigureFactory(WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Replace database with in-memory version
                services.RemoveDbContext<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}"));
                
                // Override external services with mocks for testing
                services.AddScoped<IEmailService, MockEmailService>();
                services.AddScoped<IPaymentService, MockPaymentService>();
                
                // Add test authentication scheme
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        "Test", options => { });
            });
        });
    }
    
    protected override string GetHttpFilePath()
    {
        return Path.Combine("HttpFiles", "auth-flow.http");
    }
    
    protected override void ModifyHttpFile(HttpFile httpFile)
    {
        // Set test-specific variables - use TestServer's base address
        httpFile.SetVariable("baseUrl", Factory.Server.BaseAddress.ToString().TrimEnd('/'));
        httpFile.SetVariable("contentType", "application/json");
        
        // Seed test data
        _databaseFixture.SeedTestData(Factory.Services);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _databaseFixture?.Dispose();
        }
        base.Dispose(disposing);
    }
}

// Test authentication handler for simplified testing
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create test claims based on the request
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Role, "Customer")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

#### 5.3 Specific Test Classes

**Authentication Flow Tests:**

```csharp
public class AuthFlowTests : WebApiTestFixture
{
    public AuthFlowTests(WebApplicationFactory<Program> factory) : base(factory) { }

    protected override string GetHttpFilePath() => 
        Path.Combine("HttpFiles", "auth-flow.http");
    
    [Theory]
    [MemberData(nameof(HttpFileTestData))]
    public async Task AuthFlow_ShouldWork(HttpTestCase testCase)
    {
        // The test executes against the in-memory TestServer
        var client = Factory.CreateClient();
        var requestMessage = testCase.ToHttpRequestMessage();
        
        var response = await client.SendAsync(requestMessage);
        
        // Assert using the expectation framework
        await HttpResponseAssertion.AssertResponse(response, testCase.ExpectedResponse);
        
        // Additional specific assertions for auth flow
        if (testCase.Name == "login-customer")
        {
            var content = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content);
            
            loginResponse.Token.Should().NotBeNullOrEmpty();
            loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
            loginResponse.User.Email.Should().Be("customer@example.com");
        }
    }
    
    [Fact]
    public async Task CompleteAuthFlow_ShouldMaintainUserSession()
    {
        // All requests execute against the same in-memory TestServer instance
        var registrationCase = GetTestCase("register-customer");
        var loginCase = GetTestCase("login-customer");
        var profileCase = GetTestCase("get-current-user");
        
        var client = Factory.CreateClient();
        
        // Execute in sequence with variable resolution
        var registrationResponse = await client.SendAsync(registrationCase.ToHttpRequestMessage());
        var loginResponse = await client.SendAsync(loginCase.ToHttpRequestMessage());
        var profileResponse = await client.SendAsync(profileCase.ToHttpRequestMessage());
        
        // Verify complete flow works in-memory
        registrationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

**User Management Tests:**

```csharp
public class UserApiTests : WebApiTestFixture
{
    public UserApiTests(WebApplicationFactory<Program> factory) : base(factory) { }

    protected override string GetHttpFilePath() => 
        Path.Combine("HttpFiles", "users-api.http");
    
    [Theory]
    [MemberData(nameof(HttpFileTestData))]
    public async Task UserEndpoints_ShouldWork(HttpTestCase testCase)
    {
        // All HTTP requests are executed against the in-memory TestServer
        var client = Factory.CreateClient();
        var requestMessage = testCase.ToHttpRequestMessage();
        
        var response = await client.SendAsync(requestMessage);
        
        // Assert using the expectation framework
        await HttpResponseAssertion.AssertResponse(response, testCase.ExpectedResponse);
    }
    
    [Fact]
    public async Task AdminOperations_ShouldRequireAdminRole()
    {
        var adminRequests = HttpFile.Requests
            .Where(r => r.Headers.ContainsKey("Authorization") && 
                       r.Headers["Authorization"].Contains("adminToken"))
            .ToList();
            
        var client = Factory.CreateClient();
        
        foreach (var request in adminRequests)
        {
            var testCase = HttpFile.GetTestCases().First(tc => tc.Name == request.Name);
            var requestMessage = testCase.ToHttpRequestMessage();
            var response = await client.SendAsync(requestMessage);
            
            // Admin requests should succeed with admin token
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }
    }
}
```

#### 5.4 Advanced Testing Scenarios

**Performance Testing:**

```csharp
[Fact]
public async Task ProductSearch_ShouldCompleteWithinTimeLimit()
{
    var searchRequest = HttpFile.GetRequestByName("search-products");
    searchRequest.Metadata.Expectations.Should().Contain(e => 
        e.Type == ExpectationType.MaxTime && e.Value == "2000ms");
    
    var stopwatch = Stopwatch.StartNew();
    var response = await ExecuteTestCase(searchRequest);
    stopwatch.Stop();
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
}
```

**Data Validation Testing:**

```csharp
[Fact]
public async Task CreateOrder_ShouldValidateInventory()
{
    // Test against in-memory database with controlled test data
    var orderRequest = HttpFile.GetRequestByName("create-order-insufficient-stock");
    var response = await ExecuteTestCase(orderRequest);
    
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("Insufficient stock");
}
```

### 6. Test Data Management

#### 6.1 Database Seeding

```csharp
public class DatabaseTestFixture : IDisposable
{
    public void SeedTestData(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // The in-memory database is automatically created by EF Core
        context.Database.EnsureCreated();
        
        // Clear any existing data for test isolation
        context.Users.RemoveRange(context.Users);
        context.Products.RemoveRange(context.Products);
        context.Orders.RemoveRange(context.Orders);
        
        // Seed users with known test data
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var customerUser = new User
        {
            Id = 2,
            Username = "customer",
            Email = "customer@example.com", 
            FirstName = "John",
            LastName = "Customer",
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.AddRange(adminUser, customerUser);
        
        // Seed products with predictable IDs for HTTP file references
        var products = new[]
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99m, Category = "Electronics", StockQuantity = 10, IsActive = true },
            new Product { Id = 2, Name = "Mouse", Price = 29.99m, Category = "Electronics", StockQuantity = 50, IsActive = true },
            new Product { Id = 3, Name = "Keyboard", Price = 79.99m, Category = "Electronics", StockQuantity = 25, IsActive = true }
        };
        
        context.Products.AddRange(products);
        context.SaveChanges();
    }
    
    public void Dispose()
    {
        // In-memory database is automatically disposed when context is disposed
    }
}
```

#### 6.2 Test Data Builders

```csharp
public class TestDataBuilder
{
    public static User BuildUser(UserRole role = UserRole.Customer) => new Faker<User>()
        .RuleFor(u => u.Username, f => f.Internet.UserName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.Role, role)
        .RuleFor(u => u.IsActive, true)
        .RuleFor(u => u.CreatedAt, f => f.Date.Recent())
        .Generate();
    
    public static Product BuildProduct(string category = "Electronics") => new Faker<Product>()
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price()))
        .RuleFor(p => p.Category, category)
        .RuleFor(p => p.StockQuantity, f => f.Random.Int(1, 100))
        .RuleFor(p => p.IsActive, true)
        .RuleFor(p => p.CreatedAt, f => f.Date.Recent())
        .Generate();
}
```

### 7. Continuous Integration Integration

#### 7.1 Test Pipeline Configuration

```yaml
# .github/workflows/integration-tests.yml
name: Integration Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore vscode-restclient-dotnet.slnx
    
    - name: Build solution
      run: dotnet build vscode-restclient-dotnet.slnx --no-restore --configuration Release
    
    - name: Run unit tests
      run: dotnet test tests/RESTClient.NET.Core.Tests/ --no-build --configuration Release
    
    - name: Run integration tests
      run: dotnet test samples/RESTClient.NET.Sample.Tests/ --no-build --configuration Release
    
    - name: Upload test results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: test-results
        path: |
          **/TestResults/**
          **/coverage.xml
```

### 8. Documentation and Examples

#### 8.1 Sample Project README

The sample project will include comprehensive documentation covering:

- **Getting Started**: How to run the integration tests using `WebApplicationFactory`
- **API Documentation**: Swagger/OpenAPI documentation with examples
- **HTTP File Guide**: Explanation of the test HTTP files and their structure
- **Testing Patterns**: Best practices for using RESTClient.NET.Testing with in-memory testing
- **Customization**: How to adapt the patterns for different APIs and test scenarios

#### 8.2 Tutorial Content

- **Basic Integration Testing**: Step-by-step guide to setting up in-memory integration tests
- **Advanced Scenarios**: Complex testing patterns including authentication flows with `WebApplicationFactory`
- **Performance Testing**: Using HTTP files for performance validation in test environments
- **CI/CD Integration**: Incorporating HTTP file tests in deployment pipelines without external dependencies

### 9. Success Criteria

1. **✅ Sample API Completeness**: Fully functional ASP.NET Core API with realistic features
2. **✅ HTTP File Coverage**: Comprehensive `.http` files covering all API endpoints
3. **✅ Integration Test Suite**: Complete test suite demonstrating all library features
4. **✅ Real-world Validation**: Tests validate actual HTTP requests and responses
5. **✅ Documentation Quality**: Clear, comprehensive documentation with examples
6. **✅ CI/CD Integration**: Automated testing pipeline with sample project
7. **✅ Performance Validation**: Tests complete within reasonable time limits
8. **✅ Error Handling**: Comprehensive testing of error scenarios and edge cases

### 10. Implementation Timeline

#### Phase 1: Sample API Development (Week 1-2)

- Create ASP.NET Core Web API project
- Implement core controllers and services
- Set up authentication and authorization
- Create database models and seeding

#### Phase 2: HTTP File Creation (Week 2-3)

- Develop comprehensive HTTP files for all endpoints
- Add expectation comments for automated testing
- Test HTTP files manually with VS Code REST Client
- Validate variable resolution and chaining

#### Phase 3: Integration Test Implementation (Week 3-4)

- Create test project with proper fixtures
- Implement HttpFileTestBase-derived test classes
- Add custom assertions and validation logic
- Set up test data management and seeding

#### Phase 4: Documentation and Polish (Week 4-5)

- Create comprehensive documentation
- Add code comments and examples
- Set up CI/CD pipeline
- Performance optimization and testing

### 11. Risk Mitigation

#### 11.1 Technical Risks

- **Complex Authentication Flow**: Mitigated by incremental implementation and thorough testing
- **Variable Resolution Complexity**: Addressed through comprehensive test scenarios
- **Performance Issues**: Monitored through automated performance tests
- **Database Test Isolation**: Handled through in-memory databases and proper cleanup

#### 11.2 Project Risks

- **Scope Creep**: Controlled through clear success criteria and phased approach
- **Integration Complexity**: Reduced by following established ASP.NET Core testing patterns
- **Maintenance Overhead**: Minimized through automated testing and clear documentation

### 12. Conclusion

This integration testing framework will provide a comprehensive demonstration of RESTClient.NET's capabilities while serving as a reference implementation for developers. The combination of a realistic sample API, thorough HTTP file test suites, and complete integration test project using `WebApplicationFactory` will validate the library's effectiveness in real-world scenarios and provide valuable examples for adoption.

The project will establish RESTClient.NET as a mature, production-ready solution for HTTP file-based integration testing in the .NET ecosystem, bridging the gap between development tooling and automated testing frameworks. By using `WebApplicationFactory` and in-memory databases, the tests are fast, reliable, and can run in any CI/CD environment without external dependencies.
