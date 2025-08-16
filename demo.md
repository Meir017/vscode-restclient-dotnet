# AspNetCore integration testing

```cs
public abstract class HttpFileTestBase<TProgram>
{
    protected readonly WebApplicationFactory<TProgram> _factory;
    protected readonly string _httpFilePath;

    protected readonly HttpFile _httpFile;

    protected HttpFileTestBase()
    {
        _factory = CreateFactory();
        _httpFilePath = GetHttpFilePath();

        _httpFile = new HttpFileParser().Parse(_httpFilePath);

        // GetTestData is an extension method on top of the base parsing library.
        HttpFileTestData = _httpFile.GetTestData();
    }

    protected abstract WebApplicationFactory<TProgram> CreateFactory();

    /// <summary>
    /// Gets the path to the HTTP file to be used in tests.
    /// Path is related to the csproj file.
    /// </summary>
    protected abstract string GetHttpFilePath();

    protected virtual void ModifyHttpFile(HttpFile httpFile)
    {
        // Optionally modify the HttpFile before tests
    }

    protected IEnumerable<object[]> HttpFileTestData { get; }

    [Theory]
    [MemberData(nameof(HttpFileTestData))]
    public async Task TestHttpRequest(string method, string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        // setup any additional request properties if needed

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // Here you can assert the response status code, headers, body, etc.
        // expected response will be extracted from the HttpFile, a new section on each request this testing library will introduce to allow assertions
    }
}