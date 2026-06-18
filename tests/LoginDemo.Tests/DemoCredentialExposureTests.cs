using LoginDemo.Controllers;
using LoginDemo.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LoginDemo.Tests;

public class DemoCredentialExposureTests
{
    [Fact]
    public async Task LoginPage_ShowsDemoCredentialsInDevelopment()
    {
        using var factory = CreateFactory("Development");
        var client = factory.CreateClient();

        var html = await client.GetStringAsync("/Account/Login");

        Assert.Contains("Local demo accounts", html);
        Assert.Contains("admin / Admin@123", html);
        Assert.Contains("demo / Demo@123", html);
    }

    [Fact]
    public async Task LoginPage_HidesDemoCredentialsOutsideDevelopment()
    {
        using var factory = CreateFactory("Production");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var html = await client.GetStringAsync("/Account/Login");

        Assert.DoesNotContain("Local demo accounts", html);
        Assert.DoesNotContain("admin / Admin@123", html);
        Assert.DoesNotContain("demo / Demo@123", html);
    }

    [Fact]
    public void UserService_SeedsDemoUsersInDevelopment()
    {
        using var factory = CreateFactory("Development");
        var service = GetUserService(factory);

        Assert.True(service.ValidateCredentials("admin", "Admin@123"));
        Assert.True(service.ValidateCredentials("demo", "Demo@123"));
    }

    [Fact]
    public void UserService_DoesNotSeedDemoUsersOutsideDevelopment()
    {
        using var factory = CreateFactory("Production");
        var service = GetUserService(factory);

        Assert.False(service.ValidateCredentials("admin", "Admin@123"));
        Assert.False(service.ValidateCredentials("demo", "Demo@123"));
        Assert.Null(service.FindByUsername("admin"));
        Assert.Null(service.FindByEmail("demo@example.com"));
    }

    [Fact]
    public void FindByEmail_MatchesSeededDevelopmentUserIgnoringCase()
    {
        using var factory = CreateFactory("Development");
        var service = GetUserService(factory);

        var user = service.FindByEmail("ADMIN@EXAMPLE.COM");

        Assert.NotNull(user);
        Assert.Equal("admin", user.Username);
    }

    [Fact]
    public void ValidateResetToken_ReturnsTrueForGeneratedDevelopmentToken()
    {
        using var factory = CreateFactory("Development");
        var service = GetUserService(factory);
        var token = service.GeneratePasswordResetToken("admin@example.com");

        var isValid = service.ValidateResetToken("ADMIN@EXAMPLE.COM", token);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateResetToken_ReturnsFalseForUnknownToken()
    {
        using var factory = CreateFactory("Development");
        var service = GetUserService(factory);

        var isValid = service.ValidateResetToken("admin@example.com", "bad-token");

        Assert.False(isValid);
    }

    [Fact]
    public void ResetPassword_UpdatesPasswordAndConsumesToken()
    {
        using var factory = CreateFactory("Development");
        var service = GetUserService(factory);
        var token = service.GeneratePasswordResetToken("admin@example.com");

        var reset = service.ResetPassword("admin@example.com", token, "NewAdmin@123");

        Assert.True(reset);
        Assert.True(service.ValidateCredentials("admin", "NewAdmin@123"));
        Assert.False(service.ValidateCredentials("admin", "Admin@123"));
        Assert.False(service.ValidateResetToken("admin@example.com", token));
    }

    [Fact]
    public void ResetPassword_ReturnsFalseForInvalidToken()
    {
        using var factory = CreateFactory("Development");
        var service = GetUserService(factory);

        var reset = service.ResetPassword("admin@example.com", "bad-token", "NewAdmin@123");

        Assert.False(reset);
        Assert.True(service.ValidateCredentials("admin", "Admin@123"));
    }

    [Fact]
    public void ResetPassword_ReturnsFalseWhenTokenEmailHasNoSeededUser()
    {
        using var factory = CreateFactory("Development");
        var service = GetUserService(factory);
        var token = service.GeneratePasswordResetToken("missing@example.com");

        var reset = service.ResetPassword("missing@example.com", token, "NewAdmin@123");

        Assert.False(reset);
    }

    private static IUserService GetUserService(WebApplicationFactory<AccountController> factory) =>
        factory.Services.GetRequiredService<IUserService>();

    private static WebApplicationFactory<AccountController> CreateFactory(string environmentName) =>
        new WebApplicationFactory<AccountController>()
            .WithWebHostBuilder(builder => builder.UseEnvironment(environmentName));
}
