namespace ToDoListApp.Utils;

public static class PasswordRevealHelper
{
    public static Panel CreatePasswordRow(TextBox passwordBox, int totalWidth)
    {
        var row = new Panel
        {
            Width = totalWidth,
            Height = 48,
            BackColor = AppTheme.Input,
            Padding = new Padding(50, 13, 86, 0)
        };

        var icon = new Label
        {
            Text = "●",
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            Location = new Point(17, 13),
            Size = new Size(22, 22),
            TextAlign = ContentAlignment.MiddleCenter
        };

        passwordBox.Multiline = false;
        passwordBox.BorderStyle = BorderStyle.None;
        passwordBox.UseSystemPasswordChar = true;
        passwordBox.Font = new Font("Segoe UI", 11f);
        passwordBox.ForeColor = AppTheme.TextPrimary;
        passwordBox.BackColor = row.BackColor;
        passwordBox.Dock = DockStyle.Fill;

        var toggle = new Button
        {
            Text = "Show",
            Width = 76,
            Height = 32,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(totalWidth - 88, 8),
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            ForeColor = AppTheme.TextMuted,
            BackColor = row.BackColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, 0, 0, 1)
        };
        toggle.FlatAppearance.BorderSize = 0;
        toggle.MouseEnter += (_, _) => toggle.ForeColor = AppTheme.Primary;
        toggle.MouseLeave += (_, _) => toggle.ForeColor = AppTheme.TextMuted;
        toggle.Click += (_, _) =>
        {
            passwordBox.UseSystemPasswordChar = !passwordBox.UseSystemPasswordChar;
            toggle.Text = passwordBox.UseSystemPasswordChar ? "Show" : "Hide";
        };

        row.Controls.Add(toggle);
        row.Controls.Add(passwordBox);
        row.Controls.Add(icon);
        row.Resize += (_, _) => AppTheme.ApplyRoundedRegion(row, 18);
        row.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(row, 18);
        row.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = AppTheme.RoundedRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 18);
            using var pen = new Pen(passwordBox.Focused ? AppTheme.Primary : Color.FromArgb(232, 235, 241), passwordBox.Focused ? 2 : 1);
            e.Graphics.DrawPath(pen, path);
        };
        passwordBox.GotFocus += (_, _) => row.Invalidate();
        passwordBox.LostFocus += (_, _) => row.Invalidate();

        return row;
    }
}
