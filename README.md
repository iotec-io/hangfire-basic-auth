# Iotec.Hangfire.BasicAuth

## Introduction

Iotec.Hangfire.BasicAuth is a project that provides basic authentication for the Hangfire Dashboard. Hangfire is an open-source framework that enables the creation, processing, and management of background jobs, i.e., operations that are not suitable for the request processing pipeline. This project significantly enhances the security of the Hangfire Dashboard by introducing a basic authentication mechanism. 

This mechanism requires a username and password to access the dashboard, thereby ensuring that only authorized users can access it. More importantly, it includes robust features to prevent brute force attacks. After a specified number of failed login attempts, the user's access is blocked. The duration of this block can also be customized. These features provide a robust defense against brute force attacks, making Iotec.Hangfire.BasicAuth an essential tool for securing your Hangfire Dashboard.

## Installation

Follow these steps to install the project:
1. Open your project in Visual Studio.
2. Right-click on your project in the Solution Explorer and select "Manage NuGet Packages..."
3. In the NuGet Package Manager, click on the "Browse" tab.
4. In the search box, type "Iotec.Hangfire.BasicAuth" and press enter.
5. Find "Iotec.Hangfire.BasicAuth" in the search results and click on it.
6. Click on the "Install" button to install the package.
7. Accept any license agreements if prompted.
8. Once the installation is complete, you can start using the package in your project.

## Sample Code

Here is a sample code snippet:
```csharp
builder.UseHangfireDashboard(settings.DashboardUrl, new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireBasicAuthorizationFilter(logger)
        {
            User = settings.Username,
            Pass = settings.Password,
            BlockAfterFailedAttempts = settings.BlockAfterFailedAttempts,
            BlockTimeInMinutes = settings.BlockTimeInMinutes
        }
    }
});
```


This code snippet is configuring the **Hangfire Dashboard**, a UI interface that allows you to manage background jobs, see their status, and perform actions on them.

The `UseHangfireDashboard` method is being called on the `builder` object, and it's being passed two arguments:

- `settings.DashboardUrl`: This is the URL where the Hangfire Dashboard will be accessible from.

- `new DashboardOptions`: This is an object that contains various configuration options for the Hangfire Dashboard. In this case, it's being used to set up basic authentication for the dashboard.

The `Authorization` property of `DashboardOptions` is being set to an array that contains a single `HangfireBasicAuthorizationFilter` object. This object is configured with the following properties:

- `User`: The username required to access the Hangfire Dashboard.
- `Pass`: The password required to access the Hangfire Dashboard.
- `BlockAfterFailedAttempts`: The number of failed login attempts after which the user will be blocked.
- `BlockTimeInMinutes`: The amount of time (in minutes) that the user will be blocked after exceeding the maximum number of failed login attempts.

This setup ensures that only authorized users can access the Hangfire Dashboard, and it provides some basic protection against brute force attacks.