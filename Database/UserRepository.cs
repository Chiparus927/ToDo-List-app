using MySql.Data.MySqlClient;
using ToDoListApp.Models;

namespace ToDoListApp.Database;

public class UserRepository
{
    private readonly DbConnection _dbConnection = new();

    public UserModel? GetByEmail(string email)
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = """
                             SELECT id, nume_prenume, email, role, password_hash, created_at
                             FROM users
                             WHERE email = @email
                             LIMIT 1;
                             """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new UserModel
        {
            Id = reader.GetInt32("id"),
            FullName = GetStringOrEmpty(reader, "nume_prenume"),
            Email = GetStringOrEmpty(reader, "email"),
            Role = GetStringOrDefault(reader, "role", "user"),
            PasswordHash = GetStringOrEmpty(reader, "password_hash"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }

    public List<UserModel> GetAllUsers()
    {
        var users = new List<UserModel>();
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = """
                             SELECT id, nume_prenume, email, role, password_hash, created_at
                             FROM users
                             ORDER BY created_at DESC, id DESC;
                             """;

        using var command = new MySqlCommand(query, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserModel
            {
                Id = reader.GetInt32("id"),
                FullName = GetStringOrEmpty(reader, "nume_prenume"),
                Email = GetStringOrEmpty(reader, "email"),
                Role = GetStringOrDefault(reader, "role", "user"),
                PasswordHash = GetStringOrEmpty(reader, "password_hash"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return users;
    }

    public int CreateUser(UserModel user)
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = """
                             INSERT INTO users (nume_prenume, email, role, password_hash)
                             VALUES (@numePrenume, @email, @role, @passwordHash);
                             SELECT LAST_INSERT_ID();
                             """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@numePrenume", user.FullName);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@role", string.IsNullOrWhiteSpace(user.Role) ? "user" : user.Role);
        command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);

        var id = Convert.ToInt32(command.ExecuteScalar());
        return id;
    }

    public void UpdateRole(int userId, string role)
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = "UPDATE users SET role = @role WHERE id = @id;";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", userId);
        command.Parameters.AddWithValue("@role", role);
        command.ExecuteNonQuery();
    }

    public void DeleteUser(int userId)
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string query = "DELETE FROM users WHERE id = @id;";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", userId);
        command.ExecuteNonQuery();
    }

    private static string GetStringOrEmpty(MySqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static string GetStringOrDefault(MySqlDataReader reader, string columnName, string defaultValue)
    {
        var value = GetStringOrEmpty(reader, columnName);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
