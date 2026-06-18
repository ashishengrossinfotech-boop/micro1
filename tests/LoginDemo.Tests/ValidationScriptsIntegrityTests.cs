using System.Net;
using LoginDemo.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LoginDemo.Tests;

public class ValidationScriptsIntegrityTests
{
    [Fact]
    public async Task ValidationScriptsPartialAddsIntegrityAttributesToCdnScripts()
    {
        await using var factory = new WebApplicationFactory<AccountController>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/Account/Login");
        var markup = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("https://cdn.jsdelivr.net/npm/jquery@3.7.1/dist/jquery.min.js", markup);
        Assert.Contains("integrity=\"sha384-1H217gwSVyLSIfaLxHbE7dRb3v4mYCKbpQvzx0cegeju1MVsGrX5xXxAvs/HgeFs\"", markup);
        Assert.Contains("https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js", markup);
        Assert.Contains("integrity=\"sha384-aEDtD4n2FLrMdE9psop0SHdNyy/W9cBjH22rSRp+3wPHd62Y32uijc0H2eLmgaSn\"", markup);
        Assert.Contains("https://cdn.jsdelivr.net/npm/jquery-validation-unobtrusive@4.0.0/dist/jquery.validate.unobtrusive.min.js", markup);
        Assert.Contains("integrity=\"sha384-DU2a51mTHKDhpXhTyJQ++hP8L9L8Gc48TlvbzBmUof71V7kNVs4ELmaVJKPxcAGn\"", markup);
        Assert.Equal(3, CountOccurrences(markup, "crossorigin=\"anonymous\""));
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
