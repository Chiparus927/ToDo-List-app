using MySql.Data.MySqlClient;
using ToDoListApp.Models;

namespace ToDoListApp.Database;

public class TaskRepository
{
    private readonly DbConnection _dbConnection = new();

    public List<CategoryModel> GetCategories()
    {
        var categories = new List<CategoryModel>();
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = "SELECT id, name FROM categories ORDER BY name;";
        using var command = new MySqlCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new CategoryModel
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name")
            });
        }

        return categories;
    }

    public List<AdminTaskModel> GetAllTasksForAdmin()
    {
        var tasks = new List<AdminTaskModel>();
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = """
                             SELECT t.id, u.nume_prenume, u.email, c.name AS category_name,
                                    t.title, t.description, t.due_date, t.is_completed, t.created_at
                             FROM tasks t
                             INNER JOIN users u ON u.id = t.user_id
                             INNER JOIN categories c ON c.id = t.category_id
                             ORDER BY t.created_at DESC, t.due_date ASC;
                             """;

        using var command = new MySqlCommand(query, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(new AdminTaskModel
            {
                Id = reader.GetInt32("id"),
                UserName = GetStringOrEmpty(reader, "nume_prenume"),
                UserEmail = GetStringOrEmpty(reader, "email"),
                CategoryName = GetStringOrEmpty(reader, "category_name"),
                Title = GetStringOrEmpty(reader, "title"),
                Description = GetStringOrEmpty(reader, "description"),
                DueDate = reader.GetDateTime("due_date"),
                IsCompleted = reader.GetBoolean("is_completed"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return tasks;
    }

    public List<TaskModel> GetTasks(int userId, string? statusFilter = null, string? search = null, int? categoryId = null)
    {
        var tasks = new List<TaskModel>();
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        var query = """
                    SELECT t.id, t.user_id, t.category_id, c.name AS category_name, t.title, t.description, t.due_date, t.is_completed, t.created_at
                    FROM tasks t
                    INNER JOIN categories c ON c.id = t.category_id
                    WHERE t.user_id = @userId
                    """;

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            if (statusFilter.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                query += " AND t.is_completed = 0";
            }
            else if (statusFilter.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                query += " AND t.is_completed = 1";
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query += " AND (t.title LIKE @search OR t.description LIKE @search)";
        }

        if (categoryId.HasValue)
        {
            query += " AND t.category_id = @categoryId";
        }

        query += " ORDER BY t.due_date ASC, t.created_at DESC;";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.AddWithValue("@search", $"%{search}%");
        }

        if (categoryId.HasValue)
        {
            command.Parameters.AddWithValue("@categoryId", categoryId.Value);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(new TaskModel
            {
                Id = reader.GetInt32("id"),
                UserId = reader.GetInt32("user_id"),
                CategoryId = reader.GetInt32("category_id"),
                CategoryName = GetStringOrEmpty(reader, "category_name"),
                Title = GetStringOrEmpty(reader, "title"),
                Description = GetStringOrEmpty(reader, "description"),
                DueDate = reader.GetDateTime("due_date"),
                IsCompleted = reader.GetBoolean("is_completed"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return tasks;
    }

    public int AddTask(TaskModel task)
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = """
                             INSERT INTO tasks (user_id, category_id, title, description, due_date, is_completed)
                             VALUES (@userId, @categoryId, @title, @description, @dueDate, @isCompleted);
                             SELECT LAST_INSERT_ID();
                             """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", task.UserId);
        command.Parameters.AddWithValue("@categoryId", task.CategoryId);
        command.Parameters.AddWithValue("@title", task.Title);
        command.Parameters.AddWithValue("@description", task.Description);
        command.Parameters.AddWithValue("@dueDate", task.DueDate);
        command.Parameters.AddWithValue("@isCompleted", task.IsCompleted);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateTask(TaskModel task)
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = """
                             UPDATE tasks
                             SET category_id = @categoryId,
                                 title = @title,
                                 description = @description,
                                 due_date = @dueDate,
                                 is_completed = @isCompleted
                             WHERE id = @id AND user_id = @userId;
                             """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", task.Id);
        command.Parameters.AddWithValue("@userId", task.UserId);
        command.Parameters.AddWithValue("@categoryId", task.CategoryId);
        command.Parameters.AddWithValue("@title", task.Title);
        command.Parameters.AddWithValue("@description", task.Description);
        command.Parameters.AddWithValue("@dueDate", task.DueDate);
        command.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
        command.ExecuteNonQuery();
    }

    public void DeleteTask(int taskId, int userId)
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = "DELETE FROM tasks WHERE id = @id AND user_id = @userId;";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", taskId);
        command.Parameters.AddWithValue("@userId", userId);
        command.ExecuteNonQuery();
    }

    private static string GetStringOrEmpty(MySqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
}
