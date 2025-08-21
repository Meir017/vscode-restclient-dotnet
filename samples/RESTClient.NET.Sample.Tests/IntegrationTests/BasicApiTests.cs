using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace RESTClient.NET.Sample.Tests.IntegrationTests;

/// <summary>
/// Basic integration tests to validate the API is working
/// </summary>
public class BasicApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

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
