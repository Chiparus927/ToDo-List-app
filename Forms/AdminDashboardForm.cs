using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class AdminDashboardForm : Form
{
    private readonly AdminService _adminService;
    private readonly UserModel _adminUser;
    private readonly DataGridView _usersGrid = new();
    private readonly DataGridView _tasksGrid = new();
    private readonly Label _lblStats = new();
    private readonly Label _lblUsersValue = new();
    private readonly Label _lblAdminsValue = new();
    private readonly Label _lblTasksValue = new();
    private readonly Label _lblActiveTasksValue = new();
    private readonly Label _lblCompletedTasksValue = new();
    private readonly TextBox _txtSearch = new();
    private readonly Button _btnUserMenu = new();
    private readonly Panel _userMenuPanel = new();
    private List<UserModel> _users = new();
    private List<AdminTaskModel> _tasks = new();

    public AdminDashboardForm(AdminService adminService, UserModel adminUser)
    {
        _adminService = adminService;
        _adminUser = adminUser;
        Text = $"Admin Dashboard - {_adminUser.FullName}";
        WindowState = FormWindowState.Maximized;
        InitializeComponents();
        LoadData();
    }

    private void InitializeComponents()
    {
        BackColor = AppTheme.Background;

        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 350,
            BackColor = AppTheme.Sidebar
        };

        var appName = new Label
        {
            Text = "ToDo List",
            ForeColor = AppTheme.Primary,
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(34, 34)
        };

        var btnRefresh = AppTheme.CreateNavButton("Admin Dashboard", 126, (_, _) => LoadData(), true);
        var btnMakeAdmin = AppTheme.CreateNavButton("Make admin", 190, (_, _) => ChangeSelectedUserRole("admin"));
        var btnMakeUser = AppTheme.CreateNavButton("Make user", 236, (_, _) => ChangeSelectedUserRole("user"));
        var btnDeleteUser = AppTheme.CreateNavButton("Delete user", 282, (_, _) => DeleteSelectedUser());

        sidebar.Controls.AddRange([appName, btnRefresh, btnMakeAdmin, btnMakeUser, btnDeleteUser]);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(48, 36, 48, 36),
            BackColor = Color.White
        };

        var topBar = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(1100, 112),
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "Administrator",
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 40),
            ForeColor = AppTheme.TextPrimary
        };

        _txtSearch.PlaceholderText = "Search users or tasks...";
        _txtSearch.Width = 376;
        _txtSearch.Location = new Point(640, 54);
        AppTheme.StyleComfortSingleLineTextBox(_txtSearch);
        _txtSearch.TextChanged += (_, _) => ApplyAdminFilters();
        ConfigureUserMenuButton();
        ConfigureUserMenuPanel();

        _lblStats.Location = new Point(0, 122);
        _lblStats.AutoSize = true;
        _lblStats.Font = new Font("Segoe UI", 11f);
        _lblStats.ForeColor = AppTheme.TextMuted;

        var statsPanel = new Panel
        {
            Location = new Point(0, 164),
            Size = new Size(1120, 112),
            BackColor = Color.White
        };

        var usersCard = CreateStatCard("Users", _lblUsersValue, new Point(0, 0), Color.FromArgb(245, 247, 255));
        var adminsCard = CreateStatCard("Admins", _lblAdminsValue, new Point(224, 0), AppTheme.PrimarySoft);
        var tasksCard = CreateStatCard("Tasks", _lblTasksValue, new Point(448, 0), Color.FromArgb(242, 250, 246));
        var activeCard = CreateStatCard("Active", _lblActiveTasksValue, new Point(672, 0), Color.FromArgb(255, 249, 235));
        var completedCard = CreateStatCard("Completed", _lblCompletedTasksValue, new Point(896, 0), Color.FromArgb(242, 248, 255));
        statsPanel.Controls.AddRange([usersCard, adminsCard, tasksCard, activeCard, completedCard]);

        var tabs = new TabControl
        {
            Location = new Point(0, 300),
            Size = new Size(900, 520),
            Font = new Font("Segoe UI", 10f)
        };

        var usersPage = new TabPage("Users") { BackColor = Color.White };
        var tasksPage = new TabPage("All tasks") { BackColor = Color.White };

        ConfigureGrid(_usersGrid);
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", Width = 70 });
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = "FullName", Width = 220 });
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", Width = 280 });
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Role", DataPropertyName = "Role", Width = 120 });
        _usersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = "CreatedAt", Width = 170 });

        ConfigureGrid(_tasksGrid);
        _tasksGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "User", DataPropertyName = "UserName", Width = 180 });
        _tasksGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "UserEmail", Width = 240 });
        _tasksGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Title", DataPropertyName = "Title", Width = 220 });
        _tasksGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "CategoryName", Width = 120 });
        _tasksGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Due Date", DataPropertyName = "DueDate", Width = 120 });
        _tasksGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", Width = 110 });

        usersPage.Controls.Add(_usersGrid);
        tasksPage.Controls.Add(_tasksGrid);
        tabs.TabPages.Add(usersPage);
        tabs.TabPages.Add(tasksPage);

        void LayoutMainArea()
        {
            var w = contentPanel.ClientSize.Width;
            var h = contentPanel.ClientSize.Height;
            topBar.Width = w;
            _btnUserMenu.Left = Math.Max(310, w - _btnUserMenu.Width - 8);
            _txtSearch.Left = Math.Max(260, _btnUserMenu.Left - _txtSearch.Width - 16);
            _userMenuPanel.Left = Math.Max(0, _btnUserMenu.Right - _userMenuPanel.Width);
            _userMenuPanel.Top = topBar.Bottom + 14;
            _userMenuPanel.BringToFront();
            statsPanel.Width = w;
            PositionStatCards(statsPanel, w);
            tabs.SetBounds(0, 300, w, Math.Max(240, h - 300));
        }

        contentPanel.Resize += (_, _) => LayoutMainArea();
        contentPanel.HandleCreated += (_, _) => LayoutMainArea();
        topBar.Controls.AddRange([title, _txtSearch, _btnUserMenu]);
        contentPanel.Controls.AddRange([topBar, _lblStats, statsPanel, tabs, _userMenuPanel]);

        Controls.Add(contentPanel);
        Controls.Add(sidebar);
    }

    private void ConfigureUserMenuButton()
    {
        var initial = string.IsNullOrWhiteSpace(_adminUser.FullName) ? "A" : _adminUser.FullName.Trim()[0].ToString().ToUpperInvariant();
        _btnUserMenu.Text = initial;
        _btnUserMenu.Size = new Size(46, 46);
        _btnUserMenu.Location = new Point(1034, 54);
        _btnUserMenu.FlatStyle = FlatStyle.Flat;
        _btnUserMenu.FlatAppearance.BorderSize = 0;
        _btnUserMenu.BackColor = AppTheme.Primary;
        _btnUserMenu.ForeColor = Color.White;
        _btnUserMenu.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        _btnUserMenu.Cursor = Cursors.Hand;
        _btnUserMenu.Paint += (_, _) =>
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, _btnUserMenu.Width - 1, _btnUserMenu.Height - 1);
            _btnUserMenu.Region = new Region(path);
        };
        _btnUserMenu.Click += (_, _) => _userMenuPanel.Visible = !_userMenuPanel.Visible;
    }

    private void ConfigureUserMenuPanel()
    {
        _userMenuPanel.Size = new Size(190, 110);
        _userMenuPanel.Location = new Point(900, 114);
        _userMenuPanel.BackColor = Color.White;
        _userMenuPanel.BorderStyle = BorderStyle.FixedSingle;
        _userMenuPanel.Visible = false;

        var name = new Label
        {
            Text = $"{_adminUser.FullName} - admin",
            Location = new Point(16, 14),
            Size = new Size(158, 30),
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = AppTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var separator = new Panel { Location = new Point(0, 54), Size = new Size(190, 1), BackColor = AppTheme.Border };
        var logout = new Button
        {
            Text = "Logout",
            Location = new Point(0, 56),
            Size = new Size(188, 52),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 10.5f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        logout.FlatAppearance.BorderSize = 0;
        logout.MouseEnter += (_, _) => logout.BackColor = AppTheme.PrimarySoft;
        logout.MouseLeave += (_, _) => logout.BackColor = Color.White;
        logout.Click += (_, _) => Application.Restart();
        _userMenuPanel.Controls.AddRange([name, separator, logout]);
    }

    private static Panel CreateStatCard(string title, Label valueLabel, Point location, Color background)
    {
        var card = new Panel
        {
            Location = location,
            Size = new Size(200, 92),
            BackColor = background
        };

        var titleLabel = new Label
        {
            Text = title,
            Location = new Point(18, 14),
            AutoSize = true,
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            ForeColor = AppTheme.TextMuted
        };

        valueLabel.Text = "0";
        valueLabel.Location = new Point(18, 38);
        valueLabel.AutoSize = true;
        valueLabel.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
        valueLabel.ForeColor = AppTheme.TextPrimary;

        card.Controls.AddRange([titleLabel, valueLabel]);
        return card;
    }

    private static void PositionStatCards(Panel statsPanel, int availableWidth)
    {
        var cards = statsPanel.Controls.OfType<Panel>().ToList();
        if (cards.Count == 0)
        {
            return;
        }

        var gap = 16;
        var cardWidth = Math.Max(150, (availableWidth - gap * (cards.Count - 1)) / cards.Count);
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].SetBounds(i * (cardWidth + gap), 0, cardWidth, 92);
        }
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        AppTheme.StyleGrid(grid);
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
            ApplyAdminFilters();
        }
        catch (Exception ex)
        {
            Helpers.ShowError($"Could not load admin data: {ex.Message}");
        }
    }

    private UserModel? GetSelectedUser()
    {
        return _usersGrid.CurrentRow?.DataBoundItem as UserModel;
    }

    private void ChangeSelectedUserRole(string role)
    {
        var selected = GetSelectedUser();
        if (selected is null)
        {
            Helpers.ShowInfo("Select a user first.");
            return;
        }

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
        var selected = GetSelectedUser();
        if (selected is null)
        {
            Helpers.ShowInfo("Select a user first.");
            return;
        }

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

        _usersGrid.DataSource = null;
        _usersGrid.DataSource = filteredUsers;
        _tasksGrid.DataSource = null;
        _tasksGrid.DataSource = filteredTasks;
    }
}
