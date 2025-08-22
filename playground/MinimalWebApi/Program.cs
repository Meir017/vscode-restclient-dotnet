using MinimalWebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null; // Use exact property names
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Simple test endpoints for HTTP file testing
app.MapGet("/ping", () => "pong")
   .WithName("Ping");

app.MapGet("/api/test", () => new { message = "Hello, World!", timestamp = DateTime.UtcNow })
   .WithName("GetTest");

app.MapGet("/api/test/{id:int}", (int id) => new { id, message = $"Test item {id}", timestamp = DateTime.UtcNow })
   .WithName("GetTestById");

app.MapPost("/api/test", (TestItem item) =>
{
    return Results.Created($"/api/test/{item.Id}", new { message = "Created", item, timestamp = DateTime.UtcNow });
})
.WithName("CreateTest");

app.MapPut("/api/test/{id:int}", (int id, TestItem item) =>
{
    return Results.Ok(new { message = "Updated", id, item, timestamp = DateTime.UtcNow });
})
.WithName("UpdateTest");

app.MapDelete("/api/test/{id:int}", (int id) =>
{
    return Results.NoContent();
})
.WithName("DeleteTest");

app.MapGet("/api/error", () =>
{
    return Results.Problem("Internal server error", statusCode: 500);
})
.WithName("GetError");

app.MapGet("/api/notfound", () =>
{
    return Results.NotFound(new { message = "Not found" });
})
.WithName("GetNotFound");

app.MapGet("/api/headers", (HttpContext context) =>
{
    return Results.Ok(new
    {
        message = "Headers test",
        headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
        timestamp = DateTime.UtcNow
    });
})
.WithName("GetHeaders");

app.Run();

namespace MinimalWebApi.Models
{
    /// <summary>
    /// Test model for POST/PUT requests
    /// </summary>
    /// <param name="Id">The unique identifier for the test item</param>
    /// <param name="Name">The name of the test item</param>
    /// <param name="Description">The optional description of the test item</param>
    internal sealed record TestItem(int Id, string Name, string? Description);
}

/// <summary>
/// Main program class for the MinimalWebApi application
/// </summary>
public partial class Program { }
