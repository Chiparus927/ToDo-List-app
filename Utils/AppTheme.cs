using System.Drawing.Drawing2D;
using ToDoListApp.Controls;
using ToDoListApp.Models;

namespace ToDoListApp.Utils;

public static class AppTheme
{
    public static bool IsDarkMode { get; private set; }
    public static Color Background { get; private set; } = Color.FromArgb(246, 247, 251);
    public static Color Sidebar { get; private set; } = Color.FromArgb(242, 244, 248);
    public static Color Surface { get; private set; } = Color.White;
    public static Color SoftSurface { get; private set; } = Color.FromArgb(250, 251, 253);
    public static Color Input { get; private set; } = Color.FromArgb(244, 246, 250);
    public static Color Primary { get; private set; } = Color.FromArgb(10, 132, 255);
    public static Color PrimaryHover { get; private set; } = Color.FromArgb(0, 111, 230);
    public static Color PrimarySoft { get; private set; } = Color.FromArgb(225, 240, 255);
    public static Color TextPrimary { get; private set; } = Color.FromArgb(28, 28, 30);
    public static Color TextMuted { get; private set; } = Color.FromArgb(99, 99, 102);
    public static Color Border { get; private set; } = Color.FromArgb(220, 224, 230);
    public static readonly Color Success = Color.FromArgb(52, 199, 89);
    public static readonly Color Warning = Color.FromArgb(255, 149, 0);
    public static readonly Color Danger = Color.FromArgb(255, 59, 48);
    public static readonly Font TitleFont = new("Segoe UI", 20, FontStyle.Bold);
    public static readonly Font LabelFont = new("Segoe UI", 9, FontStyle.Regular);

    public static void StyleForm(Form form, Size? minimumSize = null)
    {
        form.Font = new Font("Segoe UI", 10f);
        form.BackColor = Background;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimumSize = minimumSize ?? new Size(860, 560);
        form.DoubleBuffered(true);
    }

    public static void ApplyUserSettings(UserSettingsModel settings)
    {
        IsDarkMode = settings.DarkMode;
        Primary = ColorTranslator.FromHtml(settings.AccentColor);
        PrimaryHover = ControlPaint.Dark(Primary, 0.18f);
        PrimarySoft = settings.DarkMode ? Blend(Primary, Color.FromArgb(30, 30, 30), 0.72f) : Blend(Primary, Color.White, 0.86f);

        if (settings.DarkMode)
        {
            Background = Color.FromArgb(18, 18, 18);
            Sidebar = Color.FromArgb(30, 30, 30);
            Surface = Color.FromArgb(30, 30, 30);
            SoftSurface = Color.FromArgb(42, 42, 42);
            Input = Color.FromArgb(42, 45, 50);
            TextPrimary = Color.FromArgb(245, 245, 247);
            TextMuted = Color.FromArgb(178, 178, 184);
            Border = Color.FromArgb(68, 68, 72);
            return;
        }

        Background = Color.FromArgb(246, 247, 251);
        Sidebar = Color.FromArgb(242, 244, 248);
        Surface = Color.White;
        SoftSurface = Color.FromArgb(250, 251, 253);
        Input = Color.FromArgb(244, 246, 250);
        TextPrimary = Color.FromArgb(28, 28, 30);
        TextMuted = Color.FromArgb(99, 99, 102);
        Border = Color.FromArgb(220, 224, 230);
    }

    public static void ApplyThemeToOpenForms()
    {
        foreach (Form form in Application.OpenForms)
        {
            ApplyThemeToControl(form, true);
            if (form is IThemeAware themeAware)
            {
                themeAware.ApplyTheme();
            }
            form.Invalidate(true);
        }
    }

