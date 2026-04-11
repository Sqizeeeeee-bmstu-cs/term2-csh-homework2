using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string dbPath = "teachers.db";
string depCsv = "departments.csv";
string teacherCsv = "teachers.csv";

var db = new DatabaseManager(dbPath);
db.InitializeDatabase(depCsv, teacherCsv);

string choice = "";
while (choice != "0")
{
    Console.WriteLine("\n=== УПРАВЛЕНИЕ ПРЕПОДАВАТЕЛЯМИ ===");
    Console.WriteLine("1 - Показать все кафедры");
    Console.WriteLine("2 - Показать всех преподавателей");
    Console.WriteLine("3 - Добавить преподавателя");
    Console.WriteLine("4 - Редактировать преподавателя");
    Console.WriteLine("5 - Удалить преподавателя");
    Console.WriteLine("6 - Отчёты");
    Console.WriteLine("0 - Выход");
    Console.Write("Ваш выбор: ");
    choice = Console.ReadLine();

    if (choice == "1")
    {
        Console.WriteLine("\n---- Все кафедры ----");
        var departments = db.GetAllDepartments();
        foreach (var dep in departments)
            Console.WriteLine($"  {dep}");
        Console.WriteLine($"Итого: {departments.Count}");
    }
    else if (choice == "2")
    {
        Console.WriteLine("\n---- Все преподаватели ----");
        var teachers = db.GetAllTeachers();
        foreach (var teacher in teachers)
            Console.WriteLine($"  {teacher}");
        Console.WriteLine($"Итого: {teachers.Count}");
    }
    else if (choice == "3")
    {
        Console.WriteLine("\n---- Добавление преподавателя ----");
        Console.WriteLine("Доступные кафедры:");
        var departments = db.GetAllDepartments();
        foreach (var dep in departments)
            Console.WriteLine($"  {dep}");

        Console.Write("ID кафедры: ");
        int depId = int.Parse(Console.ReadLine());

        Console.Write("Имя преподавателя: ");
        string name = Console.ReadLine();

        Console.Write("Количество публикаций: ");
        int publications = int.Parse(Console.ReadLine());

        var teacher = new Teacher(0, depId, name, publications);
        db.AddTeacher(teacher);
        Console.WriteLine("Преподаватель добавлен.");
    }
    else if (choice == "4")
    {
        Console.WriteLine("\n---- Редактирование преподавателя ----");
        Console.Write("Введите ID преподавателя: ");
        int id = int.Parse(Console.ReadLine());

        var teacher = db.GetTeacherById(id);
        if (teacher == null)
        {
            Console.WriteLine($"Преподаватель с ID={id} не найден.");
        }
        else
        {
            Console.WriteLine($"Текущие данные: {teacher}");
            Console.WriteLine("(Нажмите Enter, чтобы оставить значение без изменений)");

            Console.Write($"Имя [{teacher.Name}]: ");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
                teacher.Name = input;

            Console.Write($"ID кафедры [{teacher.DepartmentId}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
                teacher.DepartmentId = int.Parse(input);

            Console.Write($"Публикации [{teacher.Publications}]: ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
                teacher.Publications = int.Parse(input);

            db.UpdateTeacher(teacher);
            Console.WriteLine("Данные обновлены.");
        }
    }
    else if (choice == "5")
    {
        Console.WriteLine("\n---- Удаление преподавателя ----");
        Console.Write("Введите ID преподавателя: ");
        int id = int.Parse(Console.ReadLine());

        var teacher = db.GetTeacherById(id);
        if (teacher == null)
        {
            Console.WriteLine($"Преподаватель с ID={id} не найден.");
        }
        else
        {
            Console.Write($"Удалить \"{teacher.Name}\"? (да/нет): ");
            string confirm = Console.ReadLine();
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
    }
    else if (choice == "6")
    {
        string reportChoice = "";
        while (reportChoice != "0")
        {
            Console.WriteLine("\n--- Отчёты ---");
            Console.WriteLine("1 - Преподаватели по кафедрам");
            Console.WriteLine("2 - Количество преподавателей по кафедрам");
            Console.WriteLine("3 - Среднее количество публикаций по кафедрам");
            Console.WriteLine("0 - Назад");
            Console.Write("Ваш выбор: ");
            reportChoice = Console.ReadLine();

            if (reportChoice == "1")
            {
                new ReportBuilder(db)
                    .Query(@"SELECT t.teacher_name, d.dep_name, t.publications 
                             FROM teacher t 
                             JOIN dep d ON t.dep_id = d.dep_id 
                             ORDER BY t.teacher_name")
                    .Title("Преподаватели по кафедрам")
                    .Header("Имя", "Кафедра", "Публикации")
                    .ColumnWidths(25, 25, 12)
                    .Print();
            }
            else if (reportChoice == "2")
            {
                new ReportBuilder(db)
                    .Query(@"SELECT d.dep_name, COUNT(*) AS count 
                             FROM teacher t 
                             JOIN dep d ON t.dep_id = d.dep_id 
                             GROUP BY d.dep_name 
                             ORDER BY d.dep_name")
                    .Title("Количество преподавателей по кафедрам")
                    .Header("Кафедра", "Количество")
                    .ColumnWidths(30, 12)
                    .Print();
            }
            else if (reportChoice == "3")
            {
                new ReportBuilder(db)
                    .Query(@"SELECT d.dep_name, ROUND(AVG(t.publications), 1) AS avg_publications 
                             FROM teacher t 
                             JOIN dep d ON t.dep_id = d.dep_id 
                             GROUP BY d.dep_name 
                             ORDER BY avg_publications DESC")
                    .Title("Среднее количество публикаций по кафедрам")
                    .Header("Кафедра", "Среднее публикаций")
                    .ColumnWidths(30, 20)
                    .Print();
            }
        }
    }
}