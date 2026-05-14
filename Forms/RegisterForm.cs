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

    public RegisterForm(AuthService authService)
    {
        _authService = authService;
        Text = "ToDo List - Create Account";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 640);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        BackColor = AppTheme.Background;

        const int contentLeft = 26;
        const int contentWidth = 340;

        var formPanel = AppTheme.CreateCard(new Rectangle(190, 58, 380, 510));
        formPanel.BackColor = Color.White;
        formPanel.Paint += (_, e) =>
        {
            using var border = new Pen(AppTheme.Border);
            e.Graphics.DrawRectangle(border, 0, 0, formPanel.Width - 1, formPanel.Height - 1);
        };
        var lblTitle = new Label
        {
            Text = "Create account",
            Font = new Font("Segoe UI", 25, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(contentLeft, 0),
            ForeColor = AppTheme.TextPrimary
        };
        var hint = new Label
        {
            Text = "Use your email and a strong password.",
            Font = new Font("Segoe UI", 10.5f),
            AutoSize = true,
            Location = new Point(contentLeft, 52),
            ForeColor = AppTheme.TextMuted
        };

        _txtFullName.PlaceholderText = "Full name";
        var fullNameRow = AppTheme.CreateTextInputRow(_txtFullName, contentWidth);
        fullNameRow.Location = new Point(contentLeft, 112);

        _txtEmail.PlaceholderText = "Email";
        var emailRow = AppTheme.CreateTextInputRow(_txtEmail, contentWidth);
        emailRow.Location = new Point(contentLeft, 172);

        _txtPassword.PlaceholderText = "Password";
        var pwdRow = PasswordRevealHelper.CreatePasswordRow(_txtPassword, contentWidth);
        pwdRow.Location = new Point(contentLeft, 232);

        _txtConfirmPassword.PlaceholderText = "Confirm password";
        var confirmPwdRow = PasswordRevealHelper.CreatePasswordRow(_txtConfirmPassword, contentWidth);
        confirmPwdRow.Location = new Point(contentLeft, 292);

        var lblPasswordRule = new Label
        {
            Text = "At least 8 characters, one uppercase letter, and one special character.",
            MaximumSize = new Size(contentWidth, 0),
            AutoSize = true,
            ForeColor = AppTheme.TextMuted,
            Location = new Point(contentLeft, 350),
            Font = new Font("Segoe UI", 9.5f)
        };

        var btnRegister = new Button
        {
            Text = "Create account",
            Width = contentWidth,
            Height = 44,
            Location = new Point(contentLeft, 412),
            FlatStyle = FlatStyle.Flat
        };
        AppTheme.StylePrimaryButton(btnRegister);
        AppTheme.ApplyRoundedRegion(btnRegister, 12);
        btnRegister.MouseEnter += (_, _) => btnRegister.BackColor = AppTheme.PrimaryHover;
        btnRegister.MouseLeave += (_, _) => btnRegister.BackColor = AppTheme.Primary;
        btnRegister.Click += (_, _) => Register();

        formPanel.Controls.AddRange([lblTitle, hint, fullNameRow, emailRow, pwdRow, confirmPwdRow, lblPasswordRule, btnRegister]);
        Controls.Add(formPanel);
    }

    private void Register()
    {
        if (Validator.IsNullOrWhiteSpace(_txtFullName.Text, _txtEmail.Text, _txtPassword.Text, _txtConfirmPassword.Text))
        {
            Helpers.ShowError("Please fill in all fields.");
            return;
        }

        if (!_txtPassword.Text.Equals(_txtConfirmPassword.Text, StringComparison.Ordinal))
        {
            Helpers.ShowError("Passwords do not match.");
            return;
        }

        var result = _authService.Register(_txtFullName.Text, _txtEmail.Text, _txtPassword.Text);
        if (!result.Success)
        {
            Helpers.ShowError(result.Message);
            return;
        }

        Helpers.ShowInfo(result.Message);
        Close();
    }
}
