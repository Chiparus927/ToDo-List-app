namespace ToDoListApp.Models;

public class UserModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public bool IsAdmin => Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
}
