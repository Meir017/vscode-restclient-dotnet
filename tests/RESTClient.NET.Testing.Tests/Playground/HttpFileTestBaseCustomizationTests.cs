using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core.Models;
using Xunit;

namespace RESTClient.NET.Testing.Tests.Playground;

/// <summary>
/// Tests for HttpFileTestBase virtual method customization
/// </summary>
public class HttpFileTestBaseCustomizationTests
{
    [Fact]
    public void ConfigureFactory_ShouldBeCallableDuringConstruction()
    {
        // Arrange & Act
        using var factory = new TestWebApplicationFactory();
        using var testBase = new CustomConfigurationTestBase(factory);

        // Assert
        testBase.ConfigureFactoryCalled.Should().BeTrue();
        testBase.Factory.Should().NotBeSameAs(testBase.OriginalFactory);
    }

    [Fact]
    public void ConfigureTestServices_ShouldBeCallableDuringConstruction()
    {
        // Arrange & Act
        using var factory = new TestWebApplicationFactory();
        using var testBase = new CustomServicesTestBase(factory);

        // Assert
        testBase.ConfigureTestServicesCalled.Should().BeTrue();
    }

    [Fact]
    public void ModifyHttpFile_ShouldBeCallableDuringConstruction()
    {
        // Arrange & Act
        using var factory = new TestWebApplicationFactory();
        using var testBase = new CustomModificationTestBase(factory);

        // Assert
        testBase.ModifyHttpFileCalled.Should().BeTrue();
        testBase.HttpFile.Should().NotBeNull();
    }

    [Fact]
    public void CustomConfiguration_ShouldPersistThroughoutInstance()
    {
        // Arrange & Act
        using var factory = new TestWebApplicationFactory();
        using var testBase = new FullCustomizationTestBase(factory);

        // Assert
        testBase.ConfigureFactoryCalled.Should().BeTrue();
        testBase.ConfigureTestServicesCalled.Should().BeTrue();
        testBase.ModifyHttpFileCalled.Should().BeTrue();

        // Verify the HTTP file has been processed
        testBase.HttpFile.FileVariables.Should().ContainKey("baseUrl");
        testBase.HttpFile.FileVariables.Should().ContainKey("contentType");
    }

    [Fact]
    public void GetHttpFileTestData_ShouldWorkWithCustomizedInstance()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory();
        using var testBase = new FullCustomizationTestBase(factory);

        // Act
        IEnumerable<object[]> testData = testBase.GetHttpFileTestData();

        // Assert
        testData.Should().NotBeEmpty();
    }

    // Test implementations for customization scenarios
    private sealed class CustomConfigurationTestBase : HttpFileTestBase<Program>
    {
        public bool ConfigureFactoryCalled { get; private set; }
        public new WebApplicationFactory<Program> Factory => base.Factory;
        public new WebApplicationFactory<Program> OriginalFactory => base.OriginalFactory;

        public CustomConfigurationTestBase(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => "Integration/test-integration.http";

        protected override WebApplicationFactory<Program> ConfigureFactory(WebApplicationFactory<Program> factory)
        {
            ConfigureFactoryCalled = true;
            return base.ConfigureFactory(factory);
        }
    }

    private sealed class CustomServicesTestBase : HttpFileTestBase<Program>
    {
        public bool ConfigureTestServicesCalled { get; private set; }

        public CustomServicesTestBase(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => "Integration/test-integration.http";

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            ConfigureTestServicesCalled = true;
            services.AddSingleton<ILogger<CustomServicesTestBase>>(
                provider => provider.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<CustomServicesTestBase>());
        }
    }

    private sealed class CustomModificationTestBase : HttpFileTestBase<Program>
    {
        public bool ModifyHttpFileCalled { get; private set; }
        public new HttpFile HttpFile => base.HttpFile;

        public CustomModificationTestBase(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => "Integration/test-integration.http";

        protected override void ModifyHttpFile(HttpFile httpFile)
        {
            ModifyHttpFileCalled = true;
            // Verify the HTTP file is passed correctly
            httpFile.Should().NotBeNull();
        }
    }

    private sealed class FullCustomizationTestBase : HttpFileTestBase<Program>
    {
        public bool ConfigureFactoryCalled { get; private set; }
        public bool ConfigureTestServicesCalled { get; private set; }
        public bool ModifyHttpFileCalled { get; private set; }
        public new HttpFile HttpFile => base.HttpFile;

        public FullCustomizationTestBase(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => "Integration/test-integration.http";

        protected override WebApplicationFactory<Program> ConfigureFactory(WebApplicationFactory<Program> factory)
        {
            ConfigureFactoryCalled = true;
            return base.ConfigureFactory(factory);
        }

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            ConfigureTestServicesCalled = true;
            services.AddLogging(builder => builder.AddConsole());
        }

        protected override void ModifyHttpFile(HttpFile httpFile)
        {
            ModifyHttpFileCalled = true;
            // Just verify we can modify the HTTP file by checking it's not null
            // FileVariables is readonly, so we can't actually modify it in tests
            httpFile.Should().NotBeNull();
            httpFile.FileVariables.Should().NotBeNull();
        }
    }
}
