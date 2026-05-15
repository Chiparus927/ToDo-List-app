using ToDoListApp.Models;
using ToDoListApp.Utils;

namespace ToDoListApp.Controls;

public class TaskCardControl : RoundedPanel
{
    private readonly TaskModel _task;
    private readonly Label _title = new();
    private readonly Button _check = new();

    public event EventHandler<TaskModel>? SelectedTask;
    public event EventHandler<TaskModel>? ToggleCompleted;
    public int TaskId => _task.Id;

    public TaskCardControl(TaskModel task)
    {
        _task = task;
        Height = 128;
        Width = 780;
        Margin = new Padding(0, 0, 0, 14);
        Radius = 24;
        BackColor = task.IsCompleted ? AppTheme.SoftSurface : AppTheme.Surface;
        BorderColor = AppTheme.Border;
        Cursor = Cursors.Hand;
        Build();
        ApplyTheme();
    }

    public void SetSelected(bool selected)
    {
        BorderColor = selected ? AppTheme.Primary : AppTheme.Border;
        Invalidate();
    }

    private void Build()
    {
        Controls.Clear();

        _check.Text = _task.IsCompleted ? "Done" : "Done";
        _check.Location = new Point(18, 42);
        _check.Size = new Size(56, 36);
        _check.FlatStyle = FlatStyle.Flat;
        _check.FlatAppearance.BorderSize = 0;
        _check.BackColor = CompletedBackColor();
        _check.ForeColor = _task.IsCompleted ? AppTheme.Success : AppTheme.Primary;
        _check.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
        _check.Cursor = Cursors.Hand;
        _check.Click += (_, _) => ToggleCompleted?.Invoke(this, _task);
        _check.Resize += (_, _) => AppTheme.ApplyRoundedRegion(_check, 14);
        _check.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(_check, 14);

        _title.Text = _task.Title;
        _title.Location = new Point(92, 20);
        _title.Size = new Size(420, 30);
        _title.ForeColor = _task.IsCompleted ? AppTheme.TextMuted : AppTheme.TextPrimary;
        _title.Font = new Font("Segoe UI Semibold", 13f, _task.IsCompleted ? FontStyle.Strikeout | FontStyle.Bold : FontStyle.Bold);

        var description = new Label
        {
            Text = string.IsNullOrWhiteSpace(_task.Description) ? "No description" : _task.Description,
            Location = new Point(92, 54),
            Size = new Size(520, 24),
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 10f, _task.IsCompleted ? FontStyle.Strikeout : FontStyle.Regular)
        };

        var category = new BadgeLabel
        {
            Text = _task.CategoryName,
            Location = new Point(92, 88),
            Width = Math.Max(92, Math.Min(160, _task.CategoryName.Length * 9 + 34)),
            BackColor = CategoryBackColor(_task.CategoryName),
            ForeColor = CategoryForeColor(_task.CategoryName)
        };

        var due = new BadgeLabel
        {
            Text = _task.DueDate.ToString("MMM dd, yyyy"),
            Location = new Point(category.Right + 10, 88),
            Width = 120,
            BackColor = AppTheme.Input,
            ForeColor = AppTheme.TextMuted
        };

        var status = new BadgeLabel
        {
            Text = _task.IsCompleted ? "Completed" : "Active",
            Width = 104,
            Location = new Point(Width - 138, 20),
            BackColor = CompletedBackColor(),
            ForeColor = _task.IsCompleted ? AppTheme.Success : AppTheme.Primary,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        Controls.AddRange([_check, _title, description, category, due, status]);
        Click += SelectCard;
        foreach (Control child in Controls)
        {
            if (child != _check)
            {
                child.Click += SelectCard;
            }
        }
    }

    private void SelectCard(object? sender, EventArgs e)
    {
        SelectedTask?.Invoke(this, _task);
    }

    public void ApplyTheme()
    {
        BackColor = _task.IsCompleted ? AppTheme.SoftSurface : AppTheme.Surface;
        BorderColor = AppTheme.Border;
        _check.BackColor = CompletedBackColor();
        _check.ForeColor = _task.IsCompleted ? AppTheme.Success : AppTheme.Primary;
        _title.ForeColor = _task.IsCompleted ? AppTheme.TextMuted : AppTheme.TextPrimary;

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
                case Label label when label != _title:
                    label.ForeColor = AppTheme.TextMuted;
                    break;
            }
        }

        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        BackColor = AppTheme.SoftSurface;
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        BackColor = _task.IsCompleted ? AppTheme.SoftSurface : AppTheme.Surface;
        base.OnMouseLeave(e);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        if (Controls.Count > 0)
        {
            _title.Width = Math.Max(220, Width - 280);
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
