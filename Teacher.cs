/// <summary>
/// Преподаватель (основная таблица, сторона «много»)
/// </summary>
class Teacher
{
    /// <summary>
    /// Идентификатор преподавателя (первичный ключ)
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор кафедры (внешний ключ)
    /// </summary>
    public int DepartmentId { get; set; }
    
    /// <summary>
    /// Имя преподавателя
    /// </summary>
    public string Name { get; set; }
    
    private int _publications;
    
    /// <summary>
    /// Количество научных публикаций (не может быть отрицательным)
    /// </summary>
    public int Publications
    {
        get => _publications;
        set
        {
            if (value < 0)
                throw new ArgumentException("Количество публикаций не может быть отрицательным");
            _publications = value;
        }
    }
    
    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public Teacher(int id, int departmentId, string name, int publications)
    {
        Id = id;
        DepartmentId = departmentId;
        Name = name;
        Publications = publications; // валидация сработает здесь
    }
    
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public Teacher() : this(0, 0, "", 0)
    {
    }
    
    public override string ToString() => $"{Id}. {Name}, кафедра #{DepartmentId}, публикаций: {Publications}";
}
