using RESTClient.NET.Core.Models;
using RESTClient.NET.Core.Validation;

namespace RESTClient.NET.Core.Parsing
{
    /// <summary>
    /// Interface for HTTP file validation
    /// </summary>
    public interface IHttpFileValidator
    {
        /// <summary>
        /// Validates an HTTP file
        /// </summary>
        /// <param name="httpFile">The HTTP file to validate</param>
        /// <returns>The validation result</returns>
        ValidationResult Validate(HttpFile httpFile);
    }
}
