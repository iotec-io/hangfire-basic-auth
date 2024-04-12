using System.Net;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Hangfire.Dashboard;
using System.Text;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Iotec.Hangfire.BasicAuth.Tests;


public class HangfireBasicAuthorizationFilterTests
{
    private readonly HangfireBasicAuthorizationFilter _filter;
    private readonly Mock<JobStorage> _mockJobStorage;

    public HangfireBasicAuthorizationFilterTests()
    {
        Mock<ILogger<HangfireBasicAuthorizationFilter>> mockLogger = new();
        _mockJobStorage = new Mock<JobStorage>();
        _filter = new HangfireBasicAuthorizationFilter(mockLogger.Object)
        {
            User = "testUser",
            Pass = "testPass"
        };
    }

    [Fact]
    public void Authorize_ShouldReturnFalse_WhenMissingAuthorizationHeader()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse("127.0.0.1")
            }
        };
        var dashboardContext = new AspNetCoreDashboardContext(_mockJobStorage.Object, new DashboardOptions(),httpContext);
        var result = _filter.Authorize(dashboardContext);
        Assert.False(result);
    }

    [Fact]
    public void Authorize_ShouldReturnFalse_WhenNotBasicAuthentication()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse("127.0.0.1")
            },
            Request =
            {
                Headers =
                {
                    ["Authorization"] = "Bearer token"
                }
            }
        };

        var dashboardContext = new AspNetCoreDashboardContext(_mockJobStorage.Object, new DashboardOptions(),httpContext);
        var result = _filter.Authorize(dashboardContext);
        Assert.False(result);
    }
    
    [Fact]
    public void Authorize_ShouldReturnFalse_WhenRemoteIpAddress_Is_Missing()
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_filter.User}:{_filter.Pass}"));
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
            Request =
            {
                Headers =
                {
                    ["Authorization"] = $"Basic {credentials}"
                }
            }
        };
        
        var dashboardContext = new AspNetCoreDashboardContext(_mockJobStorage.Object, new DashboardOptions(),httpContext);
        var result = _filter.Authorize(dashboardContext);
        Assert.False(result);
    }

    [Fact]
    public void Authorize_ShouldReturnTrue_WhenCredentialsMatch()
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_filter.User}:{_filter.Pass}"));
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse("127.0.0.1")
            },
            Request =
            {
                Headers =
                {
                    ["Authorization"] = $"Basic {credentials}"
                }
            }
        };
        
        var dashboardContext = new AspNetCoreDashboardContext(_mockJobStorage.Object, new DashboardOptions(),httpContext);
        var result = _filter.Authorize(dashboardContext);
        Assert.True(result);
    }
}