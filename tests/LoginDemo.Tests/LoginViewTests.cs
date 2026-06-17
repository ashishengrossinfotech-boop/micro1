using System.Net;
using LoginDemo.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LoginDemo.Tests;

public class LoginViewTests
{
    [Fact]
    public async Task LoginPageRendersUsernameInputWithEmailMobilePlaceholder()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/Account/Login");
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("name=\"Username\"", markup);
        Assert.Contains("placeholder=\"Email/Mobile\"", markup);
    }
}
