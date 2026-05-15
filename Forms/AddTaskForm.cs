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
        Size = new Size(560, 520);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AppTheme.StyleForm(this, new Size(520, 480));
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        BackColor = AppTheme.Background;
        var card = AppTheme.CreateShadowCard(new Rectangle(28, 24, 488, 420), 26);
        card.BackColor = AppTheme.Surface;

        var title = new Label
        {
            Text = "New task",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            AutoSize = true,
            Location = new Point(28, 24)
        };

        _txtTitle.PlaceholderText = "Title";
        var titleRow = AppTheme.CreateTextInputRow(_txtTitle, 432, "●");
        titleRow.Location = new Point(28, 86);

        _txtDescription.PlaceholderText = "Description";
        var descriptionRow = new Panel
        {
            Location = new Point(28, 148),
            Size = new Size(432, 112),
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
        _cmbCategory.Width = 208;
        _cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCategory.FlatStyle = FlatStyle.Flat;
        _cmbCategory.BackColor = AppTheme.Input;
        _cmbCategory.Font = new Font("Segoe UI", 10.5f);
        _cmbCategory.DataSource = _categories;
        _cmbCategory.DisplayMember = "Name";
        _cmbCategory.ValueMember = "Id";

        var lblDueDate = new Label { Text = "Due date", Location = new Point(252, 278), AutoSize = true, ForeColor = AppTheme.TextMuted, Font = new Font("Segoe UI", 9.5f) };
        _dtDueDate.Location = new Point(252, 302);
        _dtDueDate.Width = 208;
        _dtDueDate.Format = DateTimePickerFormat.Short;
        _dtDueDate.CalendarForeColor = AppTheme.TextPrimary;
        _dtDueDate.CalendarTitleBackColor = AppTheme.Primary;

        var btnSave = new Button
        {
            Text = "Save task",
            Location = new Point(28, 358),
            Width = 432,
            Height = 48
        };
        AppTheme.StylePrimaryButton(btnSave, 16);
        btnSave.Click += (_, _) => SaveTask();

        card.Controls.AddRange([title, titleRow, descriptionRow, lblCategory, _cmbCategory, lblDueDate, _dtDueDate, btnSave]);
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
