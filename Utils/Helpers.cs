using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ToDoListApp.Utils;

public static class Helpers
{
    public static string HashPassword(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public static void ShowInfo(string message, string title = "Info")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
