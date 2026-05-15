using System.Drawing.Drawing2D;
using ToDoListApp.Utils;

namespace ToDoListApp.Controls;

public class RoundedPanel : Panel
{
    public int Radius { get; set; } = 22;
    public Color BorderColor { get; set; } = Color.FromArgb(232, 235, 240);
    public bool DrawShadow { get; set; } = true;

    public RoundedPanel()
    {
        BackColor = Color.White;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var background = new SolidBrush(GetParentBackColor());
        e.Graphics.FillRectangle(background, ClientRectangle);
    }

    private Color GetParentBackColor()
    {
        var parent = Parent;
        while (parent is not null)
        {
            if (parent.BackColor != Color.Transparent)
            {
                return parent.BackColor;
            }

            parent = parent.Parent;
        }

        return AppTheme.Background;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        if (DrawShadow && !AppTheme.IsDarkMode)
        {
            using var shadowPath = AppTheme.RoundedRect(new Rectangle(4, 5, Width - 10, Height - 10), Radius);
            using var shadow = new SolidBrush(Color.FromArgb(18, 40, 50, 70));
            e.Graphics.FillPath(shadow, shadowPath);
        }

        using var path = AppTheme.RoundedRect(rect, Radius);
        using var fill = new SolidBrush(BackColor);
        using var border = new Pen(BorderColor);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);
    }
}
