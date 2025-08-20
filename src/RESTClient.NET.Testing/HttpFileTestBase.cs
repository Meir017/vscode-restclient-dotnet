using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    /// <item>xUnit integration with instance-based test data for parameterized tests</item>
    /// <item>WebApplicationFactory integration for ASP.NET Core testing</item>
    /// <item>Comprehensive assertion methods for response validation</item>
    /// </list>
    /// <para>Override <see cref="GetHttpFilePath"/> to specify your HTTP file location.</para>
    /// <para>Use <c>[MemberData(nameof(GetHttpFileTestData))]</c> to run tests for each named request.</para>
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
    ///     [MemberData(nameof(GetHttpFileTestData))]
    ///     public async Task ExecuteHttpFileTest(HttpTestCase testCase)
    ///     {
    ///         // Create HTTP client from factory
    ///         using var client = Factory.CreateClient();
    ///         
    ///         // Convert test case to HTTP request message
    ///         using var request = testCase.ToHttpRequestMessage();
    ///         
    ///         // Execute request
    ///         using var response = await client.SendAsync(request);
    ///         
    ///         // Validate expectations if present
    ///         if (testCase.ExpectedResponse != null)
    ///         {
    ///             // Add your validation logic here
    ///         }
    ///     }
    /// 
    ///     [Fact]
    ///     public async Task SpecificEndpointTest()
    ///     {
    ///         var testCase = GetTestCase("get-users");
    ///         using var client = Factory.CreateClient();
    ///         using var request = testCase.ToHttpRequestMessage();
    ///         using var response = await client.SendAsync(request);
    ///         
    ///         Assert.Equal(200, (int)response.StatusCode);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class HttpFileTestBase<TProgram> : IClassFixture<WebApplicationFactory<TProgram>>, IDisposable
        where TProgram : class
    {
        private readonly WebApplicationFactory<TProgram> _originalFactory;
        private readonly WebApplicationFactory<TProgram> _configuredFactory;
        private readonly HttpFile _httpFile;
        private readonly HttpFileProcessor _httpFileProcessor;
        private bool _disposed;

        /// <summary>
        /// Gets the original WebApplicationFactory passed to the constructor
        /// </summary>
        protected WebApplicationFactory<TProgram> OriginalFactory => _originalFactory;

        /// <summary>
        /// Gets the configured WebApplicationFactory for creating test clients
        /// </summary>
        protected WebApplicationFactory<TProgram> Factory => _configuredFactory;

        /// <summary>
        /// Gets the parsed HTTP file
        /// </summary>
        protected HttpFile HttpFile => _httpFile;

        /// <summary>
        /// Initializes a new instance of the HttpFileTestBase class
        /// </summary>
        /// <param name="factory">The WebApplicationFactory to use for testing</param>
        protected HttpFileTestBase(WebApplicationFactory<TProgram> factory)
        {
            _originalFactory = factory ?? throw new ArgumentNullException(nameof(factory));

            // Configure the factory
            _configuredFactory = ConfigureFactory(_originalFactory);

            // Get logger from the configured factory's services
            var loggerFactory = _configuredFactory.Services.GetService<ILoggerFactory>();
            var processorLogger = loggerFactory?.CreateLogger<HttpFileProcessor>();

            // Initialize HTTP file processor
            _httpFileProcessor = new HttpFileProcessor(processorLogger);

            // Load and parse the HTTP file
            var httpFilePath = GetHttpFilePath();
            processorLogger?.LogInformation("Loading HTTP file from: {FilePath}", httpFilePath);

            _httpFile = LoadHttpFileSync(httpFilePath);
            
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
        /// Gets test data for all requests in the HTTP file for use with xUnit [MemberData]
        /// </summary>
        /// <returns>Test data for xUnit [MemberData]</returns>
        public IEnumerable<object[]> GetHttpFileTestData()
        {
            return _httpFile.GetTestData();
        }

        /// <summary>
        /// Creates and initializes a new instance asynchronously (alternative factory pattern)
        /// </summary>
        /// <param name="factory">The WebApplicationFactory to use for testing</param>
        /// <param name="httpFilePath">The path to the HTTP file</param>
        /// <returns>A fully initialized HttpFileTestBase instance</returns>
        protected static async Task<TDerived> CreateAsync<TDerived>(WebApplicationFactory<TProgram> factory, string httpFilePath)
            where TDerived : HttpFileTestBase<TProgram>, new()
        {
            var instance = new TDerived();
            await instance.InitializeAsync(factory, httpFilePath);
            return instance;
        }

        /// <summary>
        /// Alternative async initialization method (for derived classes that need async initialization)
        /// </summary>
        protected virtual async Task InitializeAsync(WebApplicationFactory<TProgram> factory, string httpFilePath)
        {
            // This could be used by derived classes that need async initialization
            // For now, we use the sync pattern in the constructor
            await Task.CompletedTask;
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

        private async Task<HttpFile> LoadHttpFileAsync(string httpFilePath)
        {
            if (string.IsNullOrWhiteSpace(httpFilePath))
                throw new ArgumentException("HTTP file path cannot be null or empty", nameof(httpFilePath));

            // Resolve relative paths relative to the test assembly, not the program assembly
            if (!Path.IsPathRooted(httpFilePath))
            {
                var testAssemblyLocation = GetType().Assembly.Location;
                if (!string.IsNullOrEmpty(testAssemblyLocation))
                {
                    var testAssemblyDirectory = Path.GetDirectoryName(testAssemblyLocation);
                    if (!string.IsNullOrEmpty(testAssemblyDirectory))
                    {
                        httpFilePath = Path.Combine(testAssemblyDirectory, httpFilePath);
                    }
                }
            }

            if (!File.Exists(httpFilePath))
                throw new FileNotFoundException($"HTTP file not found: {httpFilePath}");

            var parser = new HttpFileParser();
            return await parser.ParseFileAsync(httpFilePath);
        }

        private HttpFile LoadHttpFileSync(string httpFilePath)
        {
            if (string.IsNullOrWhiteSpace(httpFilePath))
                throw new ArgumentException("HTTP file path cannot be null or empty", nameof(httpFilePath));

            // Resolve relative paths relative to the test assembly, not the program assembly
            if (!Path.IsPathRooted(httpFilePath))
            {
                var testAssemblyLocation = GetType().Assembly.Location;
                if (!string.IsNullOrEmpty(testAssemblyLocation))
                {
                    var testAssemblyDirectory = Path.GetDirectoryName(testAssemblyLocation);
                    if (!string.IsNullOrEmpty(testAssemblyDirectory))
                    {
                        httpFilePath = Path.Combine(testAssemblyDirectory, httpFilePath);
                    }
                }
            }

            if (!File.Exists(httpFilePath))
                throw new FileNotFoundException($"HTTP file not found: {httpFilePath}");

            var parser = new HttpFileParser();
            // Use the synchronous Parse method instead of async to avoid deadlocks
            var content = File.ReadAllText(httpFilePath);
            return parser.Parse(content);
        }

        /// <summary>
        /// Disposes the resources used by this instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Only dispose the configured factory, not the original one passed to constructor
                // The original factory is owned by xUnit and should not be disposed by us
                if (!ReferenceEquals(_configuredFactory, _originalFactory))
                {
                    _configuredFactory?.Dispose();
                }
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
