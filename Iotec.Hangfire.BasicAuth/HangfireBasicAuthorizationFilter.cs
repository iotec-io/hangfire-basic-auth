using System.Net.Http.Headers;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

namespace Iotec.Hangfire.BasicAuth;

public class HangfireBasicAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly ILogger _logger;
    public required string User { get; set; }
    public required string Pass { get; set; }

    private const string AuthenticationScheme = "Basic";
    private const int BlockAfterFailedAttempts = 5;
    private const int BlockTimeInMinutes = 15;

    private readonly Dictionary<string, (int Count, DateTime LastAttempt)> _failedAttempts = new();


    public HangfireBasicAuthorizationFilter() : this(
        new NullLogger<HangfireBasicAuthorizationFilter>())
    {
    }

    public HangfireBasicAuthorizationFilter(ILogger logger)
    {
        _logger = logger;
    }

    // https://gist.github.com/ndc/a1cc8e2515e5e0d941a884fc6a6267f5
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers["Authorization"];
        
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        
        if (ipAddress == null)
        {
            _logger.LogWarning("Could not determine the remote IP address. Access denied");
            return false;
        }

        if (_failedAttempts.TryGetValue(ipAddress, out var attempt))
        {
            var (count, lastAttempt) = attempt;

            if (count >= BlockAfterFailedAttempts && lastAttempt.AddMinutes(BlockTimeInMinutes) > DateTime.Now)
            {
                _logger.LogInformation("Blocked IP {IP} due to too many failed login attempts", ipAddress);
                SetChallengeResponse(httpContext,429);
                return false;
            }
        }

        if (Missing_Authorization_Header(header))
        {
            _logger.LogInformation("Request is missing Authorization Header");
            SetChallengeResponse(httpContext);
            return false;
        }

        var authValues = AuthenticationHeaderValue.Parse(header);

        if (Not_Basic_Authentication(authValues))
        {
            _logger.LogInformation("Request is NOT BASIC authentication");
            SetChallengeResponse(httpContext);
            return false;
        }

        var tokens = Extract_Authentication_Tokens(authValues);

        if (tokens.Are_Invalid())
        {
            _logger.LogInformation("Authentication tokens are invalid (empty, null, whitespace)");
            SetChallengeResponse(httpContext);
            return false;
        }

        if (tokens.Credentials_Match(User, Pass))
        {
            _logger.LogInformation("Awesome, authentication tokens match configuration!");
            _failedAttempts.Remove(ipAddress); // remove the IP from the failed attempts if the login is successful
            return true;
        }

        _logger.LogInformation("Boo! Authentication tokens [{Username}] [{Password}] do not match configuration", tokens.Username, Masked(tokens.Password));
        if (_failedAttempts.TryGetValue(ipAddress, out var value))
        {
            _failedAttempts[ipAddress] = (value.Count + 1, DateTime.Now);
        }
        else
        {
            _failedAttempts[ipAddress] = (1, DateTime.Now);
        }
        SetChallengeResponse(httpContext);
        return false;
    }

    private static bool Missing_Authorization_Header(StringValues header)
    {
        return string.IsNullOrWhiteSpace(header);
    }

    private static BasicAuthenticationTokens Extract_Authentication_Tokens(AuthenticationHeaderValue authValues)
    {
        var parameter = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter!));
        var parts = parameter.Split(':');
        return new BasicAuthenticationTokens(parts);
    }

    private static bool Not_Basic_Authentication(AuthenticationHeaderValue authValues)
    {
        return !AuthenticationScheme.Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase);
    }

    private void SetChallengeResponse(HttpContext httpContext, int statusCode = 401)
    {
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        httpContext.Response.WriteAsync("Authentication is required.");
    }

    private string Masked(string str)
    {
        return new string('*', str.Length);
    }
}