    public static void ApplyThemeToControl(Control control, bool root = false)
    {
        switch (control)
        {
            case Form form:
                form.BackColor = Background;
                break;
            case TaskCardControl taskCard:
                taskCard.ApplyTheme();
                break;
            case UserCardControl userCard:
                userCard.ApplyTheme();
                break;
            case AdminTaskCardControl adminTaskCard:
                adminTaskCard.ApplyTheme();
                break;
            case RoundedPanel panel:
                panel.BackColor = Surface;
                panel.BorderColor = Border;
                break;
            case FlowLayoutPanel flow:
                flow.BackColor = Surface;
                break;
            case Panel panel:
                if (panel.Dock == DockStyle.Left)
                {
                    panel.BackColor = Sidebar;
                }
                else if (panel.Controls.OfType<TextBox>().Any())
                {
                    panel.BackColor = Input;
                }
                else if (panel is FlowLayoutPanel)
                {
                    panel.BackColor = Surface;
                }
                else if (IsThemeManagedColor(panel.BackColor))
                {
                    panel.BackColor = root ? Background : Background;
                }
                break;
            case TextBox textBox:
                textBox.BackColor = Input;
                textBox.ForeColor = TextPrimary;
                break;
            case ComboBox comboBox:
                comboBox.BackColor = Input;
                comboBox.ForeColor = TextPrimary;
                break;
            case GradientButton gradientButton:
                gradientButton.StartColor = Primary;
                gradientButton.EndColor = PrimaryHover;
                gradientButton.Invalidate();
                break;
            case Button button:
                button.ForeColor = TextPrimary;
                if (button.BackColor == Primary || button.ForeColor == Color.White || button.Text.Contains("Add", StringComparison.OrdinalIgnoreCase))
                {
                    button.BackColor = Primary;
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = Input;
                    button.ForeColor = TextPrimary;
                }
                break;
            case BadgeLabel badge:
                if (IsDarkMode && badge.BackColor.GetBrightness() > 0.72f)
                {
                    badge.BackColor = Input;
                }
                break;
            case Label label:
                label.ForeColor = label.Font.Bold || label.Font.Size >= 11f ? TextPrimary : TextMuted;
                break;
            case DataGridView grid:
                StyleGrid(grid);
                break;
        }

        foreach (Control child in control.Controls)
        {
            ApplyThemeToControl(child);
        }
    }

    private static Color Blend(Color color, Color backColor, float amount)
    {
        var r = (int)(color.R * (1 - amount) + backColor.R * amount);
        var g = (int)(color.G * (1 - amount) + backColor.G * amount);
        var b = (int)(color.B * (1 - amount) + backColor.B * amount);
        return Color.FromArgb(r, g, b);
    }

    private static bool IsThemeManagedColor(Color color)
    {
        return color == Color.Transparent
               || color == Color.White
               || color == Color.FromArgb(246, 247, 251)
               || color == Color.FromArgb(242, 244, 248)
               || color == Color.FromArgb(250, 251, 253)
               || color == Color.FromArgb(244, 246, 250)
               || color == Color.FromArgb(18, 18, 18)
               || color == Color.FromArgb(30, 30, 30)
               || color == Color.FromArgb(42, 42, 42)
               || color == Color.FromArgb(42, 45, 50)
               || color == Color.FromArgb(48, 48, 48);
    }

