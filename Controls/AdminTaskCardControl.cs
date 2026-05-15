using ToDoListApp.Models;
using ToDoListApp.Utils;

namespace ToDoListApp.Controls;

public class AdminTaskCardControl : RoundedPanel
{
    private readonly AdminTaskModel _task;

    public AdminTaskCardControl(AdminTaskModel task)
    {
        _task = task;
        Height = 136;
        Width = 820;
        Margin = new Padding(0, 0, 0, 14);
        Radius = 24;
        BackColor = task.IsCompleted ? AppTheme.SoftSurface : AppTheme.Surface;
        BorderColor = AppTheme.Border;
        Build();
        ApplyTheme();
    }

    private void Build()
    {
        var avatarText = string.IsNullOrWhiteSpace(_task.UserName) ? "U" : _task.UserName.Trim()[0].ToString().ToUpperInvariant();
        var avatar = new Label
        {
            Text = avatarText,
            Location = new Point(22, 34),
            Size = new Size(54, 54),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = AppTheme.PrimarySoft,
            ForeColor = AppTheme.Primary,
            Font = new Font("Segoe UI Semibold", 17f, FontStyle.Bold)
        };
        avatar.Resize += (_, _) => AppTheme.ApplyRoundedRegion(avatar, 27);
        avatar.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(avatar, 27);

        var title = new Label
        {
            Text = _task.Title,
            Location = new Point(96, 18),
            Size = new Size(390, 28),
            ForeColor = _task.IsCompleted ? AppTheme.TextMuted : AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 12.5f, _task.IsCompleted ? FontStyle.Strikeout | FontStyle.Bold : FontStyle.Bold)
        };

        var description = new Label
        {
            Text = string.IsNullOrWhiteSpace(_task.Description) ? "No description" : _task.Description,
            Location = new Point(96, 48),
            Size = new Size(480, 24),
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 9.8f)
        };

        var owner = new Label
        {
            Text = $"{_task.UserName}  •  {_task.UserEmail}",
            Location = new Point(96, 76),
            Size = new Size(520, 22),
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 9.5f)
        };

        var category = new BadgeLabel
        {
            Text = _task.CategoryName,
            Location = new Point(96, 102),
            Width = Math.Max(92, Math.Min(160, _task.CategoryName.Length * 9 + 34)),
            BackColor = CategoryBackColor(_task.CategoryName),
            ForeColor = CategoryForeColor(_task.CategoryName)
        };

        var due = new BadgeLabel
        {
            Text = _task.DueDate.ToString("MMM dd, yyyy"),
            Location = new Point(category.Right + 10, 102),
            Width = 120,
            BackColor = AppTheme.Input,
            ForeColor = AppTheme.TextMuted
        };

        var status = new BadgeLabel
        {
            Text = _task.IsCompleted ? "Completed" : "Active",
            Width = 104,
            Location = new Point(Width - 138, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = _task.IsCompleted ? CompletedBackColor() : AppTheme.PrimarySoft,
            ForeColor = _task.IsCompleted ? AppTheme.Success : AppTheme.Primary
        };

        Controls.AddRange([avatar, title, description, owner, category, due, status]);
    }

    public void ApplyTheme()
    {
        BackColor = _task.IsCompleted ? AppTheme.SoftSurface : AppTheme.Surface;
        BorderColor = AppTheme.Border;
        foreach (Control control in Controls)
        {
            switch (control)
            {
                case BadgeLabel badge when badge.Text == _task.CategoryName:
                    badge.BackColor = CategoryBackColor(_task.CategoryName);
                    badge.ForeColor = CategoryForeColor(_task.CategoryName);
                    break;
                case BadgeLabel badge when badge.Text == "Completed" || badge.Text == "Active":
                    badge.BackColor = _task.IsCompleted ? CompletedBackColor() : AppTheme.PrimarySoft;
                    badge.ForeColor = _task.IsCompleted ? AppTheme.Success : AppTheme.Primary;
                    break;
                case BadgeLabel badge:
                    badge.BackColor = AppTheme.Input;
                    badge.ForeColor = AppTheme.TextMuted;
                    break;
                case Label label when label.Text.Length == 1:
                    label.BackColor = AppTheme.PrimarySoft;
                    label.ForeColor = AppTheme.Primary;
                    break;
                case Label label:
                    label.ForeColor = label.Font.Strikeout || label.Font.Bold || label.Font.Size >= 11f ? AppTheme.TextPrimary : AppTheme.TextMuted;
                    break;
            }
        }

        Invalidate();
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        foreach (var label in Controls.OfType<Label>())
        {
            if (label.Text == _task.Title)
            {
                label.Width = Math.Max(240, Width - 310);
            }
        }
    }

    private static Color CategoryBackColor(string category)
    {
        var key = category.ToLowerInvariant();
        if (AppTheme.IsDarkMode)
        {
            if (key.Contains("work")) return Color.FromArgb(25, 48, 76);
            if (key.Contains("personal")) return Color.FromArgb(24, 58, 38);
            if (key.Contains("school")) return Color.FromArgb(74, 51, 18);
            return Color.FromArgb(50, 40, 72);
        }

        if (key.Contains("work")) return Color.FromArgb(232, 240, 255);
        if (key.Contains("personal")) return Color.FromArgb(238, 250, 243);
        if (key.Contains("school")) return Color.FromArgb(255, 248, 232);
        return Color.FromArgb(244, 240, 255);
    }

    private static Color CategoryForeColor(string category)
    {
        var key = category.ToLowerInvariant();
        if (key.Contains("work")) return AppTheme.Primary;
        if (key.Contains("personal")) return AppTheme.Success;
        if (key.Contains("school")) return AppTheme.Warning;
        return AppTheme.IsDarkMode ? Color.FromArgb(191, 167, 255) : Color.FromArgb(116, 86, 190);
    }

    private static Color CompletedBackColor()
    {
        return AppTheme.IsDarkMode ? Color.FromArgb(25, 58, 38) : Color.FromArgb(232, 250, 238);
    }
}
