using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RESTClient.NET.Sample.Tests.TestFixtures;

/// <summary>
/// Test authentication handler that bypasses JWT authentication in tests
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract role information from the request headers if available
        // This allows tests to specify different user roles
        string userRole = "Customer"; // Default role
        string userId = "123"; // Default user ID
        string username = "testuser"; // Default username

        if (Request.Headers.ContainsKey("X-Test-User-Role"))
        {
            userRole = Request.Headers["X-Test-User-Role"].FirstOrDefault() ?? "Customer";
        }

        if (Request.Headers.ContainsKey("X-Test-User-Id"))
        {
            userId = Request.Headers["X-Test-User-Id"].FirstOrDefault() ?? "123";
        }

        if (Request.Headers.ContainsKey("X-Test-Username"))
        {
            username = Request.Headers["X-Test-Username"].FirstOrDefault() ?? "testuser";
        }

        // Create test claims based on the request
        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, userRole),
            new Claim("sub", userId),
            new Claim("role", userRole)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
