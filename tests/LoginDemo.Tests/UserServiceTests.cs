using LoginDemo.Services;
using Xunit;

namespace LoginDemo.Tests;

public class UserServiceTests
{
    [Theory]
    [InlineData(" admin")]
    [InlineData("admin ")]
    [InlineData("  admin  ")]
    public void ValidateCredentials_TrimsUsernameBeforeLookup(string username)
    {
        var service = new UserService();

        var isValid = service.ValidateCredentials(username, "Admin@123");

        Assert.True(isValid);
    }

    [Fact]
    public void FindByUsername_TrimsUsernameBeforeLookup()
    {
        var service = new UserService();

        var user = service.FindByUsername(" demo ");

        Assert.NotNull(user);
        Assert.Equal("demo", user.Username);
    }

    [Fact]
    public void ValidateCredentials_ReturnsFalseForUnknownUser()
    {
        var service = new UserService();

        var isValid = service.ValidateCredentials("missing", "Admin@123");

        Assert.False(isValid);
    }

    [Fact]
    public void FindByEmail_MatchesIgnoringCase()
    {
        var service = new UserService();

        var user = service.FindByEmail("ADMIN@EXAMPLE.COM");

        Assert.NotNull(user);
        Assert.Equal("admin", user.Username);
    }

    [Fact]
    public void ValidateResetToken_ReturnsTrueForGeneratedToken()
    {
        var service = new UserService();
        var token = service.GeneratePasswordResetToken("admin@example.com");

        var isValid = service.ValidateResetToken("ADMIN@EXAMPLE.COM", token);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateResetToken_ReturnsFalseForUnknownToken()
    {
        var service = new UserService();

        var isValid = service.ValidateResetToken("admin@example.com", "bad-token");

        Assert.False(isValid);
    }

    [Fact]
    public void ResetPassword_UpdatesPasswordAndConsumesToken()
    {
        var service = new UserService();
        var token = service.GeneratePasswordResetToken("admin@example.com");

        var reset = service.ResetPassword("admin@example.com", token, "NewAdmin@123");

        Assert.True(reset);
        Assert.True(service.ValidateCredentials("admin", "NewAdmin@123"));
        Assert.False(service.ValidateResetToken("admin@example.com", token));
    }

    [Fact]
    public void ResetPassword_ReturnsFalseForInvalidToken()
    {
        var service = new UserService();

        var reset = service.ResetPassword("admin@example.com", "bad-token", "NewAdmin@123");

        Assert.False(reset);
        Assert.True(service.ValidateCredentials("admin", "Admin@123"));
    }
}
