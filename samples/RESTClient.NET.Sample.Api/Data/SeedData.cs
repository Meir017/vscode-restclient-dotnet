using RESTClient.NET.Sample.Api.Models;

namespace RESTClient.NET.Sample.Api.Data;

/// <summary>
/// Seeds the database with initial test data
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Initialize the database with seed data
    /// </summary>
    public static void Initialize(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // Seed users
        var users = new[]
        {
            new User
            {
                Username = "admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "customer",
                Email = "customer@example.com",
                FirstName = "John",
                LastName = "Customer",
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Seed products
        var products = new[]
        {
            new Product
            {
                Name = "Laptop",
                Description = "High-performance laptop for professional work",
                Price = 999.99m,
                Category = "Electronics",
                StockQuantity = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Mouse",
                Description = "Wireless optical mouse",
                Price = 29.99m,
                Category = "Electronics",
                StockQuantity = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Keyboard",
                Description = "Mechanical keyboard with RGB lighting",
                Price = 79.99m,
                Category = "Electronics",
                StockQuantity = 25,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
