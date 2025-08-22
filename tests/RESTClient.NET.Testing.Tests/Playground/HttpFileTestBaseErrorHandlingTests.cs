using System;
using System.IO;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RESTClient.NET.Testing.Tests.Playground;

/// <summary>
/// Tests for HttpFileTestBase error handling scenarios
/// </summary>
public class HttpFileTestBaseErrorHandlingTests
{
    [Fact]
    public void Constructor_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Func<TestHttpFileTestBase> action = () => new TestHttpFileTestBase(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Fact]
    public void Constructor_WithNonExistentHttpFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory();

        // Act & Assert
        Func<TestHttpFileTestBaseWithNonExistentFile> action = () => new TestHttpFileTestBaseWithNonExistentFile(factory);
        action.Should().Throw<FileNotFoundException>()
            .WithMessage("*non-existent-file.http*");
    }

    [Fact]
    public void Constructor_WithEmptyHttpFilePath_ShouldThrowArgumentException()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory();

        // Act & Assert
        Func<TestHttpFileTestBaseWithEmptyPath> action = () => new TestHttpFileTestBaseWithEmptyPath(factory);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("httpFilePath");
    }

    [Fact]
    public void Constructor_WithNullHttpFilePath_ShouldThrowArgumentException()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory();

        // Act & Assert
        Func<TestHttpFileTestBaseWithNullPath> action = () => new TestHttpFileTestBaseWithNullPath(factory);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("httpFilePath");
    }

    [Fact]
    public void Dispose_ShouldBeCallableMultipleTimes()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory();
        var testBase = new TestHttpFileTestBase(factory);

        // Act & Assert
        testBase.Dispose(); // First call
        Action action = () => testBase.Dispose(); // Second call
        action.Should().NotThrow();
    }

    [Fact]
    public void FactoryDisposal_ShouldNotDisposeOriginalFactory()
    {
        // Arrange
        using var originalFactory = new TestWebApplicationFactory();
        var testBase = new TestHttpFileTestBase(originalFactory);

        // Act
        testBase.Dispose();

        // Assert
        // Original factory should still be usable (not disposed)
        Func<System.Net.Http.HttpClient> action = () => originalFactory.CreateClient();
        action.Should().NotThrow();
    }

    // Test implementations for error scenarios
    private sealed class TestHttpFileTestBase : HttpFileTestBase<Program>
    {
        public TestHttpFileTestBase(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => "Integration/test-integration.http";
    }

    private sealed class TestHttpFileTestBaseWithNonExistentFile : HttpFileTestBase<Program>
    {
        public TestHttpFileTestBaseWithNonExistentFile(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => "non-existent-file.http";
    }

    private sealed class TestHttpFileTestBaseWithEmptyPath : HttpFileTestBase<Program>
    {
        public TestHttpFileTestBaseWithEmptyPath(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => string.Empty;
    }

    private sealed class TestHttpFileTestBaseWithNullPath : HttpFileTestBase<Program>
    {
        public TestHttpFileTestBaseWithNullPath(WebApplicationFactory<Program> factory) : base(factory) { }
        protected override string GetHttpFilePath() => null!;
    }
}