    public static void StylePrimaryButton(Button button, int radius = 14)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Primary;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Resize += (_, _) => ApplyRoundedRegion(button, radius);
        button.HandleCreated += (_, _) => ApplyRoundedRegion(button, radius);
        button.MouseEnter += (_, _) => button.BackColor = PrimaryHover;
        button.MouseLeave += (_, _) => button.BackColor = Primary;
    }

    public static void StyleSecondaryButton(Button button, int radius = 14)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Input;
        button.ForeColor = TextPrimary;
        button.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Resize += (_, _) => ApplyRoundedRegion(button, radius);
        button.HandleCreated += (_, _) => ApplyRoundedRegion(button, radius);
        button.MouseEnter += (_, _) => button.BackColor = PrimarySoft;
        button.MouseLeave += (_, _) => button.BackColor = Input;
    }

    public static Panel CreateCard(Rectangle bounds)
    {
        var card = new Panel
        {
            BackColor = Surface,
            Location = bounds.Location,
            Size = bounds.Size,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(0)
        };
        ApplyCardChrome(card, 22);
        return card;
    }

    public static Panel CreateShadowCard(Rectangle bounds, int radius = 22)
    {
        var card = CreateCard(bounds);
        ApplyCardChrome(card, radius);
        return card;
    }

    public static void ApplyCardChrome(Panel panel, int radius = 22)
    {
        panel.DoubleBuffered(true);
        panel.Resize += (_, _) => ApplyRoundedRegion(panel, radius);
        panel.HandleCreated += (_, _) => ApplyRoundedRegion(panel, radius);
        panel.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), radius);
            using var border = new Pen(Border);
            e.Graphics.DrawPath(border, path);
        };
    }

    public static void StyleWebTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = Input;
        textBox.ForeColor = TextPrimary;
        if (textBox.Font?.Size < 10f)
        {
            textBox.Font = new Font("Segoe UI", 10f);
        }
    }

    public static void StyleComfortSingleLineTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = Input;
        textBox.ForeColor = TextPrimary;
        textBox.Font = new Font("Segoe UI", 10.5f);
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.None;
        textBox.WordWrap = false;
        textBox.AcceptsReturn = false;
        textBox.Height = 42;
        textBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }
        };
        textBox.Resize += (_, _) => ApplyRoundedRegion(textBox, 18);
        textBox.HandleCreated += (_, _) => ApplyRoundedRegion(textBox, 18);
    }

    public static Panel CreateTextInputRow(TextBox textBox, int totalWidth, string iconText = "")
    {
        var row = new Panel
        {
            Width = totalWidth,
            Height = 48,
            BackColor = Input,
            Padding = new Padding(string.IsNullOrEmpty(iconText) ? 16 : 50, 13, 16, 0)
        };

        if (!string.IsNullOrEmpty(iconText))
        {
            row.Controls.Add(new Label
            {
                Text = iconText,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Regular),
                Location = new Point(17, 13),
                Size = new Size(22, 22),
                TextAlign = ContentAlignment.MiddleCenter
            });
        }

        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = row.BackColor;
        textBox.ForeColor = TextPrimary;
        textBox.Font = new Font("Segoe UI", 11f);
        textBox.Multiline = false;
        textBox.Margin = Padding.Empty;
        textBox.Dock = DockStyle.Fill;

        row.Controls.Add(textBox);
        WireInputChrome(row, textBox, 18);
        return row;
    }

    public static Panel CreateSearchBox(TextBox textBox, int width)
    {
        textBox.PlaceholderText = "Search";
        return CreateTextInputRow(textBox, width, "⌕");
    }

    public static Button CreateNavButton(string text, int top, EventHandler click, bool active = false)
    {
        var button = new Button
        {
            Text = text,
            Width = 300,
            Height = 46,
            Left = 22,
            Top = top,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = active ? PrimarySoft : Sidebar,
            ForeColor = active ? Primary : TextPrimary,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 10.5f, active ? FontStyle.Bold : FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.Resize += (_, _) => ApplyRoundedRegion(button, 15);
        button.HandleCreated += (_, _) => ApplyRoundedRegion(button, 15);
        button.MouseEnter += (_, _) => button.BackColor = active ? PrimarySoft : SoftSurface;
        button.MouseLeave += (_, _) => button.BackColor = active ? PrimarySoft : Sidebar;
        button.Click += click;
        return button;
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToResizeRows = false;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.GridColor = Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersHeight = 48;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Surface;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(14, 8, 14, 8);
        grid.DefaultCellStyle.SelectionBackColor = PrimarySoft;
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.AlternatingRowsDefaultCellStyle.BackColor = SoftSurface;
        grid.DefaultCellStyle.Padding = new Padding(14, 10, 14, 10);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f);
        grid.RowTemplate.Height = 58;
    }

    public static Label CreatePill(string text, Color backColor, Color foreColor)
    {
        var label = new Label
        {
            Text = text,
            AutoSize = false,
            Size = new Size(104, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = backColor,
            ForeColor = foreColor,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
        };
        label.Resize += (_, _) => ApplyRoundedRegion(label, 15);
        label.HandleCreated += (_, _) => ApplyRoundedRegion(label, 15);
        return label;
    }

    public static void ApplyRoundedRegion(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            return;
        }

        using var path = RoundedRect(new Rectangle(0, 0, control.Width - 1, control.Height - 1), radius);
        control.Region = new Region(path);
    }

    public static Color ResolveParentBackColor(Control? control, Color fallback)
    {
        var parent = control?.Parent;
        while (parent is not null)
        {
            if (parent.BackColor != Color.Transparent && parent.BackColor != Color.Empty)
            {
                return Color.FromArgb(255, parent.BackColor);
            }

            parent = parent.Parent;
        }

        return Color.FromArgb(255, fallback);
    }

    public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            path.CloseFigure();
            return path;
        }

        var diameter = radius * 2;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static void WireInputChrome(Panel row, TextBox textBox, int radius)
    {
        row.DoubleBuffered(true);
        row.Resize += (_, _) => ApplyRoundedRegion(row, radius);
        row.HandleCreated += (_, _) => ApplyRoundedRegion(row, radius);
        row.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var focused = textBox.Focused;
            using var path = RoundedRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), radius);
            using var pen = new Pen(focused ? Primary : Border, focused ? 2 : 1);
            e.Graphics.DrawPath(pen, path);
        };
        textBox.GotFocus += (_, _) => row.Invalidate();
        textBox.LostFocus += (_, _) => row.Invalidate();
    }

    private static void DoubleBuffered(this Control control, bool enabled)
    {
        var prop = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        prop?.SetValue(control, enabled, null);
    }
}
