using MySql.Data.MySqlClient;
using System.Configuration;

namespace ToDoListApp.Database;

public class DbConnection
{
    private static readonly string ConnectionString = GetConnectionString();

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(ConnectionString);
    }

    private static string GetConnectionString()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["TodoListConnection"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Lipseste conexiunea 'TodoListConnection' din App.config.");
        }

        return connectionString;
    }
}
