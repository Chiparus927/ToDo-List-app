namespace ToDoListApp.Models;

public class UserSettingsModel
{
    public int UserId { get; set; }
    public bool DarkMode { get; set; }
    public string AccentColor { get; set; } = "#0A84FF";
    public bool TransparencyEnabled { get; set; } = true;
    public bool BlurEnabled { get; set; } = true;
    public bool TaskCompletedNotifications { get; set; } = true;
    public bool TaskReminders { get; set; } = true;
    public bool NotificationSounds { get; set; } = true;
    public bool DesktopNotifications { get; set; } = true;
    public bool TwoFactorEnabled { get; set; }
    public string? ProfileImagePath { get; set; }
    public DateTime LastLoginAt { get; set; } = DateTime.Now;
}
