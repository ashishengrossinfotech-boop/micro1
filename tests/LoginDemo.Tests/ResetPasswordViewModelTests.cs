using System.ComponentModel.DataAnnotations;
using LoginDemo.Models;
using Xunit;

namespace LoginDemo.Tests;

public class ResetPasswordViewModelTests
{
    [Theory]
    [InlineData("Short1!")]
    [InlineData("lowercase1!")]
    [InlineData("UPPERCASE1!")]
    [InlineData("NoNumber!")]
    [InlineData("NoSpecial1")]
    public void NewPassword_RejectsWeakPasswords(string password)
    {
        var model = ValidModel(password);

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ResetPasswordViewModel.NewPassword)));
    }

    [Fact]
    public void NewPassword_AcceptsStrongPassword()
    {
        var model = ValidModel("StrongPass1!");

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void ConfirmPassword_MustMatchNewPassword()
    {
        var model = ValidModel("StrongPass1!");
        model.ConfirmPassword = "Different1!";

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ResetPasswordViewModel.ConfirmPassword)));
    }

    private static ResetPasswordViewModel ValidModel(string password) => new()
    {
        Email = "admin@example.com",
        Token = "valid-token",
        NewPassword = password,
        ConfirmPassword = password
    };

    private static List<ValidationResult> Validate(ResetPasswordViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        return results;
    }
}
