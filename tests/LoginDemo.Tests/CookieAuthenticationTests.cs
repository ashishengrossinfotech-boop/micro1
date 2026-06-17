using LoginDemo.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace LoginDemo.Tests;

public class CookieAuthenticationTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void AuthCookieRequiresSecureTransport(string environmentName)
    {
        using var factory = new WebApplicationFactory<AccountController>()
            .WithWebHostBuilder(builder => builder.UseEnvironment(environmentName));

        var options = factory.Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
    }
}
