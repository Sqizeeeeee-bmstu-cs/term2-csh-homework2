using Microsoft.Data.Sqlite;

public class DatabaseManager
{
    private string _connectionString;

    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS dep (
                dep_id INTEGER PRIMARY KEY AUTOINCREMENT,
                dep_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS teacher (
                teacher_id INTEGER PRIMARY KEY AUTOINCREMENT,
                dep_id INTEGER NOT NULL,
                teacher_name TEXT NOT NULL,
                publications INTEGER NOT NULL,
                FOREIGN KEY (dep_id) REFERENCES dep(dep_id)
            );";
        cmd.ExecuteNonQuery();
    }

    public List<Department> GetAllDepartments()
    {
        var result = new List<Department>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT dep_id, dep_name FROM dep ORDER BY dep_id";

        
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Department(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }

    public List<Teacher> GetAllTeachers()
    {
        var result = new List<Teacher>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT teacher_id, dep_id, teacher_name, publications FROM teacher ORDER BY teacher_id";
        
        var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Teacher(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3)));
        }

        return result;
    }

    public Teacher GetTeacherById(int id)
    {
        Teacher res = null;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT teacher_id, dep_id, teacher_name, publications FROM teacher WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        var reader = cmd.ExecuteReader();
        
        if (reader.Read())
        {
            res = new Teacher(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)
            );
        }

        return res;
    }


}
