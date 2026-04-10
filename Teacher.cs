
public class Teacher
{
    
    public int Id {get; set; }

    public int DepartmentId {get; set; }

    public string Name {get; set; }

    private int _publications;

    public int Publications
    {
        get => _publications;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("Кол-во публикаций должно быть положительным");
            }
            _publications = value;
        }
    }

    public Teacher(int id, int depId, string name, int publications)
    {
        Id = id;
        DepartmentId = depId;
        Name = name;
        Publications = publications;
    }

    public Teacher() : this(0, 0, "", 0) {}

    public override string ToString()
    {
        return $"{Id}, {DepartmentId}, {Name}, {Publications}";
    }

}
