using MySql.Data.MySqlClient;

namespace ToDoListApp.Database;

public class DatabaseInitializer
{
    private readonly DbConnection _dbConnection = new();

    public void EnsureSchema()
    {
        using var connection = _dbConnection.CreateConnection();
        connection.Open();

        const string schemaSql = """
                                 CREATE TABLE IF NOT EXISTS users (
                                     id INT AUTO_INCREMENT PRIMARY KEY,
                                     nume_prenume VARCHAR(150) NOT NULL,
                                     email VARCHAR(150) NOT NULL UNIQUE,
                                     role VARCHAR(20) NOT NULL DEFAULT 'user',
                                     password_hash VARCHAR(255) NOT NULL,
                                     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                 );

                                 CREATE TABLE IF NOT EXISTS categories (
                                     id INT AUTO_INCREMENT PRIMARY KEY,
                                     name VARCHAR(50) NOT NULL UNIQUE
                                 );

                                 CREATE TABLE IF NOT EXISTS tasks (
                                     id INT AUTO_INCREMENT PRIMARY KEY,
                                     user_id INT NOT NULL,
                                     category_id INT NOT NULL,
                                     title VARCHAR(200) NOT NULL,
                                     description TEXT,
                                     due_date DATE NOT NULL,
                                     is_completed TINYINT(1) DEFAULT 0,
                                     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                     CONSTRAINT fk_tasks_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                                     CONSTRAINT fk_tasks_category FOREIGN KEY (category_id) REFERENCES categories(id)
                                 );
                                 """;

        using (var command = new MySqlCommand(schemaSql, connection))
        {
            command.ExecuteNonQuery();
        }

        EnsureUserColumns(connection);

        const string seedCategoriesSql = """
                                         INSERT INTO categories (name)
                                         VALUES ('Work'), ('Personal'), ('School'), ('Other')
                                         ON DUPLICATE KEY UPDATE name = VALUES(name);
                                         """;

        using var seedCommand = new MySqlCommand(seedCategoriesSql, connection);
        seedCommand.ExecuteNonQuery();
    }

    private static void EnsureUserColumns(MySqlConnection connection)
    {
        if (!ColumnExists(connection, "users", "email"))
        {
            const string addEmailColumn = """
                                          ALTER TABLE users
                                          ADD COLUMN email VARCHAR(150) NULL;
                                          """;
            using var addEmailCommand = new MySqlCommand(addEmailColumn, connection);
            addEmailCommand.ExecuteNonQuery();
        }

        if (!IndexExists(connection, "users", "idx_users_email"))
        {
            try
            {
                const string createEmailUniqueIndex = """
                                                      CREATE UNIQUE INDEX idx_users_email
                                                      ON users (email);
                                                      """;
                using var indexCommand = new MySqlCommand(createEmailUniqueIndex, connection);
                indexCommand.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
                // Ignore if duplicate index or incompatible rows.
            }
        }

        EnsureNumePrenumeColumn(connection);
        EnsureRoleColumn(connection);
        EnsureDefaultAdmin(connection);
        DropFullNameColumn(connection);
    }

    private const int DefaultAdminUserId = 4;

    private static void EnsureRoleColumn(MySqlConnection connection)
    {
        if (!ColumnExists(connection, "users", "role"))
        {
            const string addRoleColumn = """
                                         ALTER TABLE users
                                         ADD COLUMN role VARCHAR(20) NOT NULL DEFAULT 'user';
                                         """;
            using var addRoleCommand = new MySqlCommand(addRoleColumn, connection);
            addRoleCommand.ExecuteNonQuery();
        }

        const string normalizeRoles = """
                                      UPDATE users
                                      SET role = 'user'
                                      WHERE role IS NULL OR TRIM(role) = '' OR role NOT IN ('admin', 'user');
                                      """;
        using (var normalizeCommand = new MySqlCommand(normalizeRoles, connection))
        {
            normalizeCommand.ExecuteNonQuery();
        }

    }

