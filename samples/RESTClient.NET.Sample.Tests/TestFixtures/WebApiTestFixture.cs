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
    private bool _disposed;

    /// <summary>
    /// Seeds test data into the database.
    /// </summary>
    /// <param name="services">The service provider to use for database access.</param>
    public void SeedTestData(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
        Product[] products =
        [
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Category = "Electronics", StockQuantity = 10, IsActive = true },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless optical mouse", Price = 29.99m, Category = "Electronics", StockQuantity = 50, IsActive = true },
            new Product { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, Category = "Electronics", StockQuantity = 25, IsActive = true }
        ];

        context.Products.AddRange(products);
        context.SaveChanges();
    }

    /// <summary>
    /// Releases all resources used by the DatabaseTestFixture.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the DatabaseTestFixture and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // In-memory database is automatically disposed when context is disposed
                // No managed resources to dispose in this case
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Base test fixture for Web API integration tests using RESTClient.NET
/// </summary>
public class WebApiTestFixture : HttpFileTestBase<Program>
{
    private readonly DatabaseTestFixture _databaseFixture;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the WebApiTestFixture class.
    /// </summary>
    /// <param name="factory">The web application factory to use for testing.</param>
    public WebApiTestFixture(WebApplicationFactory<Program> factory) : base(factory)
    {
        _databaseFixture = new DatabaseTestFixture();
    }

    /// <summary>
    /// Configures the web application factory for testing.
    /// </summary>
    /// <param name="factory">The factory to configure.</param>
    /// <returns>The configured factory.</returns>
    protected override WebApplicationFactory<Program> ConfigureFactory(WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration completely
                ServiceDescriptor? dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Also remove the generic DbContextOptions if it exists
                ServiceDescriptor? genericDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions));
                if (genericDbContextDescriptor != null)
                {
                    services.Remove(genericDbContextDescriptor);
                }

                // Remove the ApplicationDbContext registration if it exists
                ServiceDescriptor? appDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
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

    /// <summary>
    /// Gets the path to the HTTP file used for testing.
    /// </summary>
    /// <returns>The path to the HTTP file.</returns>
    protected override string GetHttpFilePath()
    {
        return Path.Combine("HttpFiles", "auth-flow.http");
    }

    /// <summary>
    /// Modifies the HTTP file configuration for testing.
    /// </summary>
    /// <param name="httpFile">The HTTP file to modify.</param>
    protected override void ModifyHttpFile(HttpFile httpFile)
    {
        // For now, we'll rely on the test server's base address being injected
        // The HTTP files will need to use the test server's base URL
        // In a real implementation, we would process variables here

        // Seed test data after the factory is set up
        // We need to trigger server startup to get access to services

        using HttpClient client = Factory.CreateClient();
        _databaseFixture.SeedTestData(Factory.Services);

    }

    /// <summary>
    /// Releases the unmanaged resources used by the WebApiTestFixture and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _databaseFixture?.Dispose();
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }
}
