namespace ToDoListApp.Utils;

/// <summary>
/// Password row with masking + Show/Hide toggle (multiline must stay false for masking).
/// </summary>
public static class PasswordRevealHelper
{
    public static Panel CreatePasswordRow(TextBox passwordBox, int totalWidth)
    {
        var row = new Panel
        {
            Width = totalWidth,
            Height = 42,
            BackColor = AppTheme.Input,
            Padding = new Padding(14, 10, 0, 0)
        };

        passwordBox.Multiline = false;
        passwordBox.BorderStyle = BorderStyle.None;
        passwordBox.UseSystemPasswordChar = true;
        passwordBox.Font = new Font("Segoe UI", 11f);
        passwordBox.ForeColor = AppTheme.TextPrimary;
        passwordBox.BackColor = row.BackColor;

        var toggle = new Button
        {
            Text = "Show",
            Width = 76,
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f),
            ForeColor = AppTheme.TextMuted,
            BackColor = row.BackColor
        };
        toggle.FlatAppearance.BorderSize = 0;
        toggle.Click += (_, _) =>
        {
            passwordBox.UseSystemPasswordChar = !passwordBox.UseSystemPasswordChar;
            toggle.Text = passwordBox.UseSystemPasswordChar ? "Show" : "Hide";
        };

        passwordBox.Dock = DockStyle.Fill;
        row.Controls.Add(toggle);
        row.Controls.Add(passwordBox);
        row.Resize += (_, _) => AppTheme.ApplyRoundedRegion(row, 18);
        row.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(row, 18);

        return row;
    }
}
