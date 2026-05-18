using ToDoListApp.Models;
using ToDoListApp.Utils;

namespace ToDoListApp.Controls;

public class UserCardControl : RoundedPanel
{
    private readonly UserModel _user;

    public event EventHandler<UserModel>? SelectedUser;
    public event EventHandler<UserModel>? ViewTasks;
    public event EventHandler<UserModel>? MakeAdmin;
    public event EventHandler<UserModel>? MakeUser;
    public event EventHandler<UserModel>? DeleteUser;

    public UserCardControl(UserModel user)
    {
        _user = user;
        Width = 820;
        Height = 138;
        Margin = new Padding(0, 0, 0, 14);
        Radius = 24;
        BackColor = AppTheme.Surface;
        BorderColor = AppTheme.Border;
        Cursor = Cursors.Hand;
        Build();
        ApplyTheme();
    }

    private void Build()
    {
        var initial = string.IsNullOrWhiteSpace(_user.FullName) ? "U" : _user.FullName.Trim()[0].ToString().ToUpperInvariant();
        var avatar = new Label
        {
            Text = initial,
            Location = new Point(22, 30),
            Size = new Size(58, 58),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = RoleBackColor(),
            ForeColor = _user.IsAdmin ? AppTheme.Danger : AppTheme.Primary,
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold)
        };
        avatar.Resize += (_, _) => AppTheme.ApplyRoundedRegion(avatar, 29);
        avatar.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(avatar, 29);

        var name = new Label
        {
            Text = _user.FullName,
            Location = new Point(100, 22),
            Size = new Size(280, 28),
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 12.5f, FontStyle.Bold)
        };

        var email = new Label
        {
            Text = _user.Email,
            Location = new Point(100, 54),
            Size = new Size(330, 24),
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 10f)
        };

        var created = new Label
        {
            Text = "Created " + _user.CreatedAt.ToString("MMM dd, yyyy"),
            Location = new Point(100, 84),
            Size = new Size(220, 24),
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 9.5f)
        };

        var role = new BadgeLabel
        {
            Text = _user.IsAdmin ? "Admin" : "User",
            Location = new Point(430, 28),
            Width = 96,
            BackColor = RoleBackColor(),
            ForeColor = _user.IsAdmin ? AppTheme.Danger : AppTheme.Primary
        };

        var tasks = CreateActionButton("Tasks", new Point(430, 74), 74);
        tasks.Click += (_, _) => ViewTasks?.Invoke(this, _user);
        var makeAdmin = CreateActionButton("Make admin", new Point(512, 74), 108);
        makeAdmin.Click += (_, _) => MakeAdmin?.Invoke(this, _user);
        var makeUser = CreateActionButton("Make user", new Point(628, 74), 96);
        makeUser.Click += (_, _) => MakeUser?.Invoke(this, _user);
        var delete = CreateActionButton("Delete", new Point(732, 74), 82);
        delete.ForeColor = AppTheme.Danger;
        delete.Click += (_, _) => DeleteUser?.Invoke(this, _user);

        Controls.AddRange([avatar, name, email, created, role, tasks, makeAdmin, makeUser, delete]);
        Click += (_, _) => SelectedUser?.Invoke(this, _user);
        foreach (Control child in Controls)
        {
            if (child is not Button)
            {
                child.Click += (_, _) => SelectedUser?.Invoke(this, _user);
            }
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        BackColor = AppTheme.SoftSurface;
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        BackColor = AppTheme.Surface;
        base.OnMouseLeave(e);
    }

    public void ApplyTheme()
    {
        BackColor = AppTheme.Surface;
        BorderColor = AppTheme.Border;
        foreach (Control control in Controls)
        {
            switch (control)
            {
                case BadgeLabel badge:
                    badge.BackColor = RoleBackColor();
                    badge.ForeColor = _user.IsAdmin ? AppTheme.Danger : AppTheme.Primary;
                    break;
                case Button button:
                    button.BackColor = AppTheme.Input;
                    button.ForeColor = button.Text == "Delete" ? AppTheme.Danger : AppTheme.TextPrimary;
                    break;
                case Label label when label.Text.Length == 1:
                    label.BackColor = RoleBackColor();
                    label.ForeColor = _user.IsAdmin ? AppTheme.Danger : AppTheme.Primary;
                    break;
                case Label label:
                    label.ForeColor = label.Font.Bold || label.Font.Size >= 11f ? AppTheme.TextPrimary : AppTheme.TextMuted;
                    break;
            }
        }

        Invalidate();
    }

    private static Button CreateActionButton(string text, Point location, int width)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(width, 34),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = AppTheme.Input,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => button.BackColor = AppTheme.PrimarySoft;
        button.MouseLeave += (_, _) => button.BackColor = AppTheme.Input;
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 12);
        button.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(button, 12);
        return button;
    }

    private Color RoleBackColor()
    {
        if (_user.IsAdmin)
        {
            return AppTheme.IsDarkMode ? Color.FromArgb(72, 34, 36) : Color.FromArgb(255, 236, 235);
        }

        return AppTheme.PrimarySoft;
    }
}
