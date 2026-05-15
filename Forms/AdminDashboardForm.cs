using ToDoListApp.Controls;
using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class AdminDashboardForm : Form, IThemeAware
{
    private readonly AdminService _adminService;
    private readonly UserModel _adminUser;
    private readonly FlowLayoutPanel _usersList = new();
    private readonly FlowLayoutPanel _tasksList = new();
    private readonly RoundedPanel _listHost = new();
    private readonly Button _btnUsersTab = new();
    private readonly Button _btnTasksTab = new();
    private readonly Label _lblStats = new();
    private readonly Label _lblUsersValue = new();
    private readonly Label _lblAdminsValue = new();
    private readonly Label _lblTasksValue = new();
    private readonly Label _lblActiveTasksValue = new();
    private readonly Label _lblCompletedTasksValue = new();
    private readonly TextBox _txtSearch = new();
    private readonly Button _btnUserMenu = new();
    private readonly Panel _userMenuPanel = new();
    private readonly Panel _chartPanel = new();
    private List<UserModel> _users = new();
    private List<AdminTaskModel> _tasks = new();
    private UserModel? _selectedUser;
    private bool _showTasks;

    public AdminDashboardForm(AdminService adminService, UserModel adminUser)
    {
        _adminService = adminService;
        _adminUser = adminUser;
        AppTheme.ApplyUserSettings(new UserSettingsService().Load(adminUser.Id));
        Text = $"Admin Dashboard - {_adminUser.FullName}";
        WindowState = FormWindowState.Maximized;
        AppTheme.StyleForm(this, new Size(1180, 760));
        InitializeComponents();
        LoadData();
    }

    private void InitializeComponents()
    {
        var sidebar = CreateSidebar();
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(42, 38, 42, 30),
            BackColor = AppTheme.Background
        };

        var topBar = new Panel { Location = new Point(28, 0), Size = new Size(1100, 104), BackColor = AppTheme.Background };
        var title = new Label
        {
            Text = "Welcome, Administrator!",
            Font = new Font("Segoe UI", 27, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0),
            ForeColor = AppTheme.TextPrimary
        };
        _lblStats.Location = new Point(4, 64);
        _lblStats.AutoSize = true;
        _lblStats.Font = new Font("Segoe UI", 10.5f);
        _lblStats.ForeColor = AppTheme.TextMuted;

        _txtSearch.PlaceholderText = "Search users or tasks...";
        var searchRow = AppTheme.CreateSearchBox(_txtSearch, 360);
        searchRow.Location = new Point(560, 18);
        _txtSearch.TextChanged += (_, _) => ApplyAdminFilters();
        ConfigureUserMenuButton();
        ConfigureUserMenuPanel();
        topBar.Controls.AddRange([title, _lblStats, searchRow, _btnUserMenu]);

        var statsPanel = new Panel { Location = new Point(28, 124), Size = new Size(1100, 112), BackColor = AppTheme.Background };
        statsPanel.Controls.AddRange([
            CreateStatCard(_lblUsersValue, "User", Color.FromArgb(238, 246, 255), AppTheme.Primary, new Point(0, 0)),
            CreateStatCard(_lblAdminsValue, "Admin", Color.FromArgb(244, 240, 255), Color.FromArgb(116, 86, 190), new Point(220, 0)),
            CreateStatCard(_lblActiveTasksValue, "Open", Color.FromArgb(238, 250, 243), AppTheme.Success, new Point(440, 0)),
            CreateStatCard(_lblCompletedTasksValue, "Done", Color.FromArgb(255, 248, 232), AppTheme.Warning, new Point(660, 0)),
            CreateStatCard(_lblTasksValue, "List", Color.FromArgb(232, 250, 255), Color.FromArgb(0, 150, 190), new Point(880, 0))
        ]);

        _chartPanel.SetBounds(28, 258, 1100, 132);
        _chartPanel.BackColor = AppTheme.Surface;
        AppTheme.ApplyCardChrome(_chartPanel, 24);
        _chartPanel.Paint += PaintChart;

        var switcher = new RoundedPanel
        {
            Location = new Point(28, 412),
            Size = new Size(230, 48),
            Radius = 18,
            BackColor = AppTheme.SoftSurface,
            DrawShadow = false,
            Padding = new Padding(4)
        };
        ConfigureSegmentButton(_btnUsersTab, "Users", true);
        ConfigureSegmentButton(_btnTasksTab, "All tasks", false);
        _btnUsersTab.SetBounds(4, 4, 108, 40);
        _btnTasksTab.SetBounds(116, 4, 110, 40);
        _btnUsersTab.Click += (_, _) => ShowUsersTab();
        _btnTasksTab.Click += (_, _) => ShowTasksTab();
        switcher.Controls.AddRange([_btnUsersTab, _btnTasksTab]);

        _usersList.Dock = DockStyle.Fill;
        _usersList.AutoScroll = true;
        _usersList.FlowDirection = FlowDirection.TopDown;
        _usersList.WrapContents = false;
        _usersList.BackColor = AppTheme.Surface;

        _tasksList.Dock = DockStyle.Fill;
        _tasksList.AutoScroll = true;
        _tasksList.FlowDirection = FlowDirection.TopDown;
        _tasksList.WrapContents = false;
        _tasksList.BackColor = AppTheme.Surface;
        _tasksList.Visible = false;

        _listHost.Location = new Point(28, 476);
        _listHost.Size = new Size(1100, 356);
        _listHost.Radius = 26;
        _listHost.BackColor = AppTheme.Surface;
        _listHost.BorderColor = AppTheme.Border;
        _listHost.Padding = new Padding(18);
        _listHost.Controls.AddRange([_usersList, _tasksList]);

        void LayoutMainArea()
        {
            const int leftGap = 28;
            const int rightGap = 20;
            var w = Math.Max(560, contentPanel.ClientSize.Width - leftGap - rightGap);
            var h = contentPanel.ClientSize.Height;
            topBar.SetBounds(leftGap, 0, w, 104);
            _btnUserMenu.Left = Math.Max(320, w - _btnUserMenu.Width - 2);
            searchRow.Left = Math.Max(300, _btnUserMenu.Left - searchRow.Width - 16);
            _userMenuPanel.Left = leftGap + Math.Max(0, _btnUserMenu.Right - _userMenuPanel.Width);
            _userMenuPanel.Top = topBar.Bottom - 4;
            statsPanel.SetBounds(leftGap, 124, w, 112);
            PositionStatCards(statsPanel, w);
            _chartPanel.SetBounds(leftGap, 258, w, 132);
            switcher.SetBounds(leftGap, 412, 230, 48);
            _listHost.SetBounds(leftGap, 476, w, Math.Max(280, h - 476));
            ResizeUserCards();
            ResizeTaskCards();
            _userMenuPanel.BringToFront();
        }

        contentPanel.Resize += (_, _) => LayoutMainArea();
        contentPanel.HandleCreated += (_, _) => LayoutMainArea();
        contentPanel.Controls.AddRange([topBar, statsPanel, _chartPanel, switcher, _listHost, _userMenuPanel]);

        Controls.Add(contentPanel);
        Controls.Add(sidebar);
    }

    private Panel CreateSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 332, BackColor = AppTheme.Sidebar };
        sidebar.Controls.AddRange([
            new Label
            {
                Text = "Control Center",
                ForeColor = AppTheme.TextPrimary,
                Font = new Font("Segoe UI", 21f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 28)
            },
            new Label
            {
                Text = "Admin workspace",
                ForeColor = AppTheme.TextMuted,
                Font = new Font("Segoe UI", 9.5f),
                AutoSize = true,
                Location = new Point(32, 78)
            },
            AppTheme.CreateNavButton("Dashboard", 138, (_, _) => LoadData(), true),
            AppTheme.CreateNavButton("User management", 192, (_, _) => _usersList.Focus()),
            AppTheme.CreateNavButton("Recent activity", 246, (_, _) => LoadData()),
            AppTheme.CreateNavButton("Settings", 300, (_, _) => OpenSettings())
        ]);
        return sidebar;
    }

    private void OpenSettings()
    {
        using var settings = new SettingsForm(_adminUser);
        settings.ShowDialog(this);
    }

    private static RoundedPanel CreateStatCard(Label valueLabel, string icon, Color background, Color accent, Point location)
    {
        var card = new RoundedPanel
        {
            Location = location,
            Size = new Size(204, 104),
            Radius = 24,
            BackColor = AppTheme.IsDarkMode ? AppTheme.SoftSurface : background,
            BorderColor = AppTheme.Border
        };
        var iconLabel = new BadgeLabel
        {
            Text = icon,
            Location = new Point(18, 18),
            Width = 66,
            BackColor = AppTheme.IsDarkMode ? AppTheme.Input : Color.White,
            ForeColor = accent
        };
        valueLabel.Text = "0";
        valueLabel.Location = new Point(94, 34);
        valueLabel.AutoSize = true;
        valueLabel.Font = new Font("Segoe UI", 30f, FontStyle.Bold);
        valueLabel.ForeColor = accent;

        card.Controls.AddRange([iconLabel, valueLabel]);
        return card;
    }

    private static void PositionStatCards(Panel statsPanel, int availableWidth)
    {
        var cards = statsPanel.Controls.OfType<Panel>().ToList();
        var gap = 16;
        var cardWidth = Math.Max(150, (availableWidth - gap * (cards.Count - 1)) / Math.Max(1, cards.Count));
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].SetBounds(i * (cardWidth + gap), 0, cardWidth, 104);
        }
    }

    private static void ConfigureSegmentButton(Button button, string text, bool active)
    {
        button.Text = text;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = active ? AppTheme.Input : AppTheme.SoftSurface;
        button.ForeColor = active ? AppTheme.Primary : AppTheme.TextMuted;
        button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 14);
        button.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(button, 14);
    }

    private void ShowUsersTab()
    {
        _showTasks = false;
        _usersList.Visible = true;
        _tasksList.Visible = false;
        StyleTabState();
    }

    private void ShowTasksTab()
    {
        _showTasks = true;
        _usersList.Visible = false;
        _tasksList.Visible = true;
        StyleTabState();
    }

    private void StyleTabState()
    {
        _btnUsersTab.BackColor = _showTasks ? AppTheme.SoftSurface : AppTheme.Input;
        _btnUsersTab.ForeColor = _showTasks ? AppTheme.TextMuted : AppTheme.Primary;
        _btnTasksTab.BackColor = _showTasks ? AppTheme.Input : AppTheme.SoftSurface;
        _btnTasksTab.ForeColor = _showTasks ? AppTheme.Primary : AppTheme.TextMuted;
    }

    private void ConfigureUserMenuButton()
    {
        var initial = string.IsNullOrWhiteSpace(_adminUser.FullName) ? "A" : _adminUser.FullName.Trim()[0].ToString().ToUpperInvariant();
        _btnUserMenu.Text = initial;
        _btnUserMenu.Size = new Size(48, 48);
        _btnUserMenu.Location = new Point(1034, 18);
        _btnUserMenu.FlatStyle = FlatStyle.Flat;
        _btnUserMenu.FlatAppearance.BorderSize = 0;
        _btnUserMenu.BackColor = AppTheme.Primary;
        _btnUserMenu.ForeColor = Color.White;
        _btnUserMenu.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        _btnUserMenu.Cursor = Cursors.Hand;
        _btnUserMenu.Resize += (_, _) => AppTheme.ApplyRoundedRegion(_btnUserMenu, 24);
        _btnUserMenu.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(_btnUserMenu, 24);
        _btnUserMenu.Click += (_, _) => _userMenuPanel.Visible = !_userMenuPanel.Visible;
    }

    private void ConfigureUserMenuPanel()
    {
        _userMenuPanel.Size = new Size(222, 116);
        _userMenuPanel.BackColor = AppTheme.Surface;
        AppTheme.ApplyCardChrome(_userMenuPanel, 18);
        _userMenuPanel.Visible = false;

        var name = new Label
        {
            Text = $"{_adminUser.FullName} - admin",
            Location = new Point(16, 14),
            Size = new Size(190, 30),
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var logout = new Button
        {
            Text = "Logout",
            Location = new Point(12, 58),
            Size = new Size(198, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.Input,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 10.5f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        logout.FlatAppearance.BorderSize = 0;
        logout.MouseEnter += (_, _) => logout.BackColor = AppTheme.PrimarySoft;
        logout.MouseLeave += (_, _) => logout.BackColor = AppTheme.Input;
        logout.Click += (_, _) => Application.Restart();
        logout.Resize += (_, _) => AppTheme.ApplyRoundedRegion(logout, 13);
        logout.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(logout, 13);
        _userMenuPanel.Controls.AddRange([name, logout]);
    }

    private void PaintChart(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var titleFont = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
        using var labelFont = new Font("Segoe UI", 9.5f);
        using var primary = new SolidBrush(AppTheme.TextPrimary);
        using var muted = new SolidBrush(AppTheme.TextMuted);
        e.Graphics.DrawString("Task statistics", titleFont, primary, 22, 16);

        var completed = _tasks.Count(t => t.IsCompleted);
        var active = Math.Max(0, _tasks.Count - completed);
        var total = Math.Max(1, completed + active);
        DrawBar(e.Graphics, "Active", active, total, 24, 54, AppTheme.Warning);
        DrawBar(e.Graphics, "Completed", completed, total, 24, 88, AppTheme.Success);
        e.Graphics.DrawString("Refresh to update recent activity.", labelFont, muted, Math.Min(560, Math.Max(24, _chartPanel.Width - 340)), 54);
    }

    private static void DrawBar(Graphics graphics, string label, int value, int total, int x, int y, Color color)
    {
        using var labelFont = new Font("Segoe UI", 9.5f);
        using var muted = new SolidBrush(AppTheme.TextMuted);
        using var text = new SolidBrush(AppTheme.TextPrimary);
        graphics.DrawString(label, labelFont, muted, x, y - 2);
        var barX = x + 96;
        var barWidth = 360;
        using var back = new SolidBrush(AppTheme.Input);
        using var fill = new SolidBrush(color);
        graphics.FillRoundedRectangle(back, new Rectangle(barX, y, barWidth, 12), 6);
        graphics.FillRoundedRectangle(fill, new Rectangle(barX, y, Math.Max(8, barWidth * value / total), 12), 6);
        graphics.DrawString(value.ToString(), labelFont, text, barX + barWidth + 16, y - 4);
    }

    private void LoadData()
    {
        try
        {
            _users = _adminService.GetUsers();
            _tasks = _adminService.GetAllTasks();

            var admins = _users.Count(u => u.IsAdmin);
            var completed = _tasks.Count(t => t.IsCompleted);
            var active = _tasks.Count - completed;

            _lblUsersValue.Text = _users.Count.ToString();
            _lblAdminsValue.Text = admins.ToString();
            _lblTasksValue.Text = _tasks.Count.ToString();
            _lblActiveTasksValue.Text = active.ToString();
            _lblCompletedTasksValue.Text = completed.ToString();
            _lblStats.Text = $"Logged in as {_adminUser.FullName}. Manage users and monitor all tasks from here.";
            _chartPanel.Invalidate();
            ApplyAdminFilters();
        }
        catch (Exception ex)
        {
            Helpers.ShowError($"Could not load admin data: {ex.Message}");
        }
    }

    private void ApplyAdminFilters()
    {
        var search = _txtSearch.Text.Trim();
        var filteredUsers = _users;
        var filteredTasks = _tasks;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredUsers = _users
                .Where(user => user.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || user.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || user.Role.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            filteredTasks = _tasks
                .Where(task => task.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || task.UserEmail.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || task.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || task.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || task.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase)
                               || task.Status.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        RenderUserCards(filteredUsers);
        RenderTaskCards(filteredTasks);
    }

    public void ApplyTheme()
    {
        BackColor = AppTheme.Background;
        _chartPanel.BackColor = AppTheme.Surface;
        _usersList.BackColor = AppTheme.Surface;
        _tasksList.BackColor = AppTheme.Surface;
        _listHost.BackColor = AppTheme.Surface;
        _listHost.BorderColor = AppTheme.Border;
        _userMenuPanel.BackColor = AppTheme.Surface;
        StyleTabState();
        RestyleStatCards(this);
        ApplyAdminFilters();
        _chartPanel.Invalidate();
    }

    private static void RestyleStatCards(Control root)
    {
        foreach (Control control in root.Controls)
        {
            if (control is RoundedPanel card)
            {
                var badge = card.Controls.OfType<BadgeLabel>().FirstOrDefault();
                if (badge is not null)
                {
                    var (background, accent) = badge.Text switch
                    {
                        "User" => (Color.FromArgb(238, 246, 255), AppTheme.Primary),
                        "Admin" => (Color.FromArgb(244, 240, 255), Color.FromArgb(116, 86, 190)),
                        "Open" => (Color.FromArgb(238, 250, 243), AppTheme.Success),
                        "Done" => (Color.FromArgb(255, 248, 232), AppTheme.Warning),
                        "List" => (Color.FromArgb(232, 250, 255), Color.FromArgb(0, 150, 190)),
                        _ => (AppTheme.Surface, AppTheme.Primary)
                    };
                    card.BackColor = AppTheme.IsDarkMode ? AppTheme.SoftSurface : background;
                    card.BorderColor = AppTheme.Border;
                    badge.BackColor = AppTheme.IsDarkMode ? AppTheme.Input : Color.White;
                    badge.ForeColor = accent;
                }
            }

            if (control.HasChildren)
            {
                RestyleStatCards(control);
            }
        }
    }

    private void RenderUserCards(List<UserModel> users)
    {
        _usersList.SuspendLayout();
        _usersList.Controls.Clear();
        foreach (var user in users)
        {
            var card = new UserCardControl(user);
            card.SelectedUser += (_, selected) => _selectedUser = selected;
            card.MakeAdmin += (_, selected) => ChangeUserRole(selected, "admin");
            card.MakeUser += (_, selected) => ChangeUserRole(selected, "user");
            card.DeleteUser += (_, selected) => DeleteUser(selected);
            _usersList.Controls.Add(card);
        }
        ResizeUserCards();
        _usersList.ResumeLayout();
    }

    private void ResizeUserCards()
    {
        var width = Math.Max(520, _usersList.ClientSize.Width - 24);
        foreach (UserCardControl card in _usersList.Controls.OfType<UserCardControl>())
        {
            card.Width = width;
        }
    }

    private void RenderTaskCards(List<AdminTaskModel> tasks)
    {
        _tasksList.SuspendLayout();
        _tasksList.Controls.Clear();
        foreach (var task in tasks)
        {
            _tasksList.Controls.Add(new AdminTaskCardControl(task));
        }
        ResizeTaskCards();
        _tasksList.ResumeLayout();
    }

    private void ResizeTaskCards()
    {
        var width = Math.Max(520, _tasksList.ClientSize.Width - 24);
        foreach (AdminTaskCardControl card in _tasksList.Controls.OfType<AdminTaskCardControl>())
        {
            card.Width = width;
        }
    }

    private void ChangeSelectedUserRole(string role)
    {
        if (_selectedUser is null)
        {
            Helpers.ShowInfo("Select a user first.");
            return;
        }

        ChangeUserRole(_selectedUser, role);
    }

    private void ChangeUserRole(UserModel selected, string role)
    {
        if (selected.Id == _adminUser.Id && role.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            Helpers.ShowError("You cannot remove your own admin role while logged in.");
            return;
        }

        _adminService.UpdateUserRole(selected.Id, role);
        LoadData();
    }

    private void DeleteSelectedUser()
    {
        if (_selectedUser is null)
        {
            Helpers.ShowInfo("Select a user first.");
            return;
        }

        DeleteUser(_selectedUser);
    }

    private void DeleteUser(UserModel selected)
    {
        if (selected.Id == _adminUser.Id)
        {
            Helpers.ShowError("You cannot delete your own account while logged in.");
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete user {selected.FullName} and all their tasks?",
            "Confirm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        _adminService.DeleteUser(selected.Id);
        LoadData();
    }
}

public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
    {
        using var path = AppTheme.RoundedRect(bounds, radius);
        graphics.FillPath(brush, path);
    }
}
