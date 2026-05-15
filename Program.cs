using System;
using System.Windows.Forms;
using ToDoListApp.Database;
using ToDoListApp.Forms;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var databaseInitializer = new DatabaseInitializer();
            databaseInitializer.EnsureSchema();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not initialize the database: {ex.Message}",
                "Database error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var userRepository = new UserRepository();
        var taskRepository = new TaskRepository();
        var authService = new AuthService(userRepository);
        var taskService = new TaskService(taskRepository);
        var adminService = new AdminService(userRepository, taskRepository);
        AppTheme.ApplyUserSettings(new UserSettingsService().LoadLastUsedTheme());

        Application.Run(new LoginForm(authService, taskService, adminService));
    }
}
