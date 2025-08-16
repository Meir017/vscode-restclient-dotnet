using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents an HTTP file containing multiple HTTP requests with enhanced request ID functionality
    /// </summary>
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
            _requestsByName = new Dictionary<string, HttpRequest>();
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
                return request;
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
