using System.Drawing.Drawing2D;
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
        Size = new Size(1180, 720);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AppTheme.StyleForm(this, new Size(1024, 640));
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Paint += PaintBackground;

        const int contentLeft = 34;
        const int contentWidth = 372;

        var card = AppTheme.CreateShadowCard(new Rectangle(394, 82, 448, 530), 28);
        card.BackColor = AppTheme.Surface;

        var title = new Label
        {
            Text = "Welcome back",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(contentLeft, 36),
            ForeColor = AppTheme.TextPrimary
        };
        var subtitle = new Label
        {
            Text = "Sign in to continue managing your tasks.",
            Font = new Font("Segoe UI", 11f),
            AutoSize = true,
            Location = new Point(contentLeft, 96),
            ForeColor = AppTheme.TextMuted
        };

        _txtEmail.PlaceholderText = "Email";
        var emailRow = AppTheme.CreateTextInputRow(_txtEmail, contentWidth, "✉");
        emailRow.Location = new Point(contentLeft, 166);

        _txtPassword.PlaceholderText = "Password";
        var pwdRow = PasswordRevealHelper.CreatePasswordRow(_txtPassword, contentWidth);
        pwdRow.Location = new Point(contentLeft, 232);

        var btnLogin = CreatePrimaryButton("Log in", new Point(contentLeft, 318), contentWidth);
        btnLogin.Click += (_, _) => Login();

        var btnRegister = CreateSecondaryButton("Create account", new Point(contentLeft, 380), contentWidth);
        btnRegister.Click += (_, _) =>
        {
            using var register = new RegisterForm(_authService);
            register.ShowDialog(this);
        };

        var divider = new Panel
        {
            Location = new Point(contentLeft, 458),
            Size = new Size(contentWidth, 1),
            BackColor = AppTheme.Border
        };
        var footer = new Label
        {
            Text = "Minimal planning for focused days.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = AppTheme.TextMuted,
            AutoSize = true,
            Location = new Point(contentLeft, 482)
        };

        card.Controls.AddRange([title, subtitle, emailRow, pwdRow, btnLogin, btnRegister, divider, footer]);
        Controls.Add(card);
    }

    private Button CreatePrimaryButton(string text, Point location, int width)
    {
        var button = new Button { Text = text, Width = width, Height = 50, Location = location };
        AppTheme.StylePrimaryButton(button, 16);
        return button;
    }

    private Button CreateSecondaryButton(string text, Point location, int width)
    {
        var button = new Button { Text = text, Width = width, Height = 50, Location = location };
        AppTheme.StyleSecondaryButton(button, 16);
        return button;
    }

    private void PaintBackground(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var start = AppTheme.IsDarkMode ? AppTheme.Background : Color.FromArgb(244, 248, 255);
        var end = AppTheme.IsDarkMode ? AppTheme.Surface : Color.FromArgb(250, 250, 252);
        using var brush = new LinearGradientBrush(ClientRectangle, start, end, 35f);
        e.Graphics.FillRectangle(brush, ClientRectangle);
        using var blue = new SolidBrush(Color.FromArgb(AppTheme.IsDarkMode ? 24 : 54, 10, 132, 255));
        using var mint = new SolidBrush(Color.FromArgb(AppTheme.IsDarkMode ? 18 : 45, 52, 199, 179));
        using var pink = new SolidBrush(Color.FromArgb(AppTheme.IsDarkMode ? 16 : 38, 255, 159, 194));
        e.Graphics.FillEllipse(blue, 92, 94, 280, 280);
        e.Graphics.FillEllipse(mint, 780, 86, 240, 240);
        e.Graphics.FillEllipse(pink, 116, 448, 340, 210);

        using var pen = new Pen(Color.FromArgb(46, AppTheme.Primary), 2);
        e.Graphics.DrawArc(pen, 875, 420, 120, 120, 20, 280);
        e.Graphics.DrawArc(pen, 900, 446, 70, 70, 210, 250);
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
