using ToDoListApp.Database;
using ToDoListApp.Models;
using ToDoListApp.Utils;
using MySql.Data.MySqlClient;

namespace ToDoListApp.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;

    public AuthService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public (bool Success, string Message) Register(string fullName, string email, string password)
    {
        if (!Validator.IsFullNameValid(fullName))
        {
            return (false, "Full name must be at least 3 characters.");
        }

        if (!Validator.IsEmailValid(email))
        {
            return (false, "Please enter a valid email address.");
        }

        if (!Validator.IsPasswordValid(password))
        {
            return (false, "Password must be at least 8 characters and include one uppercase letter and one special character.");
        }

        try
        {
            if (_userRepository.GetByEmail(email.Trim()) is not null)
            {
                return (false, "An account with this email already exists.");
            }

            var user = new UserModel
            {
                FullName = fullName.Trim(),
                Email = email.Trim(),
                Role = "user",
                PasswordHash = Helpers.HashPassword(password)
            };

            _userRepository.CreateUser(user);
            return (true, "Account created successfully.");
        }
        catch (MySqlException ex)
        {
            return (false, $"Database error ({ex.Number}): {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            return (false, "Connection configuration is missing or invalid in App.config.");
        }
        catch (Exception ex)
        {
            return (false, $"Database error: {ex.Message}");
        }
    }

    public UserModel? Login(string email, string password)
    {
        if (Validator.IsNullOrWhiteSpace(email, password))
        {
            return null;
        }

        try
        {
            var user = _userRepository.GetByEmail(email.Trim());
            if (user is null)
            {
                return null;
            }

            var hash = Helpers.HashPassword(password);
            return user.PasswordHash.Equals(hash, StringComparison.OrdinalIgnoreCase) ? user : null;
        }
        catch (MySqlException ex)
        {
            MessageBox.Show(
                $"Database error ({ex.Number}): {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return null;
        }
        catch (InvalidOperationException)
        {
            MessageBox.Show(
                "Connection configuration is missing or invalid in App.config.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Database error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return null;
        }
    }
}
