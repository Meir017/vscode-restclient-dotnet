using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace RESTClient.NET.Sample.Tests.IntegrationTests;

/// <summary>
/// Basic integration tests to validate the API is working
/// </summary>
/// <summary>
/// Basic API integration tests
/// </summary>
public class BasicApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicApiTests"/> class
    /// </summary>
    /// <param name="factory">Web application factory</param>
    public BasicApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Tests that GET /api/products returns OK status
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    [Fact]
    public async Task GetProducts_ShouldReturnOk()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that GET /api/products/{id} returns OK status
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    [Fact]
    public async Task GetProductById_ShouldReturnOk()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/products/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Tests that the API is running and can create a client
    /// </summary>
    [Fact]
    public void ApiIsRunning_ShouldReturnSuccess()
    {
        // This basic test just checks if the test setup works
        HttpClient client = _factory.CreateClient();

        // Act & Assert - just verify we can create a client
        Assert.NotNull(client);
        Assert.NotNull(client.BaseAddress);
    }
}
