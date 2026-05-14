using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class SettingsForm : Form
{
    public SettingsForm()
    {
        Text = "Settings";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(420, 250);
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        BackColor = AppTheme.Background;
        var panel = AppTheme.CreateCard(new Rectangle(20, 20, 360, 170));
        panel.BackColor = AppTheme.Background;

        var title = new Label
        {
            Text = "App settings",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Location = new Point(25, 18),
            AutoSize = true,
            ForeColor = AppTheme.TextPrimary
        };

        var hint = new Label
        {
            Text = "You can extend this screen later with theme, notifications, or preferences.",
            Location = new Point(25, 62),
            AutoSize = true,
            ForeColor = AppTheme.TextMuted
        };

        var btnClose = new Button
        {
            Text = "Close",
            Width = 120,
            Height = 35,
            Location = new Point(25, 108),
            FlatStyle = FlatStyle.Flat
        };
        AppTheme.StylePrimaryButton(btnClose);
        btnClose.Click += (_, _) => Close();

        panel.Controls.AddRange([title, hint, btnClose]);
        Controls.Add(panel);
    }
}
