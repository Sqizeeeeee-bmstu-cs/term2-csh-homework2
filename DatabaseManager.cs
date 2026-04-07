using Microsoft.Data.Sqlite;

/// <summary>
/// Управление базой данных SQLite.
/// </summary>
class DatabaseManager
{
    private string _connectionString;

    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public void InitializeDatabase(string deptCsvPath, string teacherCsvPath)
    {
        CreateTables();
        
        if (GetAllDepartments().Count == 0 && File.Exists(deptCsvPath))
        {
            ImportDepartmentsFromCsv(deptCsvPath);
        }
        
        if (GetAllTeachers().Count == 0 && File.Exists(teacherCsvPath))
        {
            ImportTeachersFromCsv(teacherCsvPath);
        }
    }

    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS departments (
                department_id INTEGER PRIMARY KEY AUTOINCREMENT,
                department_name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS teachers (
                teacher_id INTEGER PRIMARY KEY AUTOINCREMENT,
                department_id INTEGER NOT NULL,
                teacher_name TEXT NOT NULL,
                publications INTEGER NOT NULL,
                FOREIGN KEY (department_id) REFERENCES departments(department_id)
            );
        ";
        cmd.ExecuteNonQuery();
        Console.WriteLine("Таблицы созданы");
    }

    private void ImportDepartmentsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        Console.WriteLine($"CSV кафедр: {lines.Length} строк (включая заголовок)");
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO departments (department_id, department_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"  + {parts[1]}");
        }
    }

    private void ImportTeachersFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        Console.WriteLine($"CSV преподавателей: {lines.Length} строк (включая заголовок)");
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO teachers (teacher_id, department_id, teacher_name, publications) VALUES (@id, @deptId, @name, @publications)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@deptId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@publications", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
            Console.WriteLine($"  + {parts[2]}");
        }
    }

    public List<Department> GetAllDepartments()
    {
        var result = new List<Department>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT department_id, department_name FROM departments ORDER BY department_id";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Department(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }

    public List<Teacher> GetAllTeachers()
    {
        var result = new List<Teacher>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT teacher_id, department_id, teacher_name, publications FROM teachers ORDER BY teacher_id";
        
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Teacher(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3)));
        }
        return result;
    }

    public Teacher? GetTeacherById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT teacher_id, department_id, teacher_name, publications FROM teachers WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Teacher(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3));
        }
        return null;
    }

    public void AddTeacher(Teacher teacher)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO teachers (department_id, teacher_name, publications) VALUES (@deptId, @name, @publications)";
        cmd.Parameters.AddWithValue("@deptId", teacher.DepartmentId);
        cmd.Parameters.AddWithValue("@name", teacher.Name);
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);
        cmd.ExecuteNonQuery();
    }

    public void UpdateTeacher(Teacher teacher)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE teachers SET department_id = @deptId, teacher_name = @name, publications = @publications WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", teacher.Id);
        cmd.Parameters.AddWithValue("@deptId", teacher.DepartmentId);
        cmd.Parameters.AddWithValue("@name", teacher.Name);
        cmd.Parameters.AddWithValue("@publications", teacher.Publications);
        cmd.ExecuteNonQuery();
    }

    public void DeleteTeacher(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM teachers WHERE teacher_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        
        using var reader = cmd.ExecuteReader();
        
        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);
        
        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        
        return (columns, rows);
    }
}
