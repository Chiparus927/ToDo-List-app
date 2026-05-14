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
        Size = new Size(520, 460);
        InitializeComponents(categories);
    }

    private void InitializeComponents(List<CategoryModel> categories)
    {
        BackColor = AppTheme.Background;
        var card = AppTheme.CreateCard(new Rectangle(18, 18, 470, 390));
        card.BackColor = AppTheme.Background;

        var lblTitle = new Label { Text = "Title", Location = new Point(20, 18), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _txtTitle.Location = new Point(20, 38);
        _txtTitle.Width = 440;
        _txtTitle.Text = _task.Title;
        AppTheme.StyleWebTextBox(_txtTitle);

        var lblDescription = new Label { Text = "Description", Location = new Point(20, 78), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _txtDescription.Location = new Point(20, 98);
        _txtDescription.Width = 440;
        _txtDescription.Height = 100;
        _txtDescription.Multiline = true;
        _txtDescription.Text = _task.Description;
        AppTheme.StyleWebTextBox(_txtDescription);

        var lblCategory = new Label { Text = "Category", Location = new Point(20, 208), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _cmbCategory.Location = new Point(20, 228);
        _cmbCategory.Width = 220;
        _cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCategory.FlatStyle = FlatStyle.Flat;
        _cmbCategory.BackColor = Color.FromArgb(249, 250, 251);
        _cmbCategory.DataSource = categories;
        _cmbCategory.DisplayMember = "Name";
        _cmbCategory.ValueMember = "Id";
        _cmbCategory.SelectedValue = _task.CategoryId;

        var lblDueDate = new Label { Text = "Due date", Location = new Point(260, 208), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _dtDueDate.Location = new Point(260, 228);
        _dtDueDate.Format = DateTimePickerFormat.Short;
        _dtDueDate.Value = _task.DueDate;

        _chkCompleted.Text = "Mark as completed";
        _chkCompleted.Location = new Point(20, 266);
        _chkCompleted.Checked = _task.IsCompleted;

        var btnSave = new Button
        {
            Text = "Update",
            Location = new Point(20, 306),
            Width = 440,
            Height = 38,
            FlatStyle = FlatStyle.Flat
        };
        AppTheme.StylePrimaryButton(btnSave);
        btnSave.Click += (_, _) => SaveTask();

        card.Controls.AddRange([lblTitle, _txtTitle, lblDescription, _txtDescription, lblCategory, _cmbCategory, lblDueDate, _dtDueDate, _chkCompleted, btnSave]);
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
