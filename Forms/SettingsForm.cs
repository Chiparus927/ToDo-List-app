using System.Diagnostics;
using ToDoListApp.Controls;
using ToDoListApp.Database;
using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class SettingsForm : Form, IThemeAware
{
    private readonly UserModel _user;
    private readonly UserRepository _userRepository = new();
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
            Size = new Size(200, 50),
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            ForeColor = TextColor
        };
        _sidebarSubtitle = new Label
        {
            Text = _user.IsAdmin ? "Admin workspace" : "Personal workspace",
            Location = new Point(30, 82),
            Size = new Size(190, 22),
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = MutedColor
        };
        _sidebar.Controls.AddRange([_sidebarTitle, _sidebarSubtitle]);

        var sections = new List<string> { "Profile", "Appearance", "Notifications", "Security" };
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
            UseVisualStyleBackColor = false,
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
        var card = CreateSectionCard(new Rectangle(0, 112, 720, 264));

        var avatar = CreateAvatar(34, 36, 104);
        var name = new Label
        {
            Text = _user.FullName,
            Location = new Point(194, 38),
            Size = new Size(500, 42),
            Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        };
        var email = new Label
        {
            Text = _user.Email,
            Location = new Point(196, 86),
            Size = new Size(500, 24),
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = AppTheme.TextMuted
        };
        var role = new BadgeLabel
        {
            Text = _user.IsAdmin ? "Admin" : "User",
            Location = new Point(196, 126),
            Width = 92,
            BackColor = _user.IsAdmin ? Color.FromArgb(255, 236, 235) : AppTheme.PrimarySoft,
            ForeColor = _user.IsAdmin ? AppTheme.Danger : AppTheme.Primary
        };

        var edit = CreateSecondaryButton("Edit Profile", new Point(34, 194), 132);
        edit.Click += (_, _) => EditProfile();
        var password = CreateSecondaryButton("Change Password", new Point(180, 194), 164);
        password.Click += (_, _) => ChangePassword();
        var upload = CreatePrimaryButton("Upload Image", new Point(360, 194), 148);
        upload.Click += (_, _) => UploadProfileImage();

        card.Controls.AddRange([avatar, name, email, role, edit, password, upload]);
        page.Controls.Add(card);
    }

    private void BuildAppearance(Panel page)
    {
        AddPageTitle(page, "Appearance", "Tune the visual language of your workspace.");
        var card = CreateSectionCard(new Rectangle(0, 96, 760, 224));
        card.Controls.AddRange([
            CreateToggleRow("Dark Mode", "Switch between light and dark presentation.", _settings.DarkMode, value => _settings.DarkMode = value, 28, true)
        ]);

        var accentTitle = new Label
        {
            Text = "Accent color",
            Location = new Point(28, 96),
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
                Location = new Point(left, 130),
                Size = new Size(36, 36),
                BackColor = ColorTranslator.FromHtml(color),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
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

        page.Controls.Add(card);
    }

    private void BuildNotifications(Panel page)
    {
        AddPageTitle(page, "Notifications", "Choose how the app keeps you in the flow.");
        var card = CreateSectionCard(new Rectangle(0, 96, 760, 390));
        card.Controls.AddRange([
            CreateToggleRow("Task completed alerts", "Notify when a task is marked completed.", _settings.TaskCompletedNotifications, value => _settings.TaskCompletedNotifications = value, 28),
            CreateToggleRow("Task reminders", "Show reminders for upcoming due dates.", _settings.TaskReminders, value => _settings.TaskReminders = value, 92),
            CreateToggleRow("Notification sounds", "Play a soft sound for important events.", _settings.NotificationSounds, value => _settings.NotificationSounds = value, 156),
            CreateToggleRow("Desktop notifications", "Allow native desktop notifications.", _settings.DesktopNotifications, value => _settings.DesktopNotifications = value, 220),
            CreateActionRow("Test notification", "Preview the current notification settings.", "Send", 300, SendTestNotification)
        ]);
        page.Controls.Add(card);
    }

    private void BuildSecurity(Panel page)
    {
        AddPageTitle(page, "Security", "Protect your account and review sign-in activity.");
        var card = CreateSectionCard(new Rectangle(0, 96, 760, 230));
        card.Controls.AddRange([
            CreateActionRow("Change password", "Update your current password.", "Change", 28, ChangePassword),
            CreateActionRow("Logout all sessions", "End active sessions and return to login.", "Logout", 102, LogoutAllSessions),
            new Label
            {
                Text = $"Last login: {_settings.LastLoginAt:g}",
                Location = new Point(28, 174),
                Size = new Size(420, 24),
                Font = new Font("Segoe UI", 10f),
                ForeColor = AppTheme.TextMuted
            }
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
            BackColor = AppTheme.ButtonNeutral,
            ForeColor = TextColor,
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
        github.Click += (_, _) => OpenGitHub();
        var docs = CreateSecondaryButton("Documentation", new Point(154, 224), 150);
        docs.Click += (_, _) => ShowDocumentation();
        card.Controls.AddRange([github, docs]);
        page.Controls.Add(card);
    }

    private void AddPageTitle(Panel page, string title, string subtitle)
    {
        page.Controls.Add(new Label
        {
            Text = title,
            Location = new Point(0, 0),
            Size = new Size(720, 58),
            Font = new Font("Segoe UI", 26f, FontStyle.Bold),
            ForeColor = TextColor
        });
        page.Controls.Add(new Label
        {
            Text = subtitle,
            Location = new Point(2, 68),
            Size = new Size(760, 28),
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
        var rowBackColor = Color.FromArgb(255, CardColor);
        var row = new Panel { Location = new Point(28, top), Size = new Size(690, 60), BackColor = rowBackColor };
        row.Controls.Add(new Label { Text = title, Location = new Point(0, 0), Size = new Size(360, 24), Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold), ForeColor = TextColor });
        row.Controls.Add(new Label { Text = description, Location = new Point(0, 28), Size = new Size(470, 24), Font = new Font("Segoe UI", 9.5f), ForeColor = MutedColor });
        var toggle = new ToggleSwitch { Checked = value, Location = new Point(620, 12) };
        toggle.BackColor = rowBackColor;
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

    private Control CreateAvatar(int left, int top, int size)
    {
        var text = string.IsNullOrWhiteSpace(_user.FullName) ? "U" : _user.FullName.Trim()[0].ToString().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(_settings.ProfileImagePath) && File.Exists(_settings.ProfileImagePath))
        {
            var picture = new PictureBox
            {
                Location = new Point(left, top),
                Size = new Size(size, size),
                BackColor = AppTheme.PrimarySoft,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadSquareImage(_settings.ProfileImagePath, size)
            };
            picture.Resize += (_, _) => AppTheme.ApplyRoundedRegion(picture, size / 2);
            picture.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(picture, size / 2);
            return picture;
        }

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

        return avatar;
    }

    private static Image LoadSquareImage(string path, int size)
    {
        using var image = Image.FromFile(path);
        var sourceSize = Math.Min(image.Width, image.Height);
        var sourceX = (image.Width - sourceSize) / 2;
        var sourceY = (image.Height - sourceSize) / 2;
        var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.DrawImage(
            image,
            new Rectangle(0, 0, size, size),
            new Rectangle(sourceX, sourceY, sourceSize, sourceSize),
            GraphicsUnit.Pixel);
        return bitmap;
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
            UseVisualStyleBackColor = false,
            BackColor = AppTheme.PrimarySoft,
            ForeColor = AppTheme.Primary,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => button.BackColor = AppTheme.ButtonNeutralHover;
        button.MouseLeave += (_, _) =>
        {
            button.BackColor = AppTheme.ButtonNeutral;
            button.ForeColor = TextColor;
        };
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

    private void EditProfile()
    {
        using var form = CreateAccountDialog("Edit profile", 430, 392);
        var fullNameBox = new TextBox { Text = _user.FullName, PlaceholderText = "Full name" };
        var emailBox = new TextBox { Text = _user.Email, PlaceholderText = "Email" };
        var selectedImagePath = _settings.ProfileImagePath;

        AddDialogTitle(form, "Edit profile", "Update your name and email address.");
        AddDialogInput(form, "Full name", fullNameBox, 82, false);
        AddDialogInput(form, "Email", emailBox, 148, false);

        var imageLabel = new Label
        {
            Text = string.IsNullOrWhiteSpace(selectedImagePath) ? "No profile image selected." : ShortFileName(selectedImagePath),
            Location = new Point(24, 224),
            Size = new Size(230, 40),
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = MutedColor
        };
        var chooseImage = CreateDialogSecondaryButton("Choose image", new Point(270, 224), 128);
        chooseImage.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Choose profile image",
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dialog.ShowDialog(form) != DialogResult.OK)
            {
                return;
            }

            selectedImagePath = dialog.FileName;
            imageLabel.Text = ShortFileName(selectedImagePath);
        };

        form.Controls.AddRange([imageLabel, chooseImage]);

        var save = CreateDialogPrimaryButton("Save", new Point(202, 306), 92);
        var cancel = CreateDialogSecondaryButton("Cancel", new Point(306, 306), 92);
        cancel.Click += (_, _) => form.Close();
        save.Click += (_, _) =>
        {
            var fullName = fullNameBox.Text.Trim();
            var email = emailBox.Text.Trim();

            if (!Validator.IsFullNameValid(fullName))
            {
                Helpers.ShowError("Full name must be at least 3 characters.");
                return;
            }

            if (!Validator.IsEmailValid(email))
            {
                Helpers.ShowError("Please enter a valid email address.");
                return;
            }

            try
            {
                var existing = _userRepository.GetByEmail(email);
                if (existing is not null && existing.Id != _user.Id)
                {
                    Helpers.ShowError("An account with this email already exists.");
                    return;
                }

                _userRepository.UpdateProfile(_user.Id, fullName, email);
                _user.FullName = fullName;
                _user.Email = email;
                _settings.ProfileImagePath = selectedImagePath;
                SaveSettings();
                Helpers.ShowInfo("Profile updated successfully.");
                form.DialogResult = DialogResult.OK;
                form.Close();
            }
            catch (Exception ex)
            {
                Helpers.ShowError($"Could not update profile: {ex.Message}");
            }
        };

        form.Controls.AddRange([save, cancel]);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            ShowSection("Profile");
        }
    }

    private static string ShortFileName(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "No profile image selected.";
        }

        var fileName = Path.GetFileName(path);
        return fileName.Length <= 34 ? fileName : fileName[..31] + "...";
    }

    private void ChangePassword()
    {
        using var form = CreateAccountDialog("Change password", 430, 422);
        var currentBox = new TextBox { PlaceholderText = "Current password" };
        var newBox = new TextBox { PlaceholderText = "New password" };
        var confirmBox = new TextBox { PlaceholderText = "Confirm new password" };

        AddDialogTitle(form, "Change password", "Confirm your current password, then choose a new one.");
        AddDialogInput(form, "Current password", currentBox, 82, true);
        AddDialogInput(form, "New password", newBox, 148, true);
        AddDialogInput(form, "Confirm password", confirmBox, 214, true);

        var hint = new Label
        {
            Text = "Use at least 8 characters, one uppercase letter, and one special character.",
            Location = new Point(24, 282),
            Size = new Size(374, 36),
            Font = new Font("Segoe UI", 9f),
            ForeColor = MutedColor
        };
        form.Controls.Add(hint);

        var save = CreateDialogPrimaryButton("Update", new Point(202, 340), 92);
        var cancel = CreateDialogSecondaryButton("Cancel", new Point(306, 340), 92);
        cancel.Click += (_, _) => form.Close();
        save.Click += (_, _) =>
        {
            if (Validator.IsNullOrWhiteSpace(currentBox.Text, newBox.Text, confirmBox.Text))
            {
                Helpers.ShowError("Please fill in all password fields.");
                return;
            }

            var currentHash = Helpers.HashPassword(currentBox.Text);
            if (!_user.PasswordHash.Equals(currentHash, StringComparison.OrdinalIgnoreCase))
            {
                Helpers.ShowError("Current password is incorrect.");
                return;
            }

            if (!newBox.Text.Equals(confirmBox.Text, StringComparison.Ordinal))
            {
                Helpers.ShowError("New passwords do not match.");
                return;
            }

            if (!Validator.IsPasswordValid(newBox.Text))
            {
                Helpers.ShowError("Password must be at least 8 characters and include one uppercase letter and one special character.");
                return;
            }

            try
            {
                var newHash = Helpers.HashPassword(newBox.Text);
                _userRepository.UpdatePassword(_user.Id, newHash);
                _user.PasswordHash = newHash;
                Helpers.ShowInfo("Password changed successfully.");
                form.DialogResult = DialogResult.OK;
                form.Close();
            }
            catch (Exception ex)
            {
                Helpers.ShowError($"Could not change password: {ex.Message}");
            }
        };

        form.Controls.AddRange([save, cancel]);
        form.ShowDialog(this);
    }

    private Form CreateAccountDialog(string title, int width, int height)
    {
        var dialogBackColor = Color.FromArgb(255, CardColor);
        var form = new Form
        {
            Text = title,
            Size = new Size(width, height),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = dialogBackColor
        };
        return form;
    }

    private void AddDialogTitle(Form form, string title, string subtitle)
    {
        form.Controls.Add(new Label
        {
            Text = title,
            Location = new Point(24, 18),
            Size = new Size(form.ClientSize.Width - 48, 34),
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = TextColor
        });
        form.Controls.Add(new Label
        {
            Text = subtitle,
            Location = new Point(25, 56),
            Size = new Size(form.ClientSize.Width - 50, 24),
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = MutedColor
        });
    }

    private void AddDialogInput(Form form, string label, TextBox textBox, int top, bool password)
    {
        form.Controls.Add(new Label
        {
            Text = label,
            Location = new Point(24, top),
            Size = new Size(150, 22),
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            ForeColor = TextColor
        });

        var row = password
            ? PasswordRevealHelper.CreatePasswordRow(textBox, form.ClientSize.Width - 48)
            : AppTheme.CreateTextInputRow(textBox, form.ClientSize.Width - 48);
        row.Location = new Point(24, top + 24);
        form.Controls.Add(row);
    }

    private Button CreateDialogPrimaryButton(string text, Point location, int width)
    {
        var button = new GradientButton
        {
            Text = text,
            Location = location,
            Size = new Size(width, 40)
        };
        return button;
    }

    private Button CreateDialogSecondaryButton(string text, Point location, int width)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(width, 40),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = AppTheme.ButtonNeutral,
            ForeColor = TextColor,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => button.BackColor = AppTheme.ButtonNeutralHover;
        button.MouseLeave += (_, _) =>
        {
            button.BackColor = AppTheme.ButtonNeutral;
            button.ForeColor = TextColor;
        };
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 14);
        button.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(button, 14);
        return button;
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

    private void LogoutAllSessions()
    {
        var confirm = MessageBox.Show(
            "Logout from this account and return to the login screen?",
            "Confirm logout",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        Application.Restart();
    }

    private void SendTestNotification()
    {
        if (!_settings.TaskCompletedNotifications && !_settings.TaskReminders && !_settings.DesktopNotifications)
        {
            Helpers.ShowInfo("Notifications are currently turned off.");
            return;
        }

        if (_settings.NotificationSounds)
        {
            System.Media.SystemSounds.Asterisk.Play();
        }

        if (_settings.DesktopNotifications)
        {
            using var notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                BalloonTipTitle = "ToDo List App",
                BalloonTipText = "Notifications are enabled and working."
            };
            notifyIcon.ShowBalloonTip(3000);
            return;
        }

        Helpers.ShowInfo("Notifications are enabled and working.", "ToDo List App");
    }

    private static void OpenGitHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Chiparus927/ToDo-List-app",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Helpers.ShowError($"Could not open GitHub: {ex.Message}");
        }
    }

    private void ShowDocumentation()
    {
        using var form = CreateAccountDialog("Documentation", 680, 600);
        AddDialogTitle(form, "Documentation", "ToDo List App user guide and project notes.");

        var text = new TextBox
        {
            Location = new Point(24, 112),
            Size = new Size(616, 376),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(255, CardColor),
            ForeColor = TextColor,
            Font = new Font("Segoe UI", 10f),
            Text = BuildDocumentationText()
        };

        var close = CreateDialogPrimaryButton("Close", new Point(548, 508), 92);
        close.Click += (_, _) => form.Close();
        form.Controls.AddRange([text, close]);
        form.ShowDialog(this);
    }

    private static string BuildDocumentationText()
    {
        return string.Join(Environment.NewLine, [
            "ToDo List App Documentation",
            "",
            "Overview",
            "This application helps users organize tasks in a desktop workspace. Each account has its own tasks, profile, visual preferences, and notification settings.",
            "",
            "Main features",
            "- Register and login with a protected password hash.",
            "- Create, edit, complete, filter, and delete tasks.",
            "- Organize tasks by category and due date.",
            "- Customize dark mode and accent color from Appearance.",
            "- Edit profile details, profile image, and account password from Settings.",
            "",
            "Notifications",
            "The Notifications page saves your preferences and includes a test button for sound and desktop notifications.",
            "",
            "Admin features",
            "Admin users can review all users and all tasks from the admin dashboard.",
            "",
            "Data storage",
            "The application stores users, tasks, and categories in MySQL. Personal settings, theme choices, and profile image path are saved locally in the user's AppData folder.",
            "",
            "Security notes",
            "Passwords are stored as hashes, not plain text. Changing a password requires the current password.",
            "",
            "GitHub",
            "https://github.com/Chiparus927/ToDo-List-app"
        ]);
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
                    button.BackColor = AppTheme.ButtonNeutral;
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
