using ToDoListApp.Controls;
using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class SettingsForm : Form, IThemeAware
{
    private readonly UserModel _user;
    private readonly UserSettingsService _settingsService = new();
    private readonly UserSettingsModel _settings;
    private readonly Panel _contentHost = new();
    private readonly List<Button> _navButtons = new();
    private RoundedPanel? _shell;
    private Panel? _sidebar;
    private Label? _sidebarTitle;
    private Label? _sidebarSubtitle;
    private string _activeSection = "Profile";

    public SettingsForm(UserModel user)
    {
        _user = user;
        _settings = _settingsService.Load(user.Id);
        AppTheme.ApplyUserSettings(_settings);
        Text = "Settings";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1120, 740);
        MinimumSize = new Size(980, 640);
        AppTheme.StyleForm(this, new Size(980, 640));
        InitializeComponents();
        ShowSection("Profile");
    }

    public SettingsForm() : this(new UserModel { Id = 0, FullName = "User", Email = "user@example.com", Role = "user" })
    {
    }

    private void InitializeComponents()
    {
        BackColor = BackgroundColor;
        Paint += (_, e) => PaintGlassBackground(e.Graphics, ClientRectangle);

        _shell = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(18),
            Radius = 30,
            BackColor = ShellColor,
            Padding = new Padding(0)
        };
        Controls.Add(_shell);

        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 260,
            BackColor = SidebarColor
        };

        _sidebarTitle = new Label
        {
            Text = "Settings",
            Location = new Point(28, 28),
            Size = new Size(190, 38),
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            ForeColor = TextColor
        };
        _sidebarSubtitle = new Label
        {
            Text = _user.IsAdmin ? "Admin workspace" : "Personal workspace",
            Location = new Point(30, 68),
            Size = new Size(190, 22),
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = MutedColor
        };
        _sidebar.Controls.AddRange([_sidebarTitle, _sidebarSubtitle]);

        var sections = new List<string> { "Profile", "Appearance", "Notifications", "Security", "Data & Storage" };
        if (_user.IsAdmin)
        {
            sections.Add("Admin Settings");
        }
        sections.Add("About");

        var top = 124;
        foreach (var section in sections)
        {
            var button = CreateNavButton(section, top);
            _sidebar.Controls.Add(button);
            _navButtons.Add(button);
            top += 52;
        }

        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = ShellColor;
        _contentHost.Padding = new Padding(34, 30, 34, 30);

        _shell.Controls.Add(_contentHost);
        _shell.Controls.Add(_sidebar);
    }

    private Button CreateNavButton(string text, int top)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(18, top),
            Size = new Size(224, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = SidebarColor,
            ForeColor = TextColor,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => ShowSection(text);
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 15);
        button.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(button, 15);
        return button;
    }

    private void ShowSection(string section)
    {
        ApplyStaticTheme();
        _activeSection = section;
        foreach (var button in _navButtons)
        {
            var active = button.Text == section;
            button.BackColor = active ? ActiveNavColor : SidebarColor;
            button.ForeColor = active ? AccentColor : TextColor;
        }

        _contentHost.SuspendLayout();
        _contentHost.Controls.Clear();
        var page = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ShellColor };
        _contentHost.Controls.Add(page);

        switch (section)
        {
            case "Profile":
                BuildProfile(page);
                break;
            case "Appearance":
                BuildAppearance(page);
                break;
            case "Notifications":
                BuildNotifications(page);
                break;
            case "Security":
                BuildSecurity(page);
                break;
            case "Data & Storage":
                BuildDataStorage(page);
                break;
            case "Admin Settings":
                BuildAdminSettings(page);
                break;
            default:
                BuildAbout(page);
                break;
        }

        ApplyThemeToTree(page);
        _contentHost.ResumeLayout();
    }

    private void BuildProfile(Panel page)
    {
        AddPageTitle(page, "Profile", "Manage your identity, avatar, and account shortcuts.");
        var card = CreateSectionCard(new Rectangle(0, 96, 720, 240));

        var avatar = CreateAvatar(34, 38, 96);
        var name = new Label
        {
            Text = _user.FullName,
            Location = new Point(158, 38),
            Size = new Size(420, 34),
            Font = new Font("Segoe UI", 19f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        };
        var email = new Label
        {
            Text = _user.Email,
            Location = new Point(160, 76),
            Size = new Size(420, 24),
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = AppTheme.TextMuted
        };
        var role = new BadgeLabel
        {
            Text = _user.IsAdmin ? "Admin" : "User",
            Location = new Point(160, 112),
            Width = 92,
            BackColor = _user.IsAdmin ? Color.FromArgb(255, 236, 235) : AppTheme.PrimarySoft,
            ForeColor = _user.IsAdmin ? AppTheme.Danger : AppTheme.Primary
        };

        var edit = CreateSecondaryButton("Edit Profile", new Point(34, 168), 132);
        edit.Click += (_, _) => Helpers.ShowInfo("Profile editing is ready for a future account details screen.");
        var password = CreateSecondaryButton("Change Password", new Point(180, 168), 164);
        password.Click += (_, _) => Helpers.ShowInfo("Password changes are handled from the Security section.");
        var upload = CreatePrimaryButton("Upload Image", new Point(360, 168), 148);
        upload.Click += (_, _) => UploadProfileImage();

        card.Controls.AddRange([avatar, name, email, role, edit, password, upload]);
        page.Controls.Add(card);
    }

    private void BuildAppearance(Panel page)
    {
        AddPageTitle(page, "Appearance", "Tune the visual language of your workspace.");
        var card = CreateSectionCard(new Rectangle(0, 96, 760, 322));
        card.Controls.AddRange([
            CreateToggleRow("Dark Mode", "Switch between light and dark presentation.", _settings.DarkMode, value => _settings.DarkMode = value, 28, true),
            CreateToggleRow("Transparency", "Enable frosted glass surfaces.", _settings.TransparencyEnabled, value => _settings.TransparencyEnabled = value, 92, true),
            CreateToggleRow("Blur Effects", "Use subtle blur-inspired soft layers.", _settings.BlurEnabled, value => _settings.BlurEnabled = value, 156, false)
        ]);

        var accentTitle = new Label
        {
            Text = "Accent color",
            Location = new Point(28, 220),
            Size = new Size(160, 24),
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
            ForeColor = TextColor
        };
        card.Controls.Add(accentTitle);

        var colors = new[] { "#0A84FF", "#30D158", "#FF9F0A", "#BF5AF2" };
        var left = 28;
        foreach (var color in colors)
        {
            var swatch = new Button
            {
                Location = new Point(left, 254),
                Size = new Size(36, 36),
                BackColor = ColorTranslator.FromHtml(color),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            swatch.FlatAppearance.BorderSize = _settings.AccentColor == color ? 3 : 0;
            swatch.FlatAppearance.BorderColor = Color.White;
            swatch.Click += (_, _) =>
            {
                _settings.AccentColor = color;
                SaveSettings();
                ShowSection("Appearance");
            };
            swatch.Resize += (_, _) => AppTheme.ApplyRoundedRegion(swatch, 18);
            swatch.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(swatch, 18);
            card.Controls.Add(swatch);
            left += 48;
        }

        var preview = CreateSectionCard(new Rectangle(790, 96, 250, 180));
        preview.Controls.AddRange([
            new Label
            {
                Text = "Live preview",
                Location = new Point(24, 24),
                Size = new Size(180, 28),
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = TextColor
            },
            new BadgeLabel
            {
                Text = "Accent",
                Location = new Point(24, 70),
                Width = 96,
                BackColor = AppTheme.PrimarySoft,
                ForeColor = AccentColor
            }
        ]);
        page.Controls.AddRange([card, preview]);
    }

    private void BuildNotifications(Panel page)
    {
        AddPageTitle(page, "Notifications", "Choose how the app keeps you in the flow.");
        var card = CreateSectionCard(new Rectangle(0, 96, 760, 320));
        card.Controls.AddRange([
            CreateToggleRow("Task completed alerts", "Notify when a task is marked completed.", _settings.TaskCompletedNotifications, value => _settings.TaskCompletedNotifications = value, 28),
            CreateToggleRow("Task reminders", "Show reminders for upcoming due dates.", _settings.TaskReminders, value => _settings.TaskReminders = value, 92),
            CreateToggleRow("Notification sounds", "Play a soft sound for important events.", _settings.NotificationSounds, value => _settings.NotificationSounds = value, 156),
            CreateToggleRow("Desktop notifications", "Allow native desktop notifications.", _settings.DesktopNotifications, value => _settings.DesktopNotifications = value, 220)
        ]);
        page.Controls.Add(card);
    }

    private void BuildSecurity(Panel page)
    {
        AddPageTitle(page, "Security", "Protect your account and review sign-in activity.");
        var card = CreateSectionCard(new Rectangle(0, 96, 760, 300));
        card.Controls.AddRange([
            CreateActionRow("Change password", "Update your current password.", "Change", 28, () => Helpers.ShowInfo("Password change flow is simulated for this project.")),
            CreateToggleRow("Two-factor authentication", "Simulated extra verification for sign-in.", _settings.TwoFactorEnabled, value => _settings.TwoFactorEnabled = value, 102),
            CreateActionRow("Logout all sessions", "End active sessions across devices.", "Logout", 176, () => Helpers.ShowInfo("All sessions were marked for logout.")),
            new Label
            {
                Text = $"Last login: {_settings.LastLoginAt:g}",
                Location = new Point(28, 248),
                Size = new Size(420, 24),
                Font = new Font("Segoe UI", 10f),
                ForeColor = AppTheme.TextMuted
            }
        ]);
        page.Controls.Add(card);
    }

    private void BuildDataStorage(Panel page)
    {
        AddPageTitle(page, "Data & Storage", "Review local usage and export your productivity data.");
        var card = CreateSectionCard(new Rectangle(0, 96, 760, 280));
        card.Controls.AddRange([
            CreateMetricCard("Total tasks", "Stored in your workspace", "Live", new Point(28, 30), AppTheme.Primary),
            CreateMetricCard("Completed", "Finished task records", "Sync", new Point(270, 30), AppTheme.Success),
            CreateMetricCard("Active", "Open task records", "Open", new Point(512, 30), AppTheme.Warning)
        ]);
        var export = CreatePrimaryButton("Export Tasks", new Point(28, 200), 136);
        export.Click += (_, _) => ExportSettingsSnapshot("tasks-export");
        var backup = CreateSecondaryButton("Backup Data", new Point(178, 200), 132);
        backup.Click += (_, _) => ExportSettingsSnapshot("backup");
        card.Controls.Add(export);
        card.Controls.Add(backup);
        page.Controls.Add(card);
    }

    private void BuildAdminSettings(Panel page)
    {
        AddPageTitle(page, "Admin Settings", "Global controls and operational insights for administrators.");
        var card = CreateSectionCard(new Rectangle(0, 96, 820, 330));
        card.Controls.AddRange([
            CreateMetricCard("Users", "Manage active accounts", "Admin", new Point(28, 30), AppTheme.Primary),
            CreateMetricCard("Global tasks", "Application-wide task activity", "Tasks", new Point(286, 30), AppTheme.Success),
            CreateMetricCard("Activity", "Recent user actions", "Live", new Point(544, 30), AppTheme.Warning),
            CreateActionRow("User management", "Open user control tools from the admin dashboard.", "Manage", 176, () => Helpers.ShowInfo("User management is available on the admin dashboard.")),
            CreateActionRow("Reset user passwords", "Simulated reset flow for selected users.", "Reset", 246, () => Helpers.ShowInfo("Password reset flow is simulated for this project."))
        ]);
        page.Controls.Add(card);
    }

    private void BuildAbout(Panel page)
    {
        AddPageTitle(page, "About", "Project details and application information.");
        var card = CreateSectionCard(new Rectangle(0, 96, 700, 280));
        var logo = new Label
        {
            Text = "T",
            Location = new Point(28, 34),
            Size = new Size(76, 76),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = AppTheme.PrimarySoft,
            ForeColor = AppTheme.Primary,
            Font = new Font("Segoe UI", 28f, FontStyle.Bold)
        };
        logo.Resize += (_, _) => AppTheme.ApplyRoundedRegion(logo, 38);
        logo.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(logo, 38);

        card.Controls.AddRange([
            logo,
            new Label { Text = "ToDo List App", Location = new Point(126, 38), Size = new Size(260, 34), Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary },
            new Label { Text = "Version 1.0.0", Location = new Point(128, 78), Size = new Size(240, 24), Font = new Font("Segoe UI", 10f), ForeColor = AppTheme.TextMuted },
            new Label { Text = "Author: PTPP-241", Location = new Point(30, 138), Size = new Size(420, 24), Font = new Font("Segoe UI", 10.5f), ForeColor = AppTheme.TextPrimary },
            new Label { Text = "A modern desktop productivity project built with native C# Windows Forms.", Location = new Point(30, 170), Size = new Size(570, 44), Font = new Font("Segoe UI", 10f), ForeColor = AppTheme.TextMuted },
        ]);
        var github = CreateSecondaryButton("GitHub", new Point(30, 224), 110);
        github.Click += (_, _) => Helpers.ShowInfo("GitHub link is not configured yet.");
        var docs = CreateSecondaryButton("Documentation", new Point(154, 224), 150);
        docs.Click += (_, _) => Helpers.ShowInfo("Documentation link is not configured yet.");
        card.Controls.AddRange([github, docs]);
        page.Controls.Add(card);
    }

    private void AddPageTitle(Panel page, string title, string subtitle)
    {
        page.Controls.Add(new Label
        {
            Text = title,
            Location = new Point(0, 0),
            Size = new Size(620, 44),
            Font = new Font("Segoe UI", 26f, FontStyle.Bold),
            ForeColor = TextColor
        });
        page.Controls.Add(new Label
        {
            Text = subtitle,
            Location = new Point(2, 52),
            Size = new Size(720, 26),
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = MutedColor
        });
    }

    private RoundedPanel CreateSectionCard(Rectangle bounds)
    {
        return new RoundedPanel
        {
            Location = bounds.Location,
            Size = bounds.Size,
            Radius = 26,
            BackColor = CardColor,
            Padding = new Padding(24)
        };
    }

    private Control CreateToggleRow(string title, string description, bool value, Action<bool> setter, int top, bool refreshSection = false)
    {
        var row = new Panel { Location = new Point(28, top), Size = new Size(690, 60), BackColor = Color.Transparent };
        row.Controls.Add(new Label { Text = title, Location = new Point(0, 0), Size = new Size(360, 24), Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold), ForeColor = TextColor });
        row.Controls.Add(new Label { Text = description, Location = new Point(0, 28), Size = new Size(470, 24), Font = new Font("Segoe UI", 9.5f), ForeColor = MutedColor });
        var toggle = new ToggleSwitch { Checked = value, Location = new Point(620, 12) };
        toggle.CheckedChanged += (_, _) =>
        {
            setter(toggle.Checked);
            SaveSettings();
            if (refreshSection)
            {
                ApplyStaticTheme();
                ShowSection(_activeSection);
            }
        };
        row.Controls.Add(toggle);
        return row;
    }

    private Control CreateActionRow(string title, string description, string buttonText, int top, Action? action = null)
    {
        var row = new Panel { Location = new Point(28, top), Size = new Size(690, 62), BackColor = Color.Transparent };
        row.Controls.Add(new Label { Text = title, Location = new Point(0, 0), Size = new Size(360, 24), Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold), ForeColor = TextColor });
        row.Controls.Add(new Label { Text = description, Location = new Point(0, 28), Size = new Size(450, 24), Font = new Font("Segoe UI", 9.5f), ForeColor = MutedColor });
        var button = CreateSecondaryButton(buttonText, new Point(574, 10), 104);
        if (action is not null)
        {
            button.Click += (_, _) => action();
        }

        row.Controls.Add(button);
        return row;
    }

    private RoundedPanel CreateMetricCard(string title, string subtitle, string badge, Point location, Color accent)
    {
        var card = new RoundedPanel { Location = location, Size = new Size(214, 128), Radius = 22, BackColor = CardColor };
        card.Controls.Add(new BadgeLabel { Text = badge, Location = new Point(18, 18), Width = 76, BackColor = AppTheme.PrimarySoft, ForeColor = accent });
        card.Controls.Add(new Label { Text = title, Location = new Point(20, 58), Size = new Size(170, 28), Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold), ForeColor = AppTheme.TextPrimary });
        card.Controls.Add(new Label { Text = subtitle, Location = new Point(20, 88), Size = new Size(170, 24), Font = new Font("Segoe UI", 9.2f), ForeColor = AppTheme.TextMuted });
        return card;
    }

    private Label CreateAvatar(int left, int top, int size)
    {
        var text = string.IsNullOrWhiteSpace(_user.FullName) ? "U" : _user.FullName.Trim()[0].ToString().ToUpperInvariant();
        var avatar = new Label
        {
            Text = text,
            Location = new Point(left, top),
            Size = new Size(size, size),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = AppTheme.PrimarySoft,
            ForeColor = AppTheme.Primary,
            Font = new Font("Segoe UI", 30f, FontStyle.Bold)
        };
        avatar.Resize += (_, _) => AppTheme.ApplyRoundedRegion(avatar, size / 2);
        avatar.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(avatar, size / 2);

        if (!string.IsNullOrWhiteSpace(_settings.ProfileImagePath) && File.Exists(_settings.ProfileImagePath))
        {
            avatar.Image = Image.FromFile(_settings.ProfileImagePath);
            avatar.Text = string.Empty;
            avatar.ImageAlign = ContentAlignment.MiddleCenter;
        }

        return avatar;
    }

    private Button CreatePrimaryButton(string text, Point location, int width)
    {
        var button = new GradientButton { Text = text, Location = location, Size = new Size(width, 44) };
        return button;
    }

    private Button CreateSecondaryButton(string text, Point location, int width)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(width, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.Input,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => button.BackColor = AppTheme.PrimarySoft;
        button.MouseLeave += (_, _) => button.BackColor = AppTheme.Input;
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 14);
        button.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(button, 14);
        return button;
    }

    private void UploadProfileImage()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Choose profile image",
            Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _settings.ProfileImagePath = dialog.FileName;
        SaveSettings();
        ShowSection("Profile");
    }

    private void SaveSettings()
    {
        _settingsService.Save(_settings);
        AppTheme.ApplyUserSettings(_settings);
        AppTheme.ApplyThemeToOpenForms();
    }

    public void ApplyTheme()
    {
        ApplyStaticTheme();
        ShowSection(_activeSection);
    }

    private void ExportSettingsSnapshot(string prefix)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ToDoListApp",
            "Exports");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"{prefix}-{_user.Id}-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        var payload = $$"""
        {
          "userId": {{_user.Id}},
          "name": "{{_user.FullName}}",
          "email": "{{_user.Email}}",
          "role": "{{_user.Role}}",
          "darkMode": {{_settings.DarkMode.ToString().ToLowerInvariant()}},
          "accentColor": "{{_settings.AccentColor}}",
          "createdAt": "{{DateTime.Now:O}}"
        }
        """;
        File.WriteAllText(path, payload);
        Helpers.ShowInfo($"Saved to {path}");
    }

    private Color AccentColor => ColorTranslator.FromHtml(_settings.AccentColor);
    private Color BackgroundColor => _settings.DarkMode ? Color.FromArgb(24, 27, 32) : AppTheme.Background;
    private Color ShellColor => _settings.DarkMode ? Color.FromArgb(31, 35, 42) : Color.FromArgb(248, 250, 254);
    private Color SidebarColor => _settings.DarkMode ? Color.FromArgb(37, 42, 50) : Color.FromArgb(242, 245, 250);
    private Color CardColor => _settings.DarkMode ? Color.FromArgb(42, 47, 56) : Color.FromArgb(_settings.TransparencyEnabled ? 246 : 255, 255, 255, 255);
    private Color InputColor => _settings.DarkMode ? Color.FromArgb(50, 56, 66) : AppTheme.Input;
    private Color TextColor => _settings.DarkMode ? Color.FromArgb(244, 246, 250) : AppTheme.TextPrimary;
    private Color MutedColor => _settings.DarkMode ? Color.FromArgb(174, 181, 192) : AppTheme.TextMuted;
    private Color ActiveNavColor => _settings.DarkMode ? Color.FromArgb(44, 61, 82) : AppTheme.PrimarySoft;

    private void ApplyStaticTheme()
    {
        BackColor = BackgroundColor;
        if (_shell is not null)
        {
            _shell.BackColor = ShellColor;
            _shell.Invalidate();
        }

        if (_sidebar is not null)
        {
            _sidebar.BackColor = SidebarColor;
        }

        if (_sidebarTitle is not null)
        {
            _sidebarTitle.ForeColor = TextColor;
        }

        if (_sidebarSubtitle is not null)
        {
            _sidebarSubtitle.ForeColor = MutedColor;
        }

        _contentHost.BackColor = ShellColor;
        Invalidate();
    }

    private void ApplyThemeToTree(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case BadgeLabel:
                case ToggleSwitch:
                case GradientButton:
                    break;
                case RoundedPanel panel:
                    panel.BackColor = CardColor;
                    panel.Invalidate();
                    break;
                case Label label:
                    if (label.BackColor != Color.Transparent && label.BackColor != Color.Empty)
                    {
                        break;
                    }

                    label.ForeColor = label.Font.Bold || label.Font.Size >= 11f ? TextColor : MutedColor;
                    break;
                case Button button when button.Width == 36 && button.Height == 36:
                    break;
                case Button button:
                    button.BackColor = InputColor;
                    button.ForeColor = TextColor;
                    break;
                case Panel panel when panel.BackColor != Color.Transparent:
                    panel.BackColor = ShellColor;
                    break;
            }

            if (control.HasChildren)
            {
                ApplyThemeToTree(control);
            }
        }
    }

    private void PaintGlassBackground(Graphics graphics, Rectangle bounds)
    {
        var start = _settings.DarkMode ? Color.FromArgb(24, 27, 32) : Color.FromArgb(243, 247, 252);
        var end = _settings.DarkMode ? Color.FromArgb(31, 35, 42) : Color.FromArgb(248, 250, 254);
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, start, end, 45f);
        graphics.FillRectangle(brush, bounds);
        using var blue = new SolidBrush(Color.FromArgb(_settings.DarkMode ? 18 : 28, AccentColor));
        using var purple = new SolidBrush(Color.FromArgb(_settings.DarkMode ? 14 : 24, 191, 90, 242));
        graphics.FillEllipse(blue, bounds.Width - 300, 40, 220, 220);
        graphics.FillEllipse(purple, 240, bounds.Height - 180, 260, 170);
    }
}
