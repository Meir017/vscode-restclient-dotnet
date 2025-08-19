using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTClient.NET.Core;
using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Parsing;
using RESTClient.NET.Testing.Extensions;
using RESTClient.NET.Testing.Models;
using Xunit;

namespace RESTClient.NET.Testing
{
    /// <summary>
    /// Abstract base class for HTTP file-driven integration tests with ASP.NET Core.
    /// Provides comprehensive integration testing capabilities using VS Code REST Client files.
    /// </summary>
    /// <typeparam name="TProgram">The program type for the ASP.NET Core application under test</typeparam>
    /// <remarks>
    /// <para>HttpFileTestBase enables data-driven integration testing using HTTP files:</para>
    /// <list type="bullet">
    /// <item>Automatic HTTP file parsing and request execution</item>
    /// <item>Built-in expectation validation (status codes, headers, body content)</item>
    /// <item>xUnit integration with <see cref="HttpFileTestData"/> for parameterized tests</item>
    /// <item>WebApplicationFactory integration for ASP.NET Core testing</item>
    /// <item>Comprehensive assertion methods for response validation</item>
    /// </list>
    /// <para>Override <see cref="GetHttpFilePath"/> to specify your HTTP file location.</para>
    /// <para>Use <c>[MemberData(nameof(HttpFileTestData))]</c> to run tests for each named request.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ApiIntegrationTests : HttpFileTestBase&lt;Program&gt;
    /// {
    ///     public ApiIntegrationTests(WebApplicationFactory&lt;Program&gt; factory) : base(factory) { }
    /// 
    ///     protected override string GetHttpFilePath() =&gt; "HttpFiles/api-tests.http";
    /// 
    ///     [Theory]
    ///     [MemberData(nameof(HttpFileTestData))]
    ///     public async Task ExecuteHttpFileTest(string requestName, HttpRequest request)
    ///     {
    ///         // Execute the request and validate expectations
    ///         var result = await ExecuteRequestAsync(requestName);
    ///         
    ///         // Additional custom assertions
    ///         Assert.True(result.IsSuccess);
    ///         Assert.NotNull(result.Response);
    ///     }
    /// 
    ///     [Fact]
    ///     public async Task SpecificEndpointTest()
    ///     {
    ///         var result = await ExecuteRequestAsync("get-users");
    ///         
    ///         result.Response.Should().HaveStatusCode(200);
    ///         result.Response.Should().HaveHeader("Content-Type", "application/json");
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class HttpFileTestBase<TProgram> : IClassFixture<WebApplicationFactory<TProgram>>, IDisposable
        where TProgram : class
    {
        private readonly WebApplicationFactory<TProgram> _factory;
        private readonly HttpFile _httpFile;
        private readonly HttpFileProcessor _httpFileProcessor;
        private bool _disposed;

        /// <summary>
        /// Gets the WebApplicationFactory for creating test clients
        /// </summary>
        protected WebApplicationFactory<TProgram> Factory => _factory;

        /// <summary>
        /// Gets the parsed HTTP file
        /// </summary>
        protected HttpFile HttpFile => _httpFile;

        /// <summary>
        /// Gets the test data for use with xUnit [MemberData]
        /// </summary>
        public static IEnumerable<object[]> HttpFileTestData => GetTestData();

        /// <summary>
        /// Initializes a new instance of the HttpFileTestBase class
        /// </summary>
        /// <param name="factory">The WebApplicationFactory to use for testing</param>
        protected HttpFileTestBase(WebApplicationFactory<TProgram> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            // Configure the factory if needed
            _factory = ConfigureFactory(_factory);

            // Get logger from the factory's services (cast to compatible logger type)
            var loggerFactory = _factory.Services.GetService<ILoggerFactory>();
            var processorLogger = loggerFactory?.CreateLogger<HttpFileProcessor>();

            // Initialize HTTP file processor
            _httpFileProcessor = new HttpFileProcessor(processorLogger);

            // Load and parse the HTTP file
            var httpFilePath = GetHttpFilePath();
            processorLogger?.LogInformation("Loading HTTP file from: {FilePath}", httpFilePath);

            _httpFile = LoadHttpFile(httpFilePath);
            
            // Allow modification of the HTTP file before tests
            ModifyHttpFile(_httpFile);

            processorLogger?.LogInformation("Loaded {RequestCount} requests from HTTP file", _httpFile.Requests.Count);
        }

        /// <summary>
        /// Gets the path to the HTTP file to be used in tests
        /// </summary>
        /// <returns>The absolute or relative path to the HTTP file</returns>
        protected abstract string GetHttpFilePath();

        /// <summary>
        /// Configures the WebApplicationFactory for testing
        /// Override this method to customize the test environment
        /// </summary>
        /// <param name="factory">The factory to configure</param>
        /// <returns>The configured factory</returns>
        protected virtual WebApplicationFactory<TProgram> ConfigureFactory(WebApplicationFactory<TProgram> factory)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(ConfigureTestServices);
            });
        }

        /// <summary>
        /// Configures additional services for testing
        /// Override this method to replace services with test doubles
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
            // Default implementation does nothing
            // Override in derived classes to configure test services
        }

        /// <summary>
        /// Modifies the HTTP file before tests are executed
        /// Override this method to set variables or modify requests
        /// </summary>
        /// <param name="httpFile">The HTTP file to modify</param>
        protected virtual void ModifyHttpFile(HttpFile httpFile)
        {
            // Default implementation does nothing
            // Override in derived classes to modify the HTTP file
        }

        /// <summary>
        /// Gets test data for all requests in the HTTP file
        /// </summary>
        /// <returns>Test data for xUnit [MemberData]</returns>
        protected static IEnumerable<object[]> GetTestData()
        {
            // This is a static method that creates a temporary instance to get test data
            // This is required by xUnit's [MemberData] attribute
            try
            {
                using var tempFactory = new WebApplicationFactory<TProgram>();
                var tempInstance = CreateTempInstance(tempFactory);
                return tempInstance._httpFile.GetTestData();
            }
            catch
            {
                // If we can't load the HTTP file, return empty test data
                // The actual error will be caught during test execution
                return new List<object[]> { new object[] { new HttpTestCase { Name = "LoadError", Method = "GET", Url = "/error" } } };
            }
        }

        /// <summary>
        /// Gets filtered test data based on criteria
        /// </summary>
        /// <param name="namePattern">Optional name pattern to match</param>
        /// <param name="methods">Optional HTTP methods to include</param>
        /// <param name="hasExpectations">Optional filter for test cases with expectations</param>
        /// <returns>Filtered test data for xUnit [MemberData]</returns>
        protected IEnumerable<object[]> GetFilteredTestData(
            string? namePattern = null,
            IEnumerable<string>? methods = null,
            bool? hasExpectations = null)
        {
            return _httpFile.GetTestCases()
                .Filter(namePattern, methods, hasExpectations)
                .Select(testCase => new object[] { testCase });
        }

        /// <summary>
        /// Gets a specific test case by name
        /// </summary>
        /// <param name="name">The name of the test case</param>
        /// <returns>The test case if found</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the test case is not found</exception>
        protected HttpTestCase GetTestCase(string name)
        {
            var request = _httpFile.GetRequestByName(name);
            return _httpFile.GetTestCases().First(tc => tc.Name == name);
        }

        /// <summary>
        /// Tries to get a specific test case by name
        /// </summary>
        /// <param name="name">The name of the test case</param>
        /// <param name="testCase">The test case if found</param>
        /// <returns>True if the test case was found, false otherwise</returns>
        protected bool TryGetTestCase(string name, out HttpTestCase testCase)
        {
            testCase = null!;
            
            if (!_httpFile.TryGetRequestByName(name, out var request))
                return false;

            testCase = _httpFile.GetTestCases().First(tc => tc.Name == name);
            return true;
        }

        /// <summary>
        /// Processes variables in a request with the current context
        /// </summary>
        /// <param name="requestName">The name of the request to process</param>
        /// <param name="environmentVariables">Optional environment variables</param>
        /// <returns>The processed request</returns>
        protected HttpRequest? GetProcessedRequest(string requestName, IDictionary<string, string>? environmentVariables = null)
        {
            return _httpFileProcessor.GetProcessedRequest(_httpFile, requestName, environmentVariables);
        }

        /// <summary>
        /// Creates a temporary instance for static test data generation
        /// This is a factory method that derived classes must implement
        /// </summary>
        /// <param name="factory">The factory to use</param>
        /// <returns>A temporary instance</returns>
        private static HttpFileTestBase<TProgram> CreateTempInstance(WebApplicationFactory<TProgram> factory)
        {
            // This is a bit of a hack to get around xUnit's static [MemberData] requirement
            // We need to use reflection to create a temporary instance
            var derivedType = typeof(TProgram).Assembly
                .GetTypes()
                .FirstOrDefault(t => t.IsSubclassOf(typeof(HttpFileTestBase<TProgram>)) && !t.IsAbstract);

            if (derivedType == null)
                throw new InvalidOperationException($"Could not find a concrete implementation of HttpFileTestBase<{typeof(TProgram).Name}>");

            return (HttpFileTestBase<TProgram>)Activator.CreateInstance(derivedType, factory)!;
        }

        private HttpFile LoadHttpFile(string httpFilePath)
        {
            if (string.IsNullOrWhiteSpace(httpFilePath))
                throw new ArgumentException("HTTP file path cannot be null or empty", nameof(httpFilePath));

            // Resolve relative paths
            if (!Path.IsPathRooted(httpFilePath))
            {
                // Try to resolve relative to the test assembly location
                var assemblyLocation = typeof(TProgram).Assembly.Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                httpFilePath = Path.Combine(assemblyDirectory!, httpFilePath);
            }

            if (!File.Exists(httpFilePath))
                throw new FileNotFoundException($"HTTP file not found: {httpFilePath}");

            var parser = new HttpFileParser();
            return parser.ParseFileAsync(httpFilePath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Disposes the resources used by this instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _factory?.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes the resources used by this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
