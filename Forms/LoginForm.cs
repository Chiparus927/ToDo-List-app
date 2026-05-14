using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class LoginForm : Form
{
    private readonly AuthService _authService;
    private readonly TaskService _taskService;
    private readonly AdminService _adminService;
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPassword = new();

    public LoginForm(AuthService authService, TaskService taskService, AdminService adminService)
    {
        _authService = authService;
        _taskService = taskService;
        _adminService = adminService;

        Text = "ToDo List - Login";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1180, 720);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        BackColor = AppTheme.Background;

        const int contentLeft = 30;
        const int contentWidth = 370;

        var card = AppTheme.CreateCard(new Rectangle(375, 88, 430, 500));
        card.BackColor = Color.White;
        card.Paint += (_, e) =>
        {
            using var border = new Pen(AppTheme.Border);
            e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
        };
        var title = new Label
        {
            Text = "Welcome back",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(contentLeft, 34),
            ForeColor = AppTheme.TextPrimary
        };
        var subtitle = new Label
        {
            Text = "Sign in and continue organizing your work.",
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(contentLeft, 88),
            ForeColor = AppTheme.TextMuted
        };

        _txtEmail.PlaceholderText = "Email";
        var emailRow = AppTheme.CreateTextInputRow(_txtEmail, contentWidth);
        emailRow.Location = new Point(contentLeft, 154);

        _txtPassword.PlaceholderText = "Password";
        var pwdRow = PasswordRevealHelper.CreatePasswordRow(_txtPassword, contentWidth);
        pwdRow.Location = new Point(contentLeft, 218);

        var btnLogin = CreatePrimaryButton("Log in", new Point(contentLeft, 298), contentWidth);
        btnLogin.Click += (_, _) => Login();

        var btnRegister = CreateSecondaryButton("Create account", new Point(contentLeft, 354), contentWidth);
        btnRegister.Click += (_, _) =>
        {
            using var register = new RegisterForm(_authService);
            register.ShowDialog(this);
        };

        var footer = new Label
        {
            Text = "Simple daily planning for users and administrators.",
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = AppTheme.TextMuted,
            AutoSize = true,
            Location = new Point(contentLeft, 428)
        };

        card.Controls.AddRange([title, subtitle, emailRow, pwdRow, btnLogin, btnRegister, footer]);
        Controls.Add(card);
    }

    private Button CreatePrimaryButton(string text, Point location, int width)
    {
        var button = new Button { Text = text, Width = width, Height = 46, Location = location, FlatStyle = FlatStyle.Flat };
        AppTheme.StylePrimaryButton(button);
        AppTheme.ApplyRoundedRegion(button, 12);
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 12);
        button.MouseEnter += (_, _) => button.BackColor = AppTheme.PrimaryHover;
        button.MouseLeave += (_, _) => button.BackColor = AppTheme.Primary;
        return button;
    }

    private Button CreateSecondaryButton(string text, Point location, int width)
    {
        var button = new Button { Text = text, Width = width, Height = 46, Location = location, FlatStyle = FlatStyle.Flat };
        AppTheme.StyleSecondaryButton(button);
        AppTheme.ApplyRoundedRegion(button, 12);
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 12);
        button.MouseEnter += (_, _) => button.BackColor = Color.FromArgb(232, 235, 240);
        button.MouseLeave += (_, _) => button.BackColor = Color.FromArgb(241, 243, 246);
        return button;
    }

    private void Login()
    {
        var user = _authService.Login(_txtEmail.Text, _txtPassword.Text);
        if (user is null)
        {
            Helpers.ShowError("Invalid email or password.");
            return;
        }

        Hide();
        Form dashboard = user.IsAdmin
            ? new AdminDashboardForm(_adminService, user)
            : new DashboardForm(_taskService, user);
        dashboard.FormClosed += (_, _) => Close();
        dashboard.Show();
    }
}
