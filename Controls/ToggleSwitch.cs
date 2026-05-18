using System.Drawing.Drawing2D;
using ToDoListApp.Utils;

namespace ToDoListApp.Controls;

public class ToggleSwitch : Control
{
    private bool _checked;
    private bool _hovered;

    public event EventHandler? CheckedChanged;

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value)
            {
                return;
            }

            _checked = value;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public ToggleSwitch()
    {
        Size = new Size(54, 30);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
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

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var parentBack = new SolidBrush(Parent?.BackColor ?? AppTheme.Surface);
        e.Graphics.FillRectangle(parentBack, ClientRectangle);

        var backColor = Checked
            ? (_hovered ? AppTheme.PrimaryHover : AppTheme.Primary)
            : GetOffTrackColor();

        using var track = new SolidBrush(backColor);
        using var trackPath = AppTheme.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), Height / 2);
        e.Graphics.FillPath(track, trackPath);

        var knobSize = Height - 8;
        var knobX = Checked ? Width - knobSize - 4 : 4;
        using var shadow = new SolidBrush(Color.FromArgb(30, 40, 50, 70));
        e.Graphics.FillEllipse(shadow, knobX + 1, 5, knobSize, knobSize);
        using var knob = new SolidBrush(Color.White);
        e.Graphics.FillEllipse(knob, knobX, 4, knobSize, knobSize);
    }

    private Color GetOffTrackColor()
    {
        if (AppTheme.IsDarkMode)
        {
            return _hovered ? Color.FromArgb(82, 90, 104) : Color.FromArgb(65, 72, 84);
        }

        return _hovered ? Color.FromArgb(218, 224, 233) : Color.FromArgb(229, 233, 240);
    }
}
