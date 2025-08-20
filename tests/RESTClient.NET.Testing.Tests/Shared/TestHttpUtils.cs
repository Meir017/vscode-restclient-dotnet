using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using RESTClient.NET.Core.Models;

namespace RESTClient.NET.Testing.Tests.Shared;

/// <summary>
/// Utility class for test HTTP-related helpers.
/// </summary>
internal static class TestHttpUtils
{
    /// <summary>
    /// Determines if a header is a content header that should be added to HttpContent instead of HttpRequestMessage
    /// </summary>
    /// <param name="headerName">The header name to check</param>
    /// <returns>True if the header is a content header</returns>
    public static bool IsContentHeader(string headerName)
    {
        return headerName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Language", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Location", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-MD5", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Content-Range", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Expires", StringComparison.OrdinalIgnoreCase) ||
               headerName.Equals("Last-Modified", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates HttpContent from body and headers, handling content-type properly to avoid HTTP 415 errors
    /// </summary>
    /// <param name="body">The request body content</param>
    /// <param name="headers">The headers dictionary</param>
    /// <returns>HttpContent with proper content-type header</returns>
    public static HttpContent CreateHttpContent(string body, IDictionary<string, string> headers)
    {
        if (string.IsNullOrEmpty(body))
            return null!;

        // Get the content type from headers for proper StringContent creation
        var contentType = headers.TryGetValue("Content-Type", out var ctValue) 
            ? ctValue 
            : "text/plain";
        
        // Create StringContent with proper content type to avoid HTTP 415 errors
        var content = new StringContent(body, Encoding.UTF8, contentType);
        
        // Add other content headers (excluding Content-Type which is already set)
        foreach (var header in headers)
        {
            if (IsContentHeader(header.Key) && !header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return content;
    }

    /// <summary>
    /// Creates HttpContent from body with default content type
    /// </summary>
    /// <param name="body">The request body content</param>
    /// <param name="contentType">Optional content type (defaults to text/plain)</param>
    /// <returns>HttpContent with specified content type</returns>
    public static HttpContent CreateHttpContent(string body, string contentType = "text/plain")
    {
        if (string.IsNullOrEmpty(body))
            return null!;

        return new StringContent(body, Encoding.UTF8, contentType);
    }

    /// <summary>
    /// Creates an HttpRequestMessage from a processed request
    /// </summary>
    /// <param name="processedRequest">The processed request to convert</param>
    /// <returns>HttpRequestMessage ready for sending</returns>
    public static HttpRequestMessage CreateHttpRequestMessage(HttpRequest processedRequest)
    {
        if (processedRequest == null)
            throw new ArgumentNullException(nameof(processedRequest));

        var request = new HttpRequestMessage(new HttpMethod(processedRequest.Method), processedRequest.Url);
        
        // Add headers (non-content headers)
        foreach (var header in processedRequest.Headers)
        {
            if (!IsContentHeader(header.Key))
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        
        // Add body content if present
        if (!string.IsNullOrEmpty(processedRequest.Body))
        {
            request.Content = CreateHttpContent(processedRequest.Body, processedRequest.Headers);
        }

        return request;
    }
}
