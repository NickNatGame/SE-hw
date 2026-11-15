using Pos.Domain;

namespace Pos.Application;

public interface IStudentRepository
{
    void Add(Student student);
    Student GetById(Guid id);
    IReadOnlyList<Student> GetAll();
}
