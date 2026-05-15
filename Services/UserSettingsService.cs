using System.Text.Json;
using ToDoListApp.Models;

namespace ToDoListApp.Services;

public class UserSettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    private readonly string _settingsDirectory;

    public UserSettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ToDoListApp",
            "Settings");
        Directory.CreateDirectory(_settingsDirectory);
    }

    public UserSettingsModel Load(int userId)
    {
        var path = GetSettingsPath(userId);
        if (!File.Exists(path))
        {
            return new UserSettingsModel { UserId = userId };
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<UserSettingsModel>(json) ?? new UserSettingsModel { UserId = userId };
        }
        catch
        {
            return new UserSettingsModel { UserId = userId };
        }
    }

    public void Save(UserSettingsModel settings)
    {
        var json = JsonSerializer.Serialize(settings, SerializerOptions);
        File.WriteAllText(GetSettingsPath(settings.UserId), json);
        File.WriteAllText(GetLastThemePath(), json);
    }

    public UserSettingsModel LoadLastUsedTheme()
    {
        var path = GetLastThemePath();
        if (!File.Exists(path))
        {
            return new UserSettingsModel();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<UserSettingsModel>(json) ?? new UserSettingsModel();
        }
        catch
        {
            return new UserSettingsModel();
        }
    }

    private string GetSettingsPath(int userId)
    {
        return Path.Combine(_settingsDirectory, $"user_{userId}.json");
    }

    private string GetLastThemePath()
    {
        return Path.Combine(_settingsDirectory, "last_theme.json");
    }
}
