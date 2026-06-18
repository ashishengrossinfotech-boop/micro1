using System.Net;
using System.Text.RegularExpressions;
using LoginDemo.Controllers;
using LoginDemo.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LoginDemo.Tests;

public class ValidationAccessibilityTests
{
    [Fact]
    public async Task FormsExposeValidationMessagesAsLiveRegions()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var loginResponse = await client.GetAsync("/Account/Login");
        var loginMarkup = await loginResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Contains("role=\"alert\"", loginMarkup);
        Assert.Contains("aria-describedby=\"Username-validation\"", loginMarkup);
        Assert.Contains("id=\"Username-validation\"", loginMarkup);
        Assert.Contains("aria-live=\"polite\"", loginMarkup);
        Assert.Contains("aria-describedby=\"Password-validation\"", loginMarkup);
        Assert.Contains("id=\"Password-validation\"", loginMarkup);
        Assert.Contains("aria-invalid=\"false\"", loginMarkup);

        using var forgotResponse = await client.GetAsync("/Account/ForgotPassword");
        var forgotMarkup = await forgotResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, forgotResponse.StatusCode);
        Assert.Contains("role=\"alert\"", forgotMarkup);
        Assert.Contains("aria-describedby=\"Email-validation\"", forgotMarkup);
        Assert.Contains("id=\"Email-validation\"", forgotMarkup);
        Assert.Contains("aria-live=\"polite\"", forgotMarkup);
        Assert.Contains("aria-invalid=\"false\"", forgotMarkup);

        var resetUrl = CreateResetUrl(factory);
        using var resetResponse = await client.GetAsync(resetUrl);
        var resetMarkup = await resetResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);
        Assert.Contains("role=\"alert\"", resetMarkup);
        Assert.Contains("aria-describedby=\"NewPassword-validation\"", resetMarkup);
        Assert.Contains("id=\"NewPassword-validation\"", resetMarkup);
        Assert.Contains("aria-live=\"polite\"", resetMarkup);
        Assert.Contains("aria-describedby=\"ConfirmPassword-validation\"", resetMarkup);
        Assert.Contains("id=\"ConfirmPassword-validation\"", resetMarkup);
        Assert.Contains("aria-invalid=\"false\"", resetMarkup);
    }

    [Fact]
    public void ValidationSummariesAreConfiguredAsAssertiveLiveRegions()
    {
        var repoRoot = FindRepoRoot();
        var viewPaths = new[]
        {
            Path.Combine("Views", "Account", "Login.cshtml"),
            Path.Combine("Views", "Account", "ForgotPassword.cshtml"),
            Path.Combine("Views", "Account", "ResetPassword.cshtml")
        };

        foreach (var viewPath in viewPaths)
        {
            var markup = File.ReadAllText(Path.Combine(repoRoot, viewPath));
            Assert.Contains("asp-validation-summary=\"ModelOnly\"", markup);
            Assert.Contains("role=\"alert\"", markup);
            Assert.Contains("aria-live=\"assertive\"", markup);
        }
    }

    [Fact]
    public async Task InvalidLoginMarksFieldsAsInvalid()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var token = await GetAntiforgeryToken(client, "/Account/Login");
        using var response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, CountOccurrences(markup, "aria-invalid=\"true\""));
        Assert.Contains("Username is required.", markup);
        Assert.Contains("Password is required.", markup);
    }

    [Fact]
    public async Task InvalidForgotPasswordMarksEmailAsInvalid()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var token = await GetAntiforgeryToken(client, "/Account/ForgotPassword");
        using var response = await client.PostAsync(
            "/Account/ForgotPassword",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("aria-invalid=\"true\"", markup);
        Assert.Contains("Email is required.", markup);
    }

    [Fact]
    public async Task InvalidResetPasswordMarksPasswordFieldsAsInvalid()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var resetUrl = CreateResetUrl(factory);
        using var resetPage = await client.GetAsync(resetUrl);
        var resetMarkup = await resetPage.Content.ReadAsStringAsync();
        var antiforgeryToken = ExtractAntiforgeryToken(resetMarkup);

        using var response = await client.PostAsync(
            "/Account/ResetPassword",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "admin@example.com",
                ["Token"] = ExtractQueryValue(resetUrl, "token"),
                ["__RequestVerificationToken"] = antiforgeryToken
            }));
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, CountOccurrences(markup, "aria-invalid=\"true\""));
        Assert.Contains("New password is required.", markup);
        Assert.Contains("Please confirm your new password.", markup);
    }

    private static string CreateResetUrl(WebApplicationFactory<AccountController> factory)
    {
        using var scope = factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var token = userService.GeneratePasswordResetToken("admin@example.com");
        return $"/Account/ResetPassword?email=admin%40example.com&token={Uri.EscapeDataString(token)}";
    }

    private static async Task<string> GetAntiforgeryToken(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        var markup = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return ExtractAntiforgeryToken(markup);
    }

    private static string ExtractAntiforgeryToken(string markup)
    {
        var match = Regex.Match(
            markup,
            "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"(?<value>[^\"]+)\"",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The form should include an antiforgery token.");
        return match.Groups["value"].Value;
    }

    private static string ExtractQueryValue(string url, string name)
    {
        var match = Regex.Match(url, $@"[?&]{Regex.Escape(name)}=(?<value>[^&]+)", RegexOptions.CultureInvariant);
        Assert.True(match.Success, $"The URL should include a {name} query value.");
        return Uri.UnescapeDataString(match.Groups["value"].Value);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LoginDemo.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
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
}
