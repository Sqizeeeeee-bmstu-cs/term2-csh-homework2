/// <summary>
/// Кафедра (справочная таблица, сторона «один»)
/// </summary>
class Department
{
    /// <summary>
    /// Идентификатор кафедры (первичный ключ)
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Название кафедры
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public Department(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public Department() : this(0, "")
    {
    }
    
    public override string ToString() => $"{Id}. {Name}";
}

