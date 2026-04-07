using Microsoft.Data.Sqlite;

string dbPath = "university.db";
string deptCsv = "departments.csv";
string teacherCsv = "teachers.csv";


var db = new DatabaseManager(dbPath);
db.InitializeDatabase(deptCsv, teacherCsv);

Console.WriteLine();

string choice;
do
{
    Console.WriteLine("╔════════════════════════════════════════════╗");
    Console.WriteLine("║     УПРАВЛЕНИЕ КАФЕДРАМИ И ПРЕПОДАВАТЕЛЯМИ ║");
    Console.WriteLine("╠════════════════════════════════════════════╣");
    Console.WriteLine("║ 1 — Показать все кафедры                   ║");
    Console.WriteLine("║ 2 — Показать всех преподавателей           ║");
    Console.WriteLine("║ 3 — Добавить преподавателя                 ║");
    Console.WriteLine("║ 4 — Редактировать преподавателя            ║");
    Console.WriteLine("║ 5 — Удалить преподавателя                  ║");
    Console.WriteLine("║ 6 — Отчёты                                 ║");
    Console.WriteLine("║ 0 — Выход                                  ║");
    Console.WriteLine("╚════════════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");

    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowDepartments(db); break;
        case "2": ShowTeachers(db); break;
        case "3": AddTeacher(db); break;
        case "4": EditTeacher(db); break;
        case "5": DeleteTeacher(db); break;
        case "6": ReportsMenu(db); break;
        case "0": Console.WriteLine("До свидания!"); break;
        default: Console.WriteLine("Неверный пункт меню."); break;
    }
    Console.WriteLine();
} while (choice != "0");

// ==================== ФУНКЦИИ МЕНЮ ====================

static void ShowDepartments(DatabaseManager db)
{
    Console.WriteLine("---- Все кафедры ----");
    var departments = db.GetAllDepartments();
    foreach (var dep in departments)
        Console.WriteLine($"  {dep}");
    Console.WriteLine($"Итого: {departments.Count}");
}

static void ShowTeachers(DatabaseManager db)
{
    Console.WriteLine("---- Все преподаватели ----");
    var teachers = db.GetAllTeachers();
    foreach (var teacher in teachers)
        Console.WriteLine($"  {teacher}");
    Console.WriteLine($"Итого: {teachers.Count}");
}

static void AddTeacher(DatabaseManager db)
{
    Console.WriteLine("---- Добавление преподавателя ----");
    
    // Показываем кафедры
    Console.WriteLine("Доступные кафедры:");
    var departments = db.GetAllDepartments();
    foreach (var dep in departments)
        Console.WriteLine($"  {dep}");
    
    Console.Write("ID кафедры: ");
    if (!int.TryParse(Console.ReadLine(), out int deptId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    Console.Write("Имя преподавателя: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0)
    {
        Console.WriteLine("Ошибка: имя не может быть пустым.");
        return;
    }
    
    Console.Write("Количество публикаций: ");
    if (!int.TryParse(Console.ReadLine(), out int publications))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    try
    {
        var teacher = new Teacher(0, deptId, name, publications);
        db.AddTeacher(teacher);
        Console.WriteLine("Преподаватель добавлен.");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditTeacher(DatabaseManager db)
{
    Console.WriteLine("---- Редактирование преподавателя ----");
    Console.Write("Введите ID преподавателя: ");
    
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    var teacher = db.GetTeacherById(id);
    if (teacher == null)
    {
        Console.WriteLine($"Преподаватель с ID={id} не найден.");
        return;
    }
    
    Console.WriteLine($"Текущие данные: {teacher}");
    Console.WriteLine("(Нажмите Enter, чтобы оставить значение без изменений)");
    
    // Имя
    Console.Write($"Имя [{teacher.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0)
        teacher.Name = input;
    
    // Кафедра
    Console.Write($"ID кафедры [{teacher.DepartmentId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newDeptId))
        teacher.DepartmentId = newDeptId;
    
    // Публикации
    Console.Write($"Публикации [{teacher.Publications}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newPublications))
    {
        try
        {
            teacher.Publications = newPublications;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return;
        }
    }
    
    db.UpdateTeacher(teacher);
    Console.WriteLine("Данные обновлены.");
}

static void DeleteTeacher(DatabaseManager db)
{
    Console.WriteLine("---- Удаление преподавателя ----");
    Console.Write("Введите ID преподавателя: ");
    
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }
    
    var teacher = db.GetTeacherById(id);
    if (teacher == null)
    {
        Console.WriteLine($"Преподаватель с ID={id} не найден.");
        return;
    }
    
    Console.Write($"Удалить «{teacher.Name}»? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
    if (confirm == "да")
    {
        db.DeleteTeacher(id);
        Console.WriteLine("Преподаватель удалён.");
    }
    else
    {
        Console.WriteLine("Удаление отменено.");
    }
}

// ==================== ПОДМЕНЮ ОТЧЁТОВ ====================

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine(" 1 - Преподаватели по кафедрам");
        Console.WriteLine(" 2 - Количество преподавателей по кафедрам");
        Console.WriteLine(" 3 - Среднее количество публикаций по кафедрам");
        Console.WriteLine(" 0 - Назад");
        Console.Write("Ваш выбор: ");
        
        choice = Console.ReadLine()?.Trim() ?? "";
        
        switch (choice)
        {
            case "1": Report1_TeachersWithDepartments(db); break;
            case "2": Report2_CountByDepartment(db); break;
            case "3": Report3_AvgPublicationsByDepartment(db); break;
            case "0": break;
            default: Console.WriteLine("Неверный пункт."); break;
        }
        Console.WriteLine();
    } while (choice != "0");
}

// Отчёт 1: Преподаватели с названиями кафедр (JOIN)
static void Report1_TeachersWithDepartments(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT t.teacher_name, d.department_name, t.publications
                FROM teachers t
                JOIN departments d ON t.department_id = d.department_id
                ORDER BY t.teacher_name")
        .Title("Преподаватели по кафедрам")
        .Header("Преподаватель", "Кафедра", "Публикации")
        .ColumnWidths(25, 30, 12)
        .Numbered()
        .Footer("Всего преподавателей")
        .Print();
}

// Отчёт 2: Количество преподавателей по кафедрам (GROUP BY + COUNT)
static void Report2_CountByDepartment(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT d.department_name, COUNT(*) AS cnt
                FROM teachers t
                JOIN departments d ON t.department_id = d.department_id
                GROUP BY d.department_name
                ORDER BY d.department_name")
        .Title("Количество преподавателей по кафедрам")
        .Header("Кафедра", "Кол-во")
        .ColumnWidths(35, 10)
        .Print();
}

// Отчёт 3: Среднее количество публикаций по кафедрам (GROUP BY + AVG)
static void Report3_AvgPublicationsByDepartment(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT d.department_name, ROUND(AVG(t.publications), 1) AS avg_publications
                FROM teachers t
                JOIN departments d ON t.department_id = d.department_id
                GROUP BY d.department_name
                ORDER BY avg_publications DESC")
        .Title("Среднее количество публикаций по кафедрам")
        .Header("Кафедра", "Среднее публикаций")
        .ColumnWidths(35, 20)
        .Print();
}
