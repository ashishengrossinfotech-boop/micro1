using LoginDemo.Models;

namespace LoginDemo.Services;

public interface IUserService
{
    bool ValidateCredentials(string username, string password);
    User? FindByUsername(string username);
    User? FindByEmail(string email);
    string GeneratePasswordResetToken(string email);
    bool ValidateResetToken(string email, string token);
    bool ResetPassword(string email, string token, string newPassword);
}
