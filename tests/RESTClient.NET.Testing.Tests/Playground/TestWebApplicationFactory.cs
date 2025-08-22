using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RESTClient.NET.Testing.Tests.Playground;

/// <summary>
/// Custom WebApplicationFactory that uses MinimalWebApi Program class
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestWebApplicationFactory()
    {
        // Create a dummy solution file to prevent the "Solution root could not be located" error
        string solutionFile = Path.Combine(AppContext.BaseDirectory, "dummy.sln");
        if (!File.Exists(solutionFile))
        {
            File.WriteAllText(solutionFile, @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Test Solution
");
        }
    }

    protected override IHostBuilder? CreateHostBuilder()
    {
        // Use the MinimalWebApi Program class
        return null; // Let the default behavior use the referenced Program
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Test");
        builder.UseContentRoot(AppContext.BaseDirectory);

        // Configure services
        builder.ConfigureServices(services =>
        {
            services.AddLogging();
        });
    }
}
