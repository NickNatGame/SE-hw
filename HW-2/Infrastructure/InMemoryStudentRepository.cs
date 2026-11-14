using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new List<Student>();

    public void Add(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        foreach (Student existing in _students)
        {
            if (existing.Id == student.Id)
            {
                throw new InvalidOperationException("Студент уже добавлен.");
            }
        }

        _students.Add(student);
    }

    public Student GetById(Guid id)
    {
        foreach (Student student in _students)
        {
            if (student.Id == id)
            {
                return student;
            }
        }

        throw new ArgumentException("Студент не найден.", nameof(id));
    }

    public IReadOnlyList<Student> GetAll()
    {
        return _students.AsReadOnly();
    }
}
