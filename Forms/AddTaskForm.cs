using ToDoListApp.Models;
using ToDoListApp.Utils;

namespace ToDoListApp.Forms;

public class AddTaskForm : Form
{
    private readonly List<CategoryModel> _categories;
    private readonly TextBox _txtTitle = new();
    private readonly TextBox _txtDescription = new();
    private readonly ComboBox _cmbCategory = new();
    private readonly DateTimePicker _dtDueDate = new();

    public TaskModel? CreatedTask { get; private set; }

    public AddTaskForm(List<CategoryModel> categories)
    {
        _categories = categories;
        Text = "Add Task";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(500, 420);
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        BackColor = AppTheme.Background;
        var card = AppTheme.CreateCard(new Rectangle(18, 18, 448, 350));
        card.BackColor = AppTheme.Background;

        var lblTitle = new Label { Text = "Title", Location = new Point(20, 18), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _txtTitle.Location = new Point(20, 38);
        _txtTitle.Width = 420;
        AppTheme.StyleWebTextBox(_txtTitle);

        var lblDescription = new Label { Text = "Description", Location = new Point(20, 76), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _txtDescription.Location = new Point(20, 96);
        _txtDescription.Width = 420;
        _txtDescription.Height = 100;
        _txtDescription.Multiline = true;
        AppTheme.StyleWebTextBox(_txtDescription);

        var lblCategory = new Label { Text = "Category", Location = new Point(20, 206), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _cmbCategory.Location = new Point(20, 226);
        _cmbCategory.Width = 200;
        _cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCategory.FlatStyle = FlatStyle.Flat;
        _cmbCategory.BackColor = Color.FromArgb(249, 250, 251);
        _cmbCategory.DataSource = _categories;
        _cmbCategory.DisplayMember = "Name";
        _cmbCategory.ValueMember = "Id";

        var lblDueDate = new Label { Text = "Due date", Location = new Point(238, 206), AutoSize = true, ForeColor = AppTheme.TextPrimary };
        _dtDueDate.Location = new Point(238, 226);
        _dtDueDate.Format = DateTimePickerFormat.Short;

        var btnSave = new Button
        {
            Text = "Save",
            Location = new Point(20, 274),
            Width = 420,
            Height = 38,
            FlatStyle = FlatStyle.Flat
        };
        AppTheme.StylePrimaryButton(btnSave);
        btnSave.Click += (_, _) => SaveTask();

        card.Controls.AddRange([lblTitle, _txtTitle, lblDescription, _txtDescription, lblCategory, _cmbCategory, lblDueDate, _dtDueDate, btnSave]);
        Controls.Add(card);
    }

    private void SaveTask()
    {
        if (Validator.IsNullOrWhiteSpace(_txtTitle.Text))
        {
            Helpers.ShowError("Title is required.");
            return;
        }

        CreatedTask = new TaskModel
        {
            Title = _txtTitle.Text.Trim(),
            Description = _txtDescription.Text.Trim(),
            CategoryId = Convert.ToInt32(_cmbCategory.SelectedValue),
            DueDate = _dtDueDate.Value.Date,
            IsCompleted = false
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
