using System.Drawing.Drawing2D;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class RegisterForm : Form
{
    private readonly AuthService _authService;
    private readonly TextBox _txtFullName = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPassword = new();
    private readonly TextBox _txtConfirmPassword = new();
    private readonly Label _validationMessage = new();
    private readonly Panel _strengthFill = new();
    private readonly Label _strengthText = new();

    public RegisterForm(AuthService authService)
    {
        _authService = authService;
        Text = "ToDo List - Create Account";
        Size = new Size(860, 720);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AppTheme.StyleForm(this, new Size(760, 640));
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Paint += PaintBackground;

        const int contentLeft = 34;
        const int contentWidth = 392;

        var formPanel = AppTheme.CreateShadowCard(new Rectangle(204, 44, 462, 606), 28);
        formPanel.BackColor = AppTheme.Surface;

        var lblTitle = new Label
        {
            Text = "Create account",
            Font = new Font("Segoe UI", 27, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(contentLeft, 28),
            ForeColor = AppTheme.TextPrimary
        };
        var hint = new Label
        {
            Text = "Set up your workspace in a few seconds.",
            Font = new Font("Segoe UI", 10.5f),
            AutoSize = true,
            Location = new Point(contentLeft, 84),
            ForeColor = AppTheme.TextMuted
        };

        _txtFullName.PlaceholderText = "Full name";
        var fullNameRow = AppTheme.CreateTextInputRow(_txtFullName, contentWidth, "●");
        fullNameRow.Location = new Point(contentLeft, 136);

        _txtEmail.PlaceholderText = "Email";
        var emailRow = AppTheme.CreateTextInputRow(_txtEmail, contentWidth, "✉");
        emailRow.Location = new Point(contentLeft, 198);

        _txtPassword.PlaceholderText = "Password";
        var pwdRow = PasswordRevealHelper.CreatePasswordRow(_txtPassword, contentWidth);
        pwdRow.Location = new Point(contentLeft, 260);
        _txtPassword.TextChanged += (_, _) => UpdatePasswordStrength();

        _txtConfirmPassword.PlaceholderText = "Confirm password";
        var confirmPwdRow = PasswordRevealHelper.CreatePasswordRow(_txtConfirmPassword, contentWidth);
        confirmPwdRow.Location = new Point(contentLeft, 322);

        var strengthBack = new Panel
        {
            Location = new Point(contentLeft, 390),
            Size = new Size(contentWidth, 8),
            BackColor = AppTheme.Input
        };
        AppTheme.ApplyRoundedRegion(strengthBack, 4);
        strengthBack.Resize += (_, _) => AppTheme.ApplyRoundedRegion(strengthBack, 4);
        _strengthFill.Location = new Point(0, 0);
        _strengthFill.Size = new Size(1, 8);
        _strengthFill.BackColor = AppTheme.Warning;
        strengthBack.Controls.Add(_strengthFill);

        _strengthText.Text = "Password strength";
        _strengthText.ForeColor = AppTheme.TextMuted;
        _strengthText.Font = new Font("Segoe UI", 9.5f);
        _strengthText.Location = new Point(contentLeft, 404);
        _strengthText.Size = new Size(contentWidth, 22);

        _validationMessage.ForeColor = AppTheme.TextMuted;
        _validationMessage.Font = new Font("Segoe UI", 9.5f);
        _validationMessage.Location = new Point(contentLeft, 430);
        _validationMessage.Size = new Size(contentWidth, 28);

        var btnRegister = new Button
        {
            Text = "Create account",
            Width = contentWidth,
            Height = 50,
            Location = new Point(contentLeft, 470)
        };
        AppTheme.StylePrimaryButton(btnRegister, 16);
        btnRegister.Click += (_, _) => Register();

        var btnLogin = new Button
        {
            Text = "Already have an account? Login",
            Width = contentWidth,
            Height = 46,
            Location = new Point(contentLeft, 530)
        };
        AppTheme.StyleSecondaryButton(btnLogin, 16);
        btnLogin.Click += (_, _) => Close();

        formPanel.Controls.AddRange([
            lblTitle, hint, fullNameRow, emailRow, pwdRow, confirmPwdRow,
            strengthBack, _strengthText, _validationMessage, btnRegister, btnLogin
        ]);
        Controls.Add(formPanel);
    }

    private void UpdatePasswordStrength()
    {
        var password = _txtPassword.Text;
        var score = 0;
        if (password.Length >= 8) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score++;

        var width = score switch { 0 => 1, 1 => 98, 2 => 196, 3 => 294, _ => 392 };
        var color = score switch { <= 1 => AppTheme.Danger, 2 => AppTheme.Warning, 3 => Color.FromArgb(48, 176, 199), _ => AppTheme.Success };
        var label = score switch { <= 1 => "Weak password", 2 => "Fair password", 3 => "Good password", _ => "Strong password" };
        _strengthFill.Width = width;
        _strengthFill.BackColor = color;
        AppTheme.ApplyRoundedRegion(_strengthFill, 4);
        _strengthText.Text = label;
        _strengthText.ForeColor = color;
    }

    private void Register()
    {
        if (Validator.IsNullOrWhiteSpace(_txtFullName.Text, _txtEmail.Text, _txtPassword.Text, _txtConfirmPassword.Text))
        {
            ShowValidation("Please fill in all fields.", AppTheme.Danger);
            return;
        }

        if (!_txtPassword.Text.Equals(_txtConfirmPassword.Text, StringComparison.Ordinal))
        {
            ShowValidation("Passwords do not match.", AppTheme.Danger);
            return;
        }

        var result = _authService.Register(_txtFullName.Text, _txtEmail.Text, _txtPassword.Text);
        if (!result.Success)
        {
            ShowValidation(result.Message, AppTheme.Danger);
            return;
        }

        ShowValidation(result.Message, AppTheme.Success);
        Helpers.ShowInfo(result.Message);
        Close();
    }

    private void ShowValidation(string message, Color color)
    {
        _validationMessage.Text = message;
        _validationMessage.ForeColor = color;
    }

    private void PaintBackground(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var start = AppTheme.IsDarkMode ? AppTheme.Background : Color.FromArgb(243, 248, 255);
        var end = AppTheme.IsDarkMode ? AppTheme.Surface : Color.FromArgb(250, 250, 252);
        using var brush = new LinearGradientBrush(ClientRectangle, start, end, 45f);
        e.Graphics.FillRectangle(brush, ClientRectangle);
        using var blue = new SolidBrush(Color.FromArgb(AppTheme.IsDarkMode ? 22 : 50, 10, 132, 255));
        using var violet = new SolidBrush(Color.FromArgb(AppTheme.IsDarkMode ? 16 : 34, 175, 82, 222));
        e.Graphics.FillEllipse(blue, 40, 54, 250, 250);
        e.Graphics.FillEllipse(violet, 610, 420, 240, 220);
    }

}
