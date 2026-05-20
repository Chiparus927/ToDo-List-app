using ToDoListApp.Controls;
using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class DashboardForm : Form, IThemeAware
{
    private readonly TaskService _taskService;
    private readonly UserModel _user;
    private readonly ComboBox _cmbStatusFilter = new();
    private readonly ComboBox _cmbCategoryFilter = new();
    private readonly TextBox _txtSearch = new();
    private readonly Label _lblTotal = new();
    private readonly Label _lblCompleted = new();
    private readonly Label _lblPending = new();
    private readonly FlowLayoutPanel _taskList = new();
    private readonly Panel _emptyState = new();
    private readonly Button _btnUserMenu = new();
    private readonly Panel _userMenuPanel = new();
    private List<CategoryModel> _categories = new();
    private List<TaskModel> _tasks = new();
    private TaskModel? _selectedTask;

    public DashboardForm(TaskService taskService, UserModel user)
    {
        _taskService = taskService;
        _user = user;
        AppTheme.ApplyUserSettings(new UserSettingsService().Load(user.Id));
        Text = $"ToDo List - {_user.FullName}";
        WindowState = FormWindowState.Maximized;
        AppTheme.StyleForm(this, new Size(1180, 760));
        InitializeComponents();
        LoadData();
    }

    private void InitializeComponents()
    {
        var navBar = CreateTopNav();
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(42, 38, 42, 30),
            BackColor = AppTheme.Background
        };
        contentPanel.Paint += (_, e) => PaintBackground(e.Graphics, contentPanel.ClientRectangle);

        var topBar = new Panel { Location = new Point(28, 0), Size = new Size(1100, 104), BackColor = AppTheme.Background };
        var appLogo = AppTheme.CreateAppLogo(104);
        appLogo.Location = new Point(0, -10);
        var title = new Label
        {
            Text = $"Welcome, {_user.FullName}!",
            Font = new Font("Segoe UI", 27, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(122, 0),
            ForeColor = AppTheme.TextPrimary
        };
        var subtitle = new Label
        {
            Text = "My Tasks",
            Font = new Font("Segoe UI", 10.5f),
            AutoSize = true,
            Location = new Point(126, 64),
            ForeColor = AppTheme.TextMuted
        };

        _txtSearch.PlaceholderText = "Search tasks...";
        var searchRow = AppTheme.CreateSearchBox(_txtSearch, 360);
        searchRow.Location = new Point(560, 18);
        _txtSearch.TextChanged += (_, _) => RefreshTaskCards();

        ConfigureUserMenuButton();
        ConfigureUserMenuPanel();
        topBar.Controls.AddRange([appLogo, title, subtitle, navBar, searchRow, _btnUserMenu]);

        var statsPanel = new Panel { Location = new Point(28, 124), Size = new Size(1100, 112), BackColor = AppTheme.Background };
        statsPanel.Controls.AddRange([
            CreateStatCard(_lblPending, "Folder", Color.FromArgb(238, 250, 243), AppTheme.Success, new Point(0, 0)),
            CreateStatCard(_lblCompleted, "Check", Color.FromArgb(255, 248, 232), AppTheme.Warning, new Point(236, 0)),
            CreateStatCard(_lblTotal, "List", Color.FromArgb(238, 246, 255), AppTheme.Primary, new Point(472, 0))
        ]);

        _cmbStatusFilter.Items.AddRange(["All", "Active", "Completed"]);
        _cmbStatusFilter.SelectedIndex = 0;
        StyleCombo(_cmbStatusFilter, 150);
        _cmbStatusFilter.Location = new Point(0, 12);
        _cmbStatusFilter.SelectedIndexChanged += (_, _) => RefreshTaskCards();

        StyleCombo(_cmbCategoryFilter, 210);
        _cmbCategoryFilter.Location = new Point(166, 12);
        _cmbCategoryFilter.SelectedIndexChanged += (_, _) => RefreshTaskCards();

        var filterBar = new Panel { Location = new Point(28, 260), Size = new Size(1100, 66), BackColor = AppTheme.Background };
        var filterPanel = new RoundedPanel
        {
            Location = new Point(0, 4),
            Size = new Size(282, 52),
            Radius = 20,
            DrawShadow = false,
            BackColor = AppTheme.Background,
            BorderColor = AppTheme.Background,
            Padding = new Padding(6)
        };
        _cmbCategoryFilter.Location = new Point(16, 10);
        _cmbCategoryFilter.Width = 250;
        _cmbCategoryFilter.BackColor = AppTheme.Input;
        _cmbCategoryFilter.ForeColor = AppTheme.TextPrimary;
        filterPanel.Controls.Add(_cmbCategoryFilter);
        filterBar.Controls.Add(filterPanel);

        var listHost = new RoundedPanel
        {
            Location = new Point(28, 342),
            Size = new Size(1100, 420),
            Padding = new Padding(18),
            Radius = 26,
            BackColor = AppTheme.ButtonNeutral,
            BorderColor = AppTheme.Border
        };

        _taskList.Dock = DockStyle.Fill;
        _taskList.AutoScroll = true;
        _taskList.FlowDirection = FlowDirection.TopDown;
        _taskList.WrapContents = false;
        _taskList.BackColor = AppTheme.Surface;
        listHost.Controls.Add(_taskList);

        CreateEmptyState();
        contentPanel.Controls.AddRange([topBar, statsPanel, filterBar, listHost, _emptyState, _userMenuPanel]);

        void LayoutMainArea()
        {
            const int leftGap = 28;
            const int rightGap = 20;
            var w = Math.Max(520, contentPanel.ClientSize.Width - leftGap - rightGap);
            var h = contentPanel.ClientSize.Height;
            topBar.SetBounds(leftGap, 0, w, 104);
            appLogo.BackColor = topBar.BackColor;
            _btnUserMenu.Left = Math.Max(320, w - _btnUserMenu.Width - 2);
            searchRow.Left = Math.Max(300, _btnUserMenu.Left - searchRow.Width - 16);
            navBar.Left = title.Right + 28;
            navBar.Top = 18;
            _userMenuPanel.Left = leftGap + Math.Max(0, _btnUserMenu.Right - _userMenuPanel.Width);
            _userMenuPanel.Top = topBar.Bottom - 4;
            statsPanel.SetBounds(leftGap, 124, w, 112);
            PositionCards(statsPanel, w);
            filterBar.SetBounds(leftGap, 260, w, 66);
            listHost.SetBounds(leftGap, 342, w, Math.Max(260, h - 342));
            ResizeTaskCards();
            _emptyState.SetBounds(leftGap + Math.Max(0, (w - 390) / 2), Math.Max(350, (h - 230) / 2), 390, 230);
            _userMenuPanel.BringToFront();
        }

        contentPanel.Resize += (_, _) => LayoutMainArea();
        contentPanel.HandleCreated += (_, _) => LayoutMainArea();
        Controls.Add(contentPanel);
    }

    private RoundedPanel CreateTopNav()
    {
        var nav = new RoundedPanel
        {
            Size = new Size(488, 56),
            Radius = 24,
            DrawShadow = false,
            BackColor = AppTheme.Sidebar,
            BorderColor = AppTheme.Border
        };
        nav.Controls.AddRange([
            AppTheme.CreateNavButton("All", 5, (_, _) => ResetFilters(), true, "All tasks"),
            AppTheme.CreateNavButton("Active", 5, (_, _) => SelectStatus("Active"), false, "Active"),
            AppTheme.CreateNavButton("Done", 5, (_, _) => SelectStatus("Completed"), false, "Completed"),
            AppTheme.CreateNavButton("Settings", 5, (_, _) => OpenSettings(), false, "Settings"),
            AppTheme.CreateNavButton("Add", 5, (_, _) => AddTask(), true, "Add task"),
            AppTheme.CreateNavButton("Edit", 5, (_, _) => EditTask(), false, "Edit task"),
            AppTheme.CreateNavButton("Delete", 5, (_, _) => DeleteTask(), false, "Delete task")
        ]);

        var left = 18;
        foreach (Button button in nav.Controls.OfType<Button>())
        {
            var width = button.Text switch
            {
                "Settings" => 92,
                "Delete" => 78,
                "Active" => 78,
                _ => 64
            };
            width = button.Text switch
            {
                "Settings" => 72,
                "Delete" => 62,
                "Active" => 60,
                "Done" => 58,
                _ => 48
            };
            button.SetBounds(left, 8, width, 40);
            button.Font = new Font("Segoe UI Semibold", 8.8f, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Padding = Padding.Empty;
            left += width + 7;
        }

        return nav;
    }

    private static RoundedPanel CreateStatCard(Label valueLabel, string label, Color background, Color accent, Point location)
    {
        var card = new RoundedPanel
        {
            Location = location,
            Size = new Size(220, 104),
            Radius = 24,
            BackColor = AppTheme.IsDarkMode ? AppTheme.SoftSurface : background,
            BorderColor = AppTheme.Border
        };
        var mini = new BadgeLabel
        {
            Text = label,
            Location = new Point(18, 18),
            Width = 70,
            BackColor = AppTheme.IsDarkMode ? AppTheme.Input : Color.White,
            ForeColor = accent
        };
        valueLabel.Text = "0";
        valueLabel.Location = new Point(96, 34);
        valueLabel.AutoSize = true;
        valueLabel.Font = new Font("Segoe UI", 30f, FontStyle.Bold);
        valueLabel.ForeColor = accent;
        card.Controls.AddRange([mini, valueLabel]);
        return card;
    }

    private static Button CreateFilterPill(string text, Point location, int width, EventHandler click)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(width, 40),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => button.BackColor = AppTheme.ButtonNeutralHover;
        button.MouseLeave += (_, _) =>
        {
            button.BackColor = AppTheme.ButtonNeutral;
            button.ForeColor = AppTheme.TextPrimary;
        };
        button.Resize += (_, _) => AppTheme.ApplyRoundedRegion(button, 15);
        button.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(button, 15);
        button.Click += click;
        return button;
    }

    private static void PositionCards(Panel statsPanel, int availableWidth)
    {
        var cards = statsPanel.Controls.OfType<Panel>().ToList();
        var gap = 18;
        var cardWidth = Math.Max(196, Math.Min(270, (availableWidth - gap * 2) / 3));
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].SetBounds(i * (cardWidth + gap), 0, cardWidth, 104);
        }
    }

    private static void StyleCombo(ComboBox combo, int width)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.FlatStyle = FlatStyle.Flat;
        combo.BackColor = AppTheme.Input;
        combo.ForeColor = AppTheme.TextPrimary;
        combo.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        combo.Width = width;
        combo.Height = 34;
    }

    private void ConfigureUserMenuButton()
    {
        _btnUserMenu.Size = new Size(54, 54);
        _btnUserMenu.Location = new Point(1028, 15);
        _btnUserMenu.FlatStyle = FlatStyle.Flat;
        _btnUserMenu.UseVisualStyleBackColor = false;
        _btnUserMenu.FlatAppearance.BorderSize = 0;
        _btnUserMenu.BackColor = AppTheme.Primary;
        _btnUserMenu.ForeColor = Color.White;
        _btnUserMenu.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        _btnUserMenu.Cursor = Cursors.Hand;
        _btnUserMenu.Resize += (_, _) => AppTheme.ApplyRoundedRegion(_btnUserMenu, 27);
        _btnUserMenu.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(_btnUserMenu, 27);
        _btnUserMenu.Click += (_, _) => _userMenuPanel.Visible = !_userMenuPanel.Visible;
        RefreshUserMenuButton();
    }

    private void ConfigureUserMenuPanel()
    {
        _userMenuPanel.Size = new Size(210, 116);
        _userMenuPanel.BackColor = AppTheme.Surface;
        AppTheme.ApplyCardChrome(_userMenuPanel, 18);
        _userMenuPanel.Visible = false;

        var name = new Label
        {
            Text = ShortText(_user.FullName, 24),
            Location = new Point(16, 14),
            Size = new Size(178, 30),
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var logout = new Button
        {
            Text = "Logout",
            Location = new Point(12, 58),
            Size = new Size(186, 42),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            BackColor = AppTheme.ButtonNeutral,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 10.5f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        logout.FlatAppearance.BorderSize = 0;
        logout.MouseEnter += (_, _) => logout.BackColor = AppTheme.ButtonNeutralHover;
        logout.MouseLeave += (_, _) =>
        {
            logout.BackColor = AppTheme.ButtonNeutral;
            logout.ForeColor = AppTheme.TextPrimary;
        };
        logout.Click += (_, _) => Logout();
        logout.Resize += (_, _) => AppTheme.ApplyRoundedRegion(logout, 13);
        logout.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(logout, 13);
        _userMenuPanel.Controls.AddRange([name, logout]);
    }

    private void CreateEmptyState()
    {
        _emptyState.BackColor = AppTheme.Surface;
        _emptyState.Visible = false;
        AppTheme.ApplyCardChrome(_emptyState, 24);

        var title = new Label
        {
            Text = "Nothing here yet",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(0, 44),
            Size = new Size(390, 34)
        };
        var hint = new Label
        {
            Text = "Add a task and it will appear in this workspace.",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = AppTheme.TextMuted,
            Location = new Point(36, 88),
            Size = new Size(318, 44)
        };
        var add = new GradientButton { Text = "+ Add task", Location = new Point(124, 154), Size = new Size(142, 44) };
        add.Click += (_, _) => AddTask();
        _emptyState.Controls.AddRange([title, hint, add]);
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
            RefreshTaskCards();
        }
        catch (Exception ex)
        {
            Helpers.ShowError($"Could not load data: {ex.Message}");
        }
    }

    private void RefreshTaskCards()
    {
        if (_cmbCategoryFilter.DataSource is null)
        {
            return;
        }

        var status = _cmbStatusFilter.SelectedItem?.ToString() ?? "All";
        var search = _txtSearch.Text.Trim();
        var selectedCategoryId = Convert.ToInt32(_cmbCategoryFilter.SelectedValue ?? 0);
        int? categoryId = selectedCategoryId == 0 ? null : selectedCategoryId;

        _tasks = _taskService.GetTasks(_user.Id, status, search, categoryId);
        _selectedTask = null;
        _taskList.SuspendLayout();
        _taskList.Controls.Clear();
        foreach (var task in _tasks)
        {
            var card = new TaskCardControl(task);
            card.SelectedTask += (_, selected) => SelectTask(selected);
            card.ToggleCompleted += (_, selected) => ToggleTaskDone(selected);
            _taskList.Controls.Add(card);
        }
        ResizeTaskCards();
        _taskList.ResumeLayout();

        var completed = _tasks.Count(t => t.IsCompleted);
        var active = _tasks.Count - completed;
        _lblTotal.Text = _tasks.Count.ToString();
        _lblCompleted.Text = completed.ToString();
        _lblPending.Text = active.ToString();
        _emptyState.Visible = _tasks.Count == 0;
        _taskList.Visible = _tasks.Count > 0;
    }

    public void ApplyTheme()
    {
        BackColor = AppTheme.Background;
        _taskList.BackColor = AppTheme.Surface;
        _emptyState.BackColor = AppTheme.Surface;
        _userMenuPanel.BackColor = AppTheme.Surface;
        _cmbCategoryFilter.BackColor = AppTheme.Input;
        _cmbCategoryFilter.ForeColor = AppTheme.TextPrimary;
        RestyleStatCards(this);
        RefreshTaskCards();
        RefreshUserMenuButton();
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
                    var accent = badge.Text switch
                    {
                        "Folder" => AppTheme.Success,
                        "Check" => AppTheme.Warning,
                        "List" => AppTheme.Primary,
                        _ => AppTheme.Primary
                    };
                    var background = badge.Text switch
                    {
                        "Folder" => Color.FromArgb(238, 250, 243),
                        "Check" => Color.FromArgb(255, 248, 232),
                        "List" => Color.FromArgb(238, 246, 255),
                        _ => AppTheme.Surface
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

    private void ResizeTaskCards()
    {
        var width = Math.Max(360, _taskList.ClientSize.Width - 24);
        foreach (TaskCardControl card in _taskList.Controls.OfType<TaskCardControl>())
        {
            card.Width = width;
        }
    }

    private void SelectTask(TaskModel task)
    {
        _selectedTask = task;
        foreach (TaskCardControl card in _taskList.Controls.OfType<TaskCardControl>())
        {
            card.SetSelected(card.TaskId == task.Id);
        }
    }

    private void ToggleTaskDone(TaskModel task)
    {
        task.IsCompleted = !task.IsCompleted;
        _taskService.UpdateTask(task);
        RefreshTaskCards();
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

    private void AddTask()
    {
        using var addForm = new AddTaskForm(_categories);
        if (addForm.ShowDialog(this) != DialogResult.OK || addForm.CreatedTask is null)
        {
            return;
        }

        addForm.CreatedTask.UserId = _user.Id;
        _taskService.AddTask(addForm.CreatedTask);
        RefreshTaskCards();
    }

    private void EditTask()
    {
        if (_selectedTask is null)
        {
            Helpers.ShowInfo("Select a task to edit.");
            return;
        }

        using var editForm = new EditTaskForm(_selectedTask, _categories);
        if (editForm.ShowDialog(this) != DialogResult.OK || editForm.UpdatedTask is null)
        {
            return;
        }

        _taskService.UpdateTask(editForm.UpdatedTask);
        RefreshTaskCards();
    }

    private void DeleteTask()
    {
        if (_selectedTask is null)
        {
            Helpers.ShowInfo("Select a task to delete.");
            return;
        }

        var confirm = MessageBox.Show("Delete this task?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        _taskService.DeleteTask(_selectedTask.Id, _user.Id);
        RefreshTaskCards();
    }

    private void OpenSettings()
    {
        using var settings = new SettingsForm(_user);
        settings.ShowDialog(this);
        RefreshUserMenuButton();
    }

    private void RefreshUserMenuButton()
    {
        var settings = new UserSettingsService().Load(_user.Id);
        if (_btnUserMenu.Image is not null)
        {
            _btnUserMenu.Image.Dispose();
            _btnUserMenu.Image = null;
        }

        if (!string.IsNullOrWhiteSpace(settings.ProfileImagePath) && File.Exists(settings.ProfileImagePath))
        {
            _btnUserMenu.BackgroundImage?.Dispose();
            _btnUserMenu.Text = string.Empty;
            _btnUserMenu.BackgroundImage = LoadSquareImage(settings.ProfileImagePath, _btnUserMenu.Width);
            _btnUserMenu.BackgroundImageLayout = ImageLayout.Stretch;
            _btnUserMenu.BackColor = AppTheme.PrimarySoft;
            return;
        }

        _btnUserMenu.BackgroundImage?.Dispose();
        _btnUserMenu.BackgroundImage = null;
        _btnUserMenu.Text = string.IsNullOrWhiteSpace(_user.FullName) ? "U" : _user.FullName.Trim()[0].ToString().ToUpperInvariant();
        _btnUserMenu.BackColor = AppTheme.Primary;
        _btnUserMenu.ForeColor = Color.White;
    }

    private static Image LoadSquareImage(string path, int size)
    {
        using var image = Image.FromFile(path);
        var sourceSize = Math.Min(image.Width, image.Height);
        var sourceX = (image.Width - sourceSize) / 2;
        var sourceY = (image.Height - sourceSize) / 2;
        var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.DrawImage(
            image,
            new Rectangle(0, 0, size, size),
            new Rectangle(sourceX, sourceY, sourceSize, sourceSize),
            GraphicsUnit.Pixel);
        return bitmap;
    }

    private void Logout()
    {
        Application.Restart();
    }

    private static string ShortText(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
        {
            return text;
        }

        return text[..(maxLength - 3)] + "...";
    }

    private static void PaintBackground(Graphics graphics, Rectangle bounds)
    {
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, AppTheme.Background, AppTheme.Background, 90f);
        graphics.FillRectangle(brush, bounds);
    }
}
