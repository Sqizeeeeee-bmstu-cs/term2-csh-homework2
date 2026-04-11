
public class Department
{
    public int Id {get; set;}

    public string Name {get; set;}

    public Department(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public Department() : this(0, "") { }

    public override string ToString()
    {
        return $"{Id}, {Name}";
    }
    
}
