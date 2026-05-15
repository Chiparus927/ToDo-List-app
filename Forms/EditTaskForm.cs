using ToDoListApp.Models;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class EditTaskForm : Form
{
    private readonly TaskModel _task;
    private readonly TextBox _txtTitle = new();
    private readonly TextBox _txtDescription = new();
    private readonly ComboBox _cmbCategory = new();
    private readonly DateTimePicker _dtDueDate = new();
    private readonly CheckBox _chkCompleted = new();

    public TaskModel? UpdatedTask { get; private set; }

    public EditTaskForm(TaskModel task, List<CategoryModel> categories)
    {
        _task = task;
        Text = "Edit Task";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(580, 560);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AppTheme.StyleForm(this, new Size(540, 500));
        InitializeComponents(categories);
    }

    private void InitializeComponents(List<CategoryModel> categories)
    {
        BackColor = AppTheme.Background;
        var card = AppTheme.CreateShadowCard(new Rectangle(28, 24, 508, 462), 26);
        card.BackColor = AppTheme.Surface;

        var title = new Label
        {
            Text = "Edit task",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            AutoSize = true,
            Location = new Point(28, 24)
        };

        _txtTitle.PlaceholderText = "Title";
        _txtTitle.Text = _task.Title;
        var titleRow = AppTheme.CreateTextInputRow(_txtTitle, 452, "●");
        titleRow.Location = new Point(28, 86);

        _txtDescription.PlaceholderText = "Description";
        _txtDescription.Text = _task.Description;
        var descriptionRow = new Panel
        {
            Location = new Point(28, 148),
            Size = new Size(452, 112),
            BackColor = AppTheme.Input,
            Padding = new Padding(16, 14, 16, 12)
        };
        _txtDescription.Multiline = true;
        _txtDescription.BorderStyle = BorderStyle.None;
        _txtDescription.BackColor = AppTheme.Input;
        _txtDescription.ForeColor = AppTheme.TextPrimary;
        _txtDescription.Font = new Font("Segoe UI", 10.5f);
        _txtDescription.Dock = DockStyle.Fill;
        descriptionRow.Controls.Add(_txtDescription);
        descriptionRow.Resize += (_, _) => AppTheme.ApplyRoundedRegion(descriptionRow, 18);
        descriptionRow.HandleCreated += (_, _) => AppTheme.ApplyRoundedRegion(descriptionRow, 18);

        var lblCategory = new Label { Text = "Category", Location = new Point(30, 278), AutoSize = true, ForeColor = AppTheme.TextMuted, Font = new Font("Segoe UI", 9.5f) };
        _cmbCategory.Location = new Point(28, 302);
        _cmbCategory.Width = 218;
        _cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCategory.FlatStyle = FlatStyle.Flat;
        _cmbCategory.BackColor = AppTheme.Input;
        _cmbCategory.Font = new Font("Segoe UI", 10.5f);
        _cmbCategory.DataSource = categories;
        _cmbCategory.DisplayMember = "Name";
        _cmbCategory.ValueMember = "Id";
        _cmbCategory.SelectedValue = _task.CategoryId;

        var lblDueDate = new Label { Text = "Due date", Location = new Point(264, 278), AutoSize = true, ForeColor = AppTheme.TextMuted, Font = new Font("Segoe UI", 9.5f) };
        _dtDueDate.Location = new Point(264, 302);
        _dtDueDate.Width = 216;
        _dtDueDate.Format = DateTimePickerFormat.Short;
        _dtDueDate.Value = _task.DueDate;

        _chkCompleted.Text = "Mark as completed";
        _chkCompleted.Location = new Point(30, 354);
        _chkCompleted.Size = new Size(220, 28);
        _chkCompleted.Checked = _task.IsCompleted;
        _chkCompleted.Font = new Font("Segoe UI", 10.5f);
        _chkCompleted.ForeColor = AppTheme.TextPrimary;

        var btnSave = new Button
        {
            Text = "Update task",
            Location = new Point(28, 400),
            Width = 452,
            Height = 48
        };
        AppTheme.StylePrimaryButton(btnSave, 16);
        btnSave.Click += (_, _) => SaveTask();

        card.Controls.AddRange([title, titleRow, descriptionRow, lblCategory, _cmbCategory, lblDueDate, _dtDueDate, _chkCompleted, btnSave]);
        Controls.Add(card);
    }

    private void SaveTask()
    {
        if (Validator.IsNullOrWhiteSpace(_txtTitle.Text))
        {
            Helpers.ShowError("Title is required.");
            return;
        }

        UpdatedTask = new TaskModel
        {
            Id = _task.Id,
            UserId = _task.UserId,
            Title = _txtTitle.Text.Trim(),
            Description = _txtDescription.Text.Trim(),
            CategoryId = Convert.ToInt32(_cmbCategory.SelectedValue),
            DueDate = _dtDueDate.Value.Date,
            IsCompleted = _chkCompleted.Checked
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
