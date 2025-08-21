using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Manages storage and retrieval of HTTP response data for request chaining
    /// Supports variables like {{requestName.response.body.$.token}}
    /// </summary>
    public class ResponseContext
    {
        private readonly ConcurrentDictionary<string, HttpResponseData> _responses = new();

        /// <summary>
        /// Gets all stored responses
        /// </summary>
        public IReadOnlyDictionary<string, HttpResponseData> Responses => _responses;

        /// <summary>
        /// Stores a response for a named request
        /// </summary>
        /// <param name="requestName">The name of the request (from # @name comment)</param>
        /// <param name="responseData">The response data to store</param>
        public void StoreResponse(string requestName, HttpResponseData responseData)
        {
            if (string.IsNullOrEmpty(requestName))
            {
                throw new ArgumentException("Request name cannot be null or empty", nameof(requestName));
            }

            ArgumentNullException.ThrowIfNull(responseData);

            _responses[requestName] = responseData;
        }

        /// <summary>
        /// Gets a stored response by request name
        /// </summary>
        /// <param name="requestName">The name of the request</param>
        /// <returns>The response data, or null if not found</returns>
        public HttpResponseData? GetResponse(string requestName)
        {
            if (string.IsNullOrEmpty(requestName))
            {
                return null;
            }

            _responses.TryGetValue(requestName, out HttpResponseData? response);
            return response;
        }

        /// <summary>
        /// Checks if a response exists for the given request name
        /// </summary>
        /// <param name="requestName">The name of the request</param>
        /// <returns>True if response exists, false otherwise</returns>
        public bool HasResponse(string requestName)
        {
            return !string.IsNullOrEmpty(requestName) && _responses.ContainsKey(requestName);
        }

        /// <summary>
        /// Removes a stored response
        /// </summary>
        /// <param name="requestName">The name of the request</param>
        /// <returns>True if response was removed, false if it didn't exist</returns>
        public bool RemoveResponse(string requestName)
        {
            if (string.IsNullOrEmpty(requestName))
            {
                return false;
            }

            return _responses.TryRemove(requestName, out _);
        }

        /// <summary>
        /// Clears all stored responses
        /// </summary>
        public void Clear()
        {
            _responses.Clear();
        }

        /// <summary>
        /// Gets the number of stored responses
        /// </summary>
        public int Count => _responses.Count;

        /// <summary>
        /// Gets all request names that have stored responses
        /// </summary>
        public IEnumerable<string> RequestNames => _responses.Keys;

        /// <summary>
        /// Creates a copy of the current response context
        /// </summary>
        /// <returns>A new ResponseContext with copied response data</returns>
        public ResponseContext Clone()
        {
            var clone = new ResponseContext();
            foreach (KeyValuePair<string, HttpResponseData> kvp in _responses)
            {
                clone._responses[kvp.Key] = kvp.Value;
            }
            return clone;
        }
    }
}
