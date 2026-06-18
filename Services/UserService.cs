using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using LoginDemo.Models;
using Microsoft.Extensions.Hosting;

namespace LoginDemo.Services;

/// <summary>
/// Lightweight, in-memory user store for demo purposes.
/// Replace with a real database-backed implementation for production use.
/// Registered as a singleton, so data only lives for the lifetime of the process.
/// </summary>
public class UserService : IUserService
{
    private readonly List<User> _users;
    private readonly ConcurrentDictionary<string, (string Token, DateTime ExpiryUtc)> _resetTokens = new();

    public UserService(IHostEnvironment environment)
    {
        _users = environment.IsDevelopment()
            ? new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    FullName = "Administrator",
                    PasswordHash = Hash("Admin@123")
                },
                new User
                {
                    Username = "demo",
                    Email = "demo@example.com",
                    FullName = "Demo User",
                    PasswordHash = Hash("Demo@123")
                }
            }
            : new List<User>();
    }

    public bool ValidateCredentials(string username, string password)
    {
        var user = FindByUsername(username);
        return user is not null && user.PasswordHash == Hash(password);
    }

    public User? FindByUsername(string username) =>
        _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

    public User? FindByEmail(string email) =>
        _users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

    public string GeneratePasswordResetToken(string email)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        _resetTokens[email.ToLowerInvariant()] = (token, DateTime.UtcNow.AddMinutes(30));
        return token;
    }

    public bool ValidateResetToken(string email, string token)
    {
        if (_resetTokens.TryGetValue(email.ToLowerInvariant(), out var entry))
        {
            return entry.Token == token && entry.ExpiryUtc > DateTime.UtcNow;
        }

        return false;
    }

    public bool ResetPassword(string email, string token, string newPassword)
    {
        if (!ValidateResetToken(email, token))
        {
            return false;
        }

        var user = FindByEmail(email);
        if (user is null)
        {
            return false;
        }

        user.PasswordHash = Hash(newPassword);
        _resetTokens.TryRemove(email.ToLowerInvariant(), out _);
        return true;
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
