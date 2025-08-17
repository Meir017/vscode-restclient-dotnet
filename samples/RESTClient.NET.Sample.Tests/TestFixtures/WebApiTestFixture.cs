using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Sample.Api.Data;
using RESTClient.NET.Sample.Api.Models;
using RESTClient.NET.Testing;

namespace RESTClient.NET.Sample.Tests.TestFixtures;

/// <summary>
/// Database test fixture for managing test data
/// </summary>
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
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Category = "Electronics", StockQuantity = 10, IsActive = true },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless optical mouse", Price = 29.99m, Category = "Electronics", StockQuantity = 50, IsActive = true },
            new Product { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, Category = "Electronics", StockQuantity = 25, IsActive = true }
        };
        
        context.Products.AddRange(products);
        context.SaveChanges();
    }
    
    public void Dispose()
    {
        // In-memory database is automatically disposed when context is disposed
    }
}

/// <summary>
/// Base test fixture for Web API integration tests using RESTClient.NET
/// </summary>
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
                // Remove the existing DbContext registration completely
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }
                
                // Also remove the generic DbContextOptions if it exists
                var genericDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions));
                if (genericDbContextDescriptor != null)
                {
                    services.Remove(genericDbContextDescriptor);
                }
                
                // Remove the ApplicationDbContext registration if it exists
                var appDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
                if (appDbContextDescriptor != null)
                {
                    services.Remove(appDbContextDescriptor);
                }

                // Add our test in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}"));
                
                // For now, keep the existing authentication but we'll override it in the client
                // This ensures all dependencies are satisfied
            });
        });
    }
    
    protected override string GetHttpFilePath()
    {
        return Path.Combine("HttpFiles", "auth-flow.http");
    }
    
    protected override void ModifyHttpFile(HttpFile httpFile)
    {
        // For now, we'll rely on the test server's base address being injected
        // The HTTP files will need to use the test server's base URL
        // In a real implementation, we would process variables here
        
        // Seed test data after the factory is set up
        // We need to trigger server startup to get access to services
        try
        {
            using var client = Factory.CreateClient();
            _databaseFixture.SeedTestData(Factory.Services);
        }
        catch (Exception)
        {
            // If seeding fails, continue - the database will be empty for tests
            // Some tests may still pass if they don't require specific data
        }
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
