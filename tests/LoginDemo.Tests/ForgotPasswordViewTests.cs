using System.Net;
using LoginDemo.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LoginDemo.Tests;

public class ForgotPasswordViewTests
{
    [Fact]
    public async Task ForgotPasswordPageUsesProductionSafeResetCopy()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/Account/ForgotPassword");
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("send reset instructions if the account exists", markup);
        Assert.DoesNotContain("show you a reset link", markup);
    }
}
