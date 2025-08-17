using Microsoft.AspNetCore.Mvc;

namespace RESTClient.NET.Testing.Tests;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseRouting();

        // Simple API endpoints for testing
        app.MapGet("/api/users", () => new[] 
        {
            new { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
        });

        app.MapGet("/api/users/{id:int}", (int id) => 
        {
            if (id == 1)
                return Results.Ok(new { Id = 1, Name = "John Doe", Email = "john@example.com" });
            if (id == 2)
                return Results.Ok(new { Id = 2, Name = "Jane Smith", Email = "jane@example.com" });
            
            return Results.NotFound();
        });

        app.MapPost("/api/users", ([FromBody] dynamic user) => 
        {
            return Results.Created("/api/users/3", new { Id = 3, Name = user.Name, Email = user.Email });
        });

        app.MapPost("/api/auth/login", ([FromBody] dynamic loginRequest) => 
        {
            if (loginRequest.Username == "admin" && loginRequest.Password == "password")
            {
                return Results.Ok(new { 
                    Token = "jwt-token-123", 
                    ExpiresIn = 3600,
                    User = new { Id = 1, Name = "Admin User", Role = "Administrator" }
                });
            }
            
            return Results.Unauthorized();
        });

        app.Run();
    }
}