    private static void EnsureDefaultAdmin(MySqlConnection connection)
    {
        const string resetOtherAdmins = """
                                        UPDATE users
                                        SET role = 'user'
                                        WHERE id <> @defaultAdminUserId
                                          AND role = 'admin';
                                        """;
        using (var resetOtherAdminsCommand = new MySqlCommand(resetOtherAdmins, connection))
        {
            resetOtherAdminsCommand.Parameters.AddWithValue("@defaultAdminUserId", DefaultAdminUserId);
            resetOtherAdminsCommand.ExecuteNonQuery();
        }

        const string setDefaultAdmin = """
                                       UPDATE users
                                       SET role = 'admin'
                                       WHERE id = @defaultAdminUserId;
                                       """;
        using var setDefaultAdminCommand = new MySqlCommand(setDefaultAdmin, connection);
        setDefaultAdminCommand.Parameters.AddWithValue("@defaultAdminUserId", DefaultAdminUserId);
        setDefaultAdminCommand.ExecuteNonQuery();

        const string promoteFirstUserIfDefaultMissing = """
                                                        UPDATE users
                                                        SET role = 'admin'
                                                        WHERE NOT EXISTS (
                                                            SELECT 1
                                                            FROM (SELECT id FROM users WHERE id = @defaultAdminUserId LIMIT 1) AS default_admin
                                                        )
                                                          AND NOT EXISTS (
                                                            SELECT 1
                                                            FROM (SELECT id FROM users WHERE role = 'admin' LIMIT 1) AS existing_admin
                                                        )
                                                        ORDER BY id ASC
                                                        LIMIT 1;
                                                        """;
        using var promoteCommand = new MySqlCommand(promoteFirstUserIfDefaultMissing, connection);
        promoteCommand.Parameters.AddWithValue("@defaultAdminUserId", DefaultAdminUserId);
        promoteCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Stores the user's display name in nume_prenume. Legacy columns are migrated into it.
    /// </summary>
    private static void EnsureNumePrenumeColumn(MySqlConnection connection)
    {
        if (ColumnExists(connection, "users", "nume_prenume"))
        {
            SyncNumePrenumeFromLegacyColumns(connection);
            return;
        }

        if (ColumnExists(connection, "users", "username"))
        {
            DropUniqueIndexesOnColumn(connection, "users", "username");

            const string renameColumn = """
                                        ALTER TABLE users CHANGE COLUMN username nume_prenume VARCHAR(150) NOT NULL DEFAULT '';
                                        """;
            using (var renameCmd = new MySqlCommand(renameColumn, connection))
            {
                renameCmd.ExecuteNonQuery();
            }
        }
        else
        {
            const string addColumn = """
                                     ALTER TABLE users
                                     ADD COLUMN nume_prenume VARCHAR(150) NOT NULL DEFAULT '';
                                     """;
            using var addCmd = new MySqlCommand(addColumn, connection);
            addCmd.ExecuteNonQuery();
        }

        SyncNumePrenumeFromLegacyColumns(connection);
    }

    private static void SyncNumePrenumeFromLegacyColumns(MySqlConnection connection)
    {
        if (ColumnExists(connection, "users", "full_name"))
        {
            const string syncFromFullName = """
                                            UPDATE users
                                            SET nume_prenume = full_name
                                            WHERE (nume_prenume IS NULL OR TRIM(nume_prenume) = '' OR nume_prenume LIKE '%@%')
                                              AND full_name IS NOT NULL
                                              AND TRIM(full_name) <> '';
                                            """;
            using var fullNameCmd = new MySqlCommand(syncFromFullName, connection);
            fullNameCmd.ExecuteNonQuery();
        }

        if (ColumnExists(connection, "users", "username"))
        {
            const string syncFromUsername = """
                                            UPDATE users
                                            SET nume_prenume = username
                                            WHERE (nume_prenume IS NULL OR TRIM(nume_prenume) = '' OR nume_prenume LIKE '%@%')
                                              AND username IS NOT NULL
                                              AND TRIM(username) <> ''
                                              AND username NOT LIKE '%@%';
                                            """;
            using var usernameCmd = new MySqlCommand(syncFromUsername, connection);
            usernameCmd.ExecuteNonQuery();
        }

        const string fallbackEmptyNames = """
                                          UPDATE users
                                          SET nume_prenume = email
                                          WHERE nume_prenume IS NULL OR TRIM(nume_prenume) = '';
                                          """;
        using var fallbackCmd = new MySqlCommand(fallbackEmptyNames, connection);
        fallbackCmd.ExecuteNonQuery();
    }

    private static void DropFullNameColumn(MySqlConnection connection)
    {
        if (!ColumnExists(connection, "users", "full_name"))
        {
            return;
        }

        const string dropColumn = """
                                  ALTER TABLE users
                                  DROP COLUMN full_name;
                                  """;
        using var dropCmd = new MySqlCommand(dropColumn, connection);
        dropCmd.ExecuteNonQuery();
    }

    private static void DropUniqueIndexesOnColumn(MySqlConnection connection, string tableName, string columnName)
    {
        const string query = """
                             SELECT DISTINCT INDEX_NAME
                             FROM information_schema.STATISTICS
                             WHERE TABLE_SCHEMA = DATABASE()
                               AND TABLE_NAME = @tableName
                               AND COLUMN_NAME = @columnName
                               AND NON_UNIQUE = 0
                               AND INDEX_NAME <> 'PRIMARY';
                             """;

        using var listCmd = new MySqlCommand(query, connection);
        listCmd.Parameters.AddWithValue("@tableName", tableName);
        listCmd.Parameters.AddWithValue("@columnName", columnName);

        var indexNames = new List<string>();
        using (var reader = listCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                indexNames.Add(reader.GetString(0));
            }
        }

        foreach (var indexName in indexNames)
        {
            try
            {
                using var dropCmd = new MySqlCommand(
                    $"ALTER TABLE `{tableName}` DROP INDEX `{indexName}`;",
                    connection);
                dropCmd.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
                // Ignore missing index name variations.
            }
        }
    }

    private static bool ColumnExists(MySqlConnection connection, string tableName, string columnName)
    {
        const string query = """
                             SELECT COUNT(*)
                             FROM information_schema.columns
                             WHERE table_schema = DATABASE()
                               AND table_name = @tableName
                               AND column_name = @columnName;
                             """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        command.Parameters.AddWithValue("@columnName", columnName);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static bool IndexExists(MySqlConnection connection, string tableName, string indexName)
    {
        const string query = """
                             SELECT COUNT(*)
                             FROM information_schema.statistics
                             WHERE table_schema = DATABASE()
                               AND table_name = @tableName
                               AND index_name = @indexName;
                             """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        command.Parameters.AddWithValue("@indexName", indexName);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
