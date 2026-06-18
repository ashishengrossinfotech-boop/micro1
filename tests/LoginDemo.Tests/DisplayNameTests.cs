using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using LoginDemo.Controllers;
using LoginDemo.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LoginDemo.Tests;

public class DisplayNameTests
{
    [Fact]
    public async Task HomePageUsesFullNameClaimForAuthenticatedUsers()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        using var loginPage = await client.GetAsync("/Account/Login");
        var loginMarkup = await loginPage.Content.ReadAsStringAsync();
        var antiforgeryToken = ExtractAntiforgeryToken(loginMarkup);

        using var loginResponse = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Username"] = "admin",
                ["Password"] = "Admin@123",
                ["__RequestVerificationToken"] = antiforgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        using var response = await client.GetAsync("/Home/Index");
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Welcome, Administrator!", markup);
        Assert.Contains("Hi, Administrator", markup);
        Assert.DoesNotContain("Welcome, admin!", markup);
    }

    [Fact]
    public void GetDisplayNameFallsBackToIdentityName()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "admin") },
            CookieAuthenticationDefaults.AuthenticationScheme);

        var displayName = new ClaimsPrincipal(identity).GetDisplayName();

        Assert.Equal("admin", displayName);
    }

    [Fact]
    public void GetDisplayNameFallsBackToSafeGenericName()
    {
        var identity = new ClaimsIdentity(
            Array.Empty<Claim>(),
            CookieAuthenticationDefaults.AuthenticationScheme);

        var displayName = new ClaimsPrincipal(identity).GetDisplayName();

        Assert.Equal("there", displayName);
    }

    private static string ExtractAntiforgeryToken(string markup)
    {
        var match = Regex.Match(
            markup,
            "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<value>[^\"]+)\"",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The login form should include an antiforgery token.");
        return match.Groups["value"].Value;
    }
}
