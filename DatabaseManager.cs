using System.IO;
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
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
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

    public void AddTeacher(Teacher teacher)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO teacher (dep_id, teacher_name, publications) VALUES (@depId, @name, @publications)";

        cmd.Parameters.AddWithValue("@depId", teacher.DepartmentId);
        cmd.Parameters.AddWithValue("@name", teacher.Name);
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);

        cmd.ExecuteNonQuery();
    }

    public void UpdateTeacher(Teacher teacher)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE teacher SET dep_id = @depId, teacher_name = @name, publications = @publications WHERE teacher_id = @id";

    
        cmd.Parameters.AddWithValue("@depId", teacher.DepartmentId);
        cmd.Parameters.AddWithValue("@name", teacher.Name);
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);
        cmd.Parameters.AddWithValue("@id", teacher.Id);

        cmd.ExecuteNonQuery();
    }

    public void DeleteTeacher(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM teacher WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        cmd.ExecuteNonQuery();

    }

    private void ImportTeachersFromCsv(string path)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        string[] lines = File.ReadAllLines(path);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 3) continue;
            
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO teacher (dep_id, teacher_name, publications) VALUES (@depId, @name, @publications)";
            cmd.Parameters.AddWithValue("@depId", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.Parameters.AddWithValue("@publications", int.Parse(parts[2]));
            
            cmd.ExecuteNonQuery();
        }
    }

    private void ImportDepartmentsFromCsv(string path)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        string[] lines = File.ReadAllLines(path);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO dep (dep_id, dep_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            
            cmd.ExecuteNonQuery();
        }
    }

    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        string[] columns;
        List<string[]> rows = new List<string[]>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        var reader = cmd.ExecuteReader();

        columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }

        return (columns, rows);
    }

    public void InitializeDatabase(string depCsvPath, string teacherCsvPath)
    {
        CreateTables();
        
        if (GetAllDepartments().Count == 0 && File.Exists(depCsvPath))
        {
            ImportDepartmentsFromCsv(depCsvPath);
            Console.WriteLine($"[OK] Загружены кафедры из {depCsvPath}");
        }
        
        if (GetAllTeachers().Count == 0 && File.Exists(teacherCsvPath))
        {
            ImportTeachersFromCsv(teacherCsvPath);
            Console.WriteLine($"[OK] Загружены преподаватели из {teacherCsvPath}");
        }
    }


}
