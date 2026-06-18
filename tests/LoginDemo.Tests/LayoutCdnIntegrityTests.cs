using System.Net;
using System.Text.RegularExpressions;
using LoginDemo.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LoginDemo.Tests;

public class LayoutCdnIntegrityTests
{
    [Fact]
    public async Task LayoutAddsIntegrityAttributesToBootstrapCdnAssets()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/Account/Login");
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css", markup);
        Assert.Contains("integrity=\"sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH\"", markup);
        Assert.Contains("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js", markup);
        Assert.Contains("integrity=\"sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz\"", markup);
        Assert.Equal(2, CountOccurrences(markup, "crossorigin=\"anonymous\""));
    }

    [Fact]
    public async Task AuthenticatedLayoutKeepsBootstrapIntegrityAttributes()
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
        Assert.Contains("Hi, admin", markup);
        Assert.Contains("integrity=\"sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH\"", markup);
        Assert.Contains("integrity=\"sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz\"", markup);
        Assert.Equal(2, CountOccurrences(markup, "crossorigin=\"anonymous\""));
    }

    private static int CountOccurrences(string value, string search)
    {
        var count = 0;
        var index = 0;

        while ((index = value.IndexOf(search, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += search.Length;
        }

        return count;
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
