using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents an HTTP file containing multiple HTTP requests with enhanced metadata and name-based lookup.
    /// Provides the primary container for parsed VS Code REST Client (.http) files.
    /// </summary>
    /// <remarks>
    /// <para>HttpFile serves as the root container for parsed HTTP files with the following features:</para>
    /// <list type="bullet">
    /// <item>Name-based request lookup via <see cref="GetRequestByName"/> and <see cref="TryGetRequestByName"/></item>
    /// <item>File-level variable storage and resolution</item>
    /// <item>Request metadata preservation (expect comments, custom headers)</item>
    /// <item>First-occurrence-wins policy for duplicate request names</item>
    /// </list>
    /// <para>Request names are case-sensitive and must follow the pattern: <c>^[a-zA-Z0-9_-]+$</c></para>
    /// <para>File variables are processed before individual requests and available for variable substitution.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Parse an HTTP file
    /// var parser = new HttpFileParser();
    /// var httpFile = parser.Parse(httpFileContent);
    ///
    /// // Access requests by name
    /// var loginRequest = httpFile.GetRequestByName("login-user");
    /// var profileRequest = httpFile.GetRequestByName("get-profile");
    ///
    /// // Check if request exists
    /// if (httpFile.TryGetRequestByName("optional-request", out var request))
    /// {
    ///     Console.WriteLine($"Found request: {request.Method} {request.Url}");
    /// }
    ///
    /// // Access file variables
    /// var baseUrl = httpFile.FileVariables["baseUrl"];
    /// Console.WriteLine($"API base URL: {baseUrl}");
    ///
    /// // Iterate all requests
    /// foreach (var req in httpFile.Requests)
    /// {
    ///     Console.WriteLine($"{req.Name}: {req.Method} {req.Url}");
    /// }
    /// </code>
    /// </example>
    public class HttpFile
    {
        /// <summary>
        /// Gets or sets the source path of the HTTP file
        /// </summary>
        public string? SourcePath { get; set; }

        /// <summary>
        /// Gets the collection of HTTP requests in the file
        /// </summary>
        public IReadOnlyList<HttpRequest> Requests { get; }

        /// <summary>
        /// Gets the file-level variables defined in the HTTP file
        /// </summary>
        public IReadOnlyDictionary<string, string> FileVariables { get; }

        private readonly Dictionary<string, HttpRequest> _requestsByName;

        /// <summary>
        /// Initializes a new instance of the HttpFile class
        /// </summary>
        /// <param name="requests">The collection of HTTP requests</param>
        /// <param name="fileVariables">The file-level variables</param>
        public HttpFile(IEnumerable<HttpRequest> requests, IReadOnlyDictionary<string, string>? fileVariables = null)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));

            var requestList = requests.ToList();
            Requests = requestList.AsReadOnly();
            FileVariables = fileVariables ?? new Dictionary<string, string>();

            // Build request lookup dictionary - keep first occurrence for duplicates
            _requestsByName = [];
            foreach (var request in requestList)
            {
                if (!string.IsNullOrEmpty(request.Name))
                {
                    if (!_requestsByName.ContainsKey(request.Name))
                    {
                        _requestsByName[request.Name] = request;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an HTTP request by its name
        /// </summary>
        /// <param name="name">The request name to search for</param>
        /// <returns>The HTTP request with the specified name</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when no request with the specified name is found</exception>
        public HttpRequest GetRequestByName(string name)
        {
            if (TryGetRequestByName(name, out var request))
            {
                return request!;
            }

            throw new KeyNotFoundException($"Request with name '{name}' not found");
        }

        /// <summary>
        /// Attempts to get an HTTP request by its name
        /// </summary>
        /// <param name="name">The request name to search for</param>
        /// <param name="request">When this method returns, contains the HTTP request if found; otherwise, null</param>
        /// <returns>true if a request with the specified name was found; otherwise, false</returns>
        public bool TryGetRequestByName(string name, out HttpRequest? request)
        {
            request = null;
            if (string.IsNullOrEmpty(name))
                return false;

            return _requestsByName.TryGetValue(name, out request);
        }

        /// <summary>
        /// Gets all unique request names in the file
        /// </summary>
        /// <returns>A collection of request names</returns>
        public IEnumerable<string> GetRequestNames()
        {
            return _requestsByName.Keys;
        }

        /// <summary>
        /// Checks if a request with the specified name exists
        /// </summary>
        /// <param name="name">The request name to check</param>
        /// <returns>true if a request with the specified name exists; otherwise, false</returns>
        public bool ContainsRequestName(string name)
        {
            return _requestsByName.ContainsKey(name);
        }
    }
}
