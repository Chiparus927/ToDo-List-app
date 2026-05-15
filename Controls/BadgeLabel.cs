using ToDoListApp.Utils;

namespace ToDoListApp.Controls;

public class BadgeLabel : Label
{
    public int Radius { get; set; } = 13;

    public BadgeLabel()
    {
        AutoSize = false;
        Height = 26;
        TextAlign = ContentAlignment.MiddleCenter;
        Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
        BackColor = AppTheme.PrimarySoft;
        ForeColor = AppTheme.Primary;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        AppTheme.ApplyRoundedRegion(this, Radius);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        AppTheme.ApplyRoundedRegion(this, Radius);
    }
}
