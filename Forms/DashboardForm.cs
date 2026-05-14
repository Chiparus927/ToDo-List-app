using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class DashboardForm : Form
{
    private readonly TaskService _taskService;
    private readonly UserModel _user;
    private readonly DataGridView _grid = new();
    private readonly ComboBox _cmbStatusFilter = new();
    private readonly ComboBox _cmbCategoryFilter = new();
    private readonly TextBox _txtSearch = new();
    private readonly Label _lblStats = new();
    private readonly Panel _emptyState = new();
    private readonly Button _btnUserMenu = new();
    private readonly Panel _userMenuPanel = new();
    private List<CategoryModel> _categories = new();
    private List<TaskModel> _tasks = new();

    public DashboardForm(TaskService taskService, UserModel user)
    {
        _taskService = taskService;
        _user = user;
        Text = $"ToDo List - {_user.FullName}";
        WindowState = FormWindowState.Maximized;
        InitializeComponents();
        LoadData();
    }

    private void InitializeComponents()
    {
        BackColor = Color.White;

        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 350,
            BackColor = AppTheme.Sidebar
        };

        sidebar.Controls.AddRange([
            new Label
            {
                Text = "ToDo List",
                ForeColor = AppTheme.Primary,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(34, 34)
            },
            AppTheme.CreateNavButton("All Tasks", 126, (_, _) => ResetFilters(), true),
            AppTheme.CreateNavButton("Completed", 178, (_, _) => SelectStatus("Completed")),
            AppTheme.CreateNavButton("Active", 224, (_, _) => SelectStatus("Active")),
            AppTheme.CreateNavButton("+ Add task", 292, (_, _) => AddTask(), true),
            AppTheme.CreateNavButton("Edit task", 344, (_, _) => EditTask()),
            AppTheme.CreateNavButton("Delete task", 390, (_, _) => DeleteTask()),
            AppTheme.CreateNavButton("Settings", 490, (_, _) => OpenSettings())
        ]);

        var projectTitle = new Label
        {
            Text = "My Projects",
            Location = new Point(20, 610),
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = AppTheme.TextMuted
        };
        var project = new Label
        {
            Text = "# Personal tasks",
            Location = new Point(28, 646),
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = AppTheme.TextPrimary
        };
        sidebar.Controls.AddRange([projectTitle, project]);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(48, 36, 48, 36),
            BackColor = Color.White
        };

        var headerTitle = new Label
        {
            Text = "All Tasks",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 40),
            ForeColor = AppTheme.TextPrimary
        };

        var topBar = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(1100, 112),
            BackColor = Color.White
        };

        var filterBar = new Panel
        {
            Location = new Point(0, 124),
            Size = new Size(1100, 54),
            BackColor = Color.White
        };

        _txtSearch.PlaceholderText = "Search tasks...";
        _txtSearch.Width = 376;
        _txtSearch.Location = new Point(640, 42);
        AppTheme.StyleComfortSingleLineTextBox(_txtSearch);
        _txtSearch.TextChanged += (_, _) => RefreshGrid();

        ConfigureUserMenuButton();
        ConfigureUserMenuPanel();

        _cmbStatusFilter.Items.AddRange(["All", "Active", "Completed"]);
        _cmbStatusFilter.SelectedIndex = 0;
        _cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbStatusFilter.FlatStyle = FlatStyle.Flat;
        _cmbStatusFilter.BackColor = AppTheme.Input;
        _cmbStatusFilter.Font = new Font("Segoe UI", 10.5f);
        _cmbStatusFilter.Width = 150;
        _cmbStatusFilter.Height = 42;
        _cmbStatusFilter.Location = new Point(0, 8);
        _cmbStatusFilter.SelectedIndexChanged += (_, _) => RefreshGrid();

        _cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCategoryFilter.FlatStyle = FlatStyle.Flat;
        _cmbCategoryFilter.BackColor = AppTheme.Input;
        _cmbCategoryFilter.Font = new Font("Segoe UI", 10.5f);
        _cmbCategoryFilter.Width = 190;
        _cmbCategoryFilter.Height = 42;
        _cmbCategoryFilter.Location = new Point(166, 8);
        _cmbCategoryFilter.SelectedIndexChanged += (_, _) => RefreshGrid();

        _lblStats.Location = new Point(380, 18);
        _lblStats.AutoSize = true;
        _lblStats.Font = new Font("Segoe UI", 10.5f);
        _lblStats.ForeColor = AppTheme.TextMuted;

        var gridHost = new Panel
        {
            Location = new Point(0, 200),
            Size = new Size(900, 420),
            BackColor = Color.White
        };

        _grid.Dock = DockStyle.Fill;
        AppTheme.StyleGrid(_grid);
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Title", DataPropertyName = "Title", Width = 280 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", DataPropertyName = "Description", Width = 340 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "CategoryName", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Due Date", DataPropertyName = "DueDate", Width = 130 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Done", DataPropertyName = "IsCompleted", Name = "Done", Width = 90 });
        _grid.CellClick += (_, e) => ToggleTaskDone(e);

        CreateEmptyState();
        topBar.Controls.AddRange([headerTitle, _txtSearch, _btnUserMenu]);
        filterBar.Controls.AddRange([_cmbStatusFilter, _cmbCategoryFilter, _lblStats]);
        gridHost.Controls.Add(_grid);
        contentPanel.Controls.AddRange([topBar, filterBar, gridHost, _emptyState, _userMenuPanel]);

        void LayoutMainArea()
        {
            var w = contentPanel.ClientSize.Width;
            var h = contentPanel.ClientSize.Height;
            topBar.Width = w;
            filterBar.Width = w;
            _btnUserMenu.Left = Math.Max(310, w - _btnUserMenu.Width - 8);
            _txtSearch.Left = Math.Max(260, _btnUserMenu.Left - _txtSearch.Width - 16);
            _userMenuPanel.Left = Math.Max(0, _btnUserMenu.Right - _userMenuPanel.Width);
            _userMenuPanel.Top = topBar.Bottom + 2;
            _userMenuPanel.BringToFront();
            gridHost.SetBounds(0, 200, w, Math.Max(220, h - 200));
            _emptyState.SetBounds(Math.Max(0, (w - 360) / 2), Math.Max(170, (h - 260) / 2), 360, 220);
        }

        contentPanel.Resize += (_, _) => LayoutMainArea();
        contentPanel.HandleCreated += (_, _) => LayoutMainArea();

        Controls.Add(contentPanel);
        Controls.Add(sidebar);
    }

    private void ConfigureUserMenuButton()
    {
        var initial = string.IsNullOrWhiteSpace(_user.FullName) ? "U" : _user.FullName.Trim()[0].ToString().ToUpperInvariant();
        _btnUserMenu.Text = initial;
        _btnUserMenu.Size = new Size(46, 46);
        _btnUserMenu.Location = new Point(1034, 42);
        _btnUserMenu.FlatStyle = FlatStyle.Flat;
        _btnUserMenu.FlatAppearance.BorderSize = 0;
        _btnUserMenu.BackColor = AppTheme.Primary;
        _btnUserMenu.ForeColor = Color.White;
        _btnUserMenu.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        _btnUserMenu.Cursor = Cursors.Hand;
        _btnUserMenu.Paint += (_, e) =>
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, _btnUserMenu.Width - 1, _btnUserMenu.Height - 1);
            _btnUserMenu.Region = new Region(path);
        };
        _btnUserMenu.Click += (_, _) => _userMenuPanel.Visible = !_userMenuPanel.Visible;
    }

    private void ConfigureUserMenuPanel()
    {
        _userMenuPanel.Size = new Size(180, 110);
        _userMenuPanel.Location = new Point(900, 78);
        _userMenuPanel.BackColor = Color.White;
        _userMenuPanel.BorderStyle = BorderStyle.FixedSingle;
        _userMenuPanel.Visible = false;
        _userMenuPanel.BringToFront();

        var name = new Label
        {
            Text = ShortText(_user.FullName, 22),
            Location = new Point(16, 14),
            Size = new Size(148, 30),
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = AppTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var separator = new Panel
        {
            Location = new Point(0, 54),
            Size = new Size(180, 1),
            BackColor = AppTheme.Border
        };

        var logout = new Button
        {
            Text = "Logout",
            Location = new Point(0, 56),
            Size = new Size(178, 52),
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
        logout.Click += (_, _) => Logout();

        _userMenuPanel.Controls.AddRange([name, separator, logout]);
    }

    private Label CreateAvatar()
    {
        var initial = string.IsNullOrWhiteSpace(_user.FullName) ? "U" : _user.FullName.Trim()[0].ToString().ToUpperInvariant();
        return new Label
        {
            Text = initial,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(82, 183, 172),
            Location = new Point(20, 20),
            Size = new Size(34, 34)
        };
    }

    private void CreateEmptyState()
    {
        _emptyState.BackColor = Color.White;
        _emptyState.Visible = false;

        var icon = new Label
        {
            Text = "[]",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 42, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 202, 82),
            Location = new Point(130, 0),
            Size = new Size(100, 70)
        };
        var title = new Label
        {
            Text = "Capture now, plan later",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(0, 86),
            Size = new Size(360, 28)
        };
        var hint = new Label
        {
            Text = "Inbox is your place for quick task entry. Add a task and organize it when you are ready.",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = AppTheme.TextMuted,
            Location = new Point(20, 120),
            Size = new Size(320, 52)
        };
        var add = new Button
        {
            Text = "+ Add task",
            Location = new Point(112, 184),
            Size = new Size(136, 38)
        };
        AppTheme.StylePrimaryButton(add);
        add.Click += (_, _) => AddTask();
        _emptyState.Controls.AddRange([icon, title, hint, add]);
    }

    private static string ShortText(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
        {
            return text;
        }

        return text[..(maxLength - 3)] + "...";
    }

    private void ResetFilters()
    {
        _txtSearch.Clear();
        _cmbStatusFilter.SelectedIndex = 0;
        if (_cmbCategoryFilter.Items.Count > 0)
        {
            _cmbCategoryFilter.SelectedIndex = 0;
        }
    }

    private void SelectStatus(string status)
    {
        _txtSearch.Clear();
        _cmbStatusFilter.SelectedItem = status;
        if (_cmbCategoryFilter.Items.Count > 0)
        {
            _cmbCategoryFilter.SelectedIndex = 0;
        }
    }

    private void LoadData()
    {
        try
        {
            _categories = _taskService.GetCategories();
            var categoryFilterSource = new List<CategoryModel> { new() { Id = 0, Name = "All Categories" } };
            categoryFilterSource.AddRange(_categories);
            _cmbCategoryFilter.DataSource = categoryFilterSource;
            _cmbCategoryFilter.DisplayMember = "Name";
            _cmbCategoryFilter.ValueMember = "Id";
            RefreshGrid();
        }
        catch (Exception ex)
        {
            Helpers.ShowError($"Could not load data: {ex.Message}");
        }
    }

    private void RefreshGrid()
    {
        var status = _cmbStatusFilter.SelectedItem?.ToString() ?? "All";
        var search = _txtSearch.Text.Trim();
        var selectedCategoryId = Convert.ToInt32(_cmbCategoryFilter.SelectedValue ?? 0);
        int? categoryId = selectedCategoryId == 0 ? null : selectedCategoryId;

        _tasks = _taskService.GetTasks(_user.Id, status, search, categoryId);
        _grid.DataSource = null;
        _grid.DataSource = _tasks;

        var completed = _tasks.Count(t => t.IsCompleted);
        var active = _tasks.Count - completed;
        _lblStats.Text = $"Total: {_tasks.Count} | Active: {active} | Completed: {completed}";
        _emptyState.Visible = _tasks.Count == 0;
        _grid.Visible = _tasks.Count > 0;
    }

    private TaskModel? GetSelectedTask()
    {
        return _grid.CurrentRow?.DataBoundItem as TaskModel;
    }

    private void ToggleTaskDone(DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Done")
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].DataBoundItem is not TaskModel task)
        {
            return;
        }

        task.IsCompleted = !task.IsCompleted;
        _taskService.UpdateTask(task);
        RefreshGrid();
    }

    private void AddTask()
    {
        using var addForm = new AddTaskForm(_categories);
        if (addForm.ShowDialog(this) != DialogResult.OK || addForm.CreatedTask is null)
        {
            return;
        }

        addForm.CreatedTask.UserId = _user.Id;
        _taskService.AddTask(addForm.CreatedTask);
        RefreshGrid();
    }

    private void EditTask()
    {
        var selected = GetSelectedTask();
        if (selected is null)
        {
            Helpers.ShowInfo("Select a task to edit.");
            return;
        }

        using var editForm = new EditTaskForm(selected, _categories);
        if (editForm.ShowDialog(this) != DialogResult.OK || editForm.UpdatedTask is null)
        {
            return;
        }

        _taskService.UpdateTask(editForm.UpdatedTask);
        RefreshGrid();
    }

    private void DeleteTask()
    {
        var selected = GetSelectedTask();
        if (selected is null)
        {
            Helpers.ShowInfo("Select a task to delete.");
            return;
        }

        var confirm = MessageBox.Show("Delete this task?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        _taskService.DeleteTask(selected.Id, _user.Id);
        RefreshGrid();
    }

    private void MarkCompleted()
    {
        var selected = GetSelectedTask();
        if (selected is null)
        {
            Helpers.ShowInfo("Select a task to mark completed.");
            return;
        }

        selected.IsCompleted = true;
        _taskService.UpdateTask(selected);
        RefreshGrid();
    }

    private void OpenSettings()
    {
        using var settings = new SettingsForm();
        settings.ShowDialog(this);
    }

    private void Logout()
    {
        Application.Restart();
    }
}
