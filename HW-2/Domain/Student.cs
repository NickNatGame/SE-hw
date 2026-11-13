namespace Pos.Domain;

public class Student
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }

    public Student(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя студента не может быть пустым.", nameof(name));

        Name = name.Trim();
    }
}
