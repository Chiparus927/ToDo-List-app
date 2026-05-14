namespace ToDoListApp.Utils;

public static class AppTheme
{
    public static readonly Color Background = Color.FromArgb(245, 247, 251);
    public static readonly Color Sidebar = Color.FromArgb(247, 248, 250);
    public static readonly Color Surface = Color.White;
    public static readonly Color SoftSurface = Color.FromArgb(246, 249, 255);
    public static readonly Color Input = Color.FromArgb(250, 251, 253);
    public static readonly Color Primary = Color.FromArgb(0, 122, 255);
    public static readonly Color PrimaryHover = Color.FromArgb(0, 97, 214);
    public static readonly Color PrimarySoft = Color.FromArgb(229, 241, 255);
    public static readonly Color TextPrimary = Color.FromArgb(28, 28, 30);
    public static readonly Color TextMuted = Color.FromArgb(99, 99, 102);
    public static readonly Color Border = Color.FromArgb(218, 220, 224);
    public static readonly Font TitleFont = new("Segoe UI", 20, FontStyle.Bold);
    public static readonly Font LabelFont = new("Segoe UI", 9, FontStyle.Regular);

    public static void StylePrimaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Primary;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    public static void StyleSecondaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Border;
        button.BackColor = Color.FromArgb(241, 243, 246);
        button.ForeColor = TextPrimary;
        button.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    public static Panel CreateCard(Rectangle bounds)
    {
        return new Panel
        {
            BackColor = Surface,
            Location = bounds.Location,
            Size = bounds.Size,
            BorderStyle = BorderStyle.None
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
        textBox.Font = new Font("Segoe UI", 11f);
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

    public static Panel CreateTextInputRow(TextBox textBox, int totalWidth)
    {
        var row = new Panel
        {
            Width = totalWidth,
            Height = 42,
            BackColor = Input,
            Padding = new Padding(16, 10, 16, 0)
        };

        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = row.BackColor;
        textBox.ForeColor = TextPrimary;
        textBox.Font = new Font("Segoe UI", 11f);
        textBox.Multiline = false;
        textBox.Margin = Padding.Empty;
        textBox.Dock = DockStyle.Fill;

        row.Controls.Add(textBox);
        row.Resize += (_, _) => ApplyRoundedRegion(row, 18);
        row.HandleCreated += (_, _) => ApplyRoundedRegion(row, 18);

        return row;
    }

    public static Button CreateNavButton(string text, int top, EventHandler click, bool active = false)
    {
        var button = new Button
        {
            Text = text,
            Width = 318,
            Height = 44,
            Left = 16,
            Top = top,
            FlatStyle = FlatStyle.Flat,
            BackColor = active ? PrimarySoft : Sidebar,
            ForeColor = active ? Primary : TextPrimary,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10.5f, active ? FontStyle.Bold : FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.Paint += (_, _) =>
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 26, 26, 180, 90);
            path.AddArc(button.Width - 27, 0, 26, 26, 270, 90);
            path.AddArc(button.Width - 27, button.Height - 27, 26, 26, 0, 90);
            path.AddArc(0, button.Height - 27, 26, 26, 90, 90);
            path.CloseFigure();
            button.Region = new Region(path);
        };
        button.MouseEnter += (_, _) => button.BackColor = active ? PrimarySoft : Color.FromArgb(239, 241, 245);
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
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.GridColor = Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 8, 12, 8);
        grid.DefaultCellStyle.SelectionBackColor = PrimarySoft;
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f);
        grid.RowTemplate.Height = 48;
    }

    public static void ApplyRoundedRegion(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            return;
        }

        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(0, 0, diameter, diameter, 180, 90);
        path.AddArc(control.Width - diameter - 1, 0, diameter, diameter, 270, 90);
        path.AddArc(control.Width - diameter - 1, control.Height - diameter - 1, diameter, diameter, 0, 90);
        path.AddArc(0, control.Height - diameter - 1, diameter, diameter, 90, 90);
        path.CloseFigure();
        control.Region = new Region(path);
    }
}
