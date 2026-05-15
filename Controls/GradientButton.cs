using System.Drawing.Drawing2D;
using ToDoListApp.Utils;

namespace ToDoListApp.Controls;

public class GradientButton : Button
{
    private bool _hovered;

    public int Radius { get; set; }
    public Color StartColor { get; set; } = AppTheme.Primary;
    public Color EndColor { get; set; } = Color.FromArgb(0, 102, 220);
    public Color HoverStartColor { get; set; } = Color.FromArgb(37, 150, 255);
    public Color HoverEndColor { get; set; } = AppTheme.PrimaryHover;

    public GradientButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = Color.White;
        Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var background = new SolidBrush(Parent?.BackColor ?? AppTheme.Background);
        pevent.Graphics.FillRectangle(background, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = AppTheme.RoundedRect(rect, Radius);
        if (Radius > 0)
        {
            using var shadow = new SolidBrush(Color.FromArgb(_hovered ? 38 : 24, 10, 132, 255));
            pevent.Graphics.FillPath(shadow, AppTheme.RoundedRect(new Rectangle(2, 4, Width - 5, Height - 5), Radius));
        }

        using var brush = new LinearGradientBrush(rect, _hovered ? HoverStartColor : StartColor, _hovered ? HoverEndColor : EndColor, 0f);
        pevent.Graphics.FillPath(brush, path);
        TextRenderer.DrawText(pevent.Graphics, Text, Font, rect, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
