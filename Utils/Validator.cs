namespace ToDoListApp.Utils;

public static class Validator
{
    public static bool IsNullOrWhiteSpace(params string[] values)
    {
        return values.Any(string.IsNullOrWhiteSpace);
    }

    public static bool IsFullNameValid(string fullName)
    {
        return !string.IsNullOrWhiteSpace(fullName) && fullName.Trim().Length >= 3;
    }

    public static bool IsEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            _ = new System.Net.Mail.MailAddress(email.Trim());
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsPasswordValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return false;
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));
        return hasUpper && hasSpecial;
    }
}
