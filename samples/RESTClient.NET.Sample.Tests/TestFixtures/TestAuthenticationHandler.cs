using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace RESTClient.NET.Sample.Tests.TestFixtures;

/// <summary>
/// Test authentication handler that bypasses JWT authentication in tests
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAuthenticationHandler"/> class
    /// </summary>
    /// <param name="options">Authentication scheme options</param>
    /// <param name="logger">Logger factory</param>
    /// <param name="encoder">URL encoder</param>
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <summary>
    /// Handles authentication by creating test claims
    /// </summary>
    /// <returns>Authentication result with test claims</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract role information from the request headers if available
        // This allows tests to specify different user roles
        string userRole = "Customer"; // Default role
        string userId = "123"; // Default user ID
        string username = "testuser"; // Default username

        if (Request.Headers.TryGetValue("X-Test-User-Role", out StringValues roleHeader))
        {
            userRole = roleHeader.FirstOrDefault() ?? "Customer";
        }

        if (Request.Headers.TryGetValue("X-Test-User-Id", out StringValues userIdHeader))
        {
            userId = userIdHeader.FirstOrDefault() ?? "123";
        }

        if (Request.Headers.TryGetValue("X-Test-Username", out StringValues usernameHeader))
        {
            username = usernameHeader.FirstOrDefault() ?? "testuser";
        }

        // Create test claims based on the request
        Claim[] claims =
        [
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, userRole),
            new("sub", userId),
            new("role", userRole)
        ];

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
