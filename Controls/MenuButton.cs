namespace ToDoListApp.Controls;

public class MenuButton : Button
{
    protected override bool ShowFocusCues => false;

    protected override void OnGotFocus(EventArgs e)
    {
        Parent?.Focus();
        base.OnGotFocus(e);
    }
}
