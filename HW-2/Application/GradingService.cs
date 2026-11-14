using Pos.Domain;

namespace Pos.Application;

public class GradingService
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly FinalScoreCalculator _calculator;

    public GradingService(IStudentRepository students, IRubricRepository rubrics, FinalScoreCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(students);
        ArgumentNullException.ThrowIfNull(rubrics);
        ArgumentNullException.ThrowIfNull(calculator);

        _students = students;
        _rubrics = rubrics;
        _calculator = calculator;
    }

    public Student AddStudent(string name)
    {
        Student student = new Student(name);
        _students.Add(student);
        return student;
    }

    public IReadOnlyList<Student> GetStudents()
    {
        return _students.GetAll();
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        Rubric rubric = _rubrics.Get();
        Criterion criterion = new Criterion(name, maxScore, weight);
        rubric.Add(criterion);
        return criterion;
    }

    public IReadOnlyList<Criterion> GetCriteria()
    {
        Rubric rubric = _rubrics.Get();
        return rubric.Criteria;
    }

    public void AddEvaluation(Guid studentId, Guid criterionId, decimal score)
    {
        Student student = _students.GetById(studentId);
        Rubric rubric = _rubrics.Get();
        student.AddEvaluation(rubric, criterionId, score);
    }

    public void CorrectEvaluation(Guid studentId, Guid criterionId, decimal score)
    {
        Student student = _students.GetById(studentId);
        Rubric rubric = _rubrics.Get();
        student.CorrectEvaluation(rubric, criterionId, score);
    }

    public void CancelEvaluation(Guid studentId, Guid criterionId)
    {
        Student student = _students.GetById(studentId);
        student.CancelEvaluation(criterionId);
    }

    public IReadOnlyList<Evaluation> GetEvaluations(Guid studentId)
    {
        Student student = _students.GetById(studentId);
        return student.Evaluations;
    }

    public decimal CalculateFinalScore(Guid studentId)
    {
        Student student = _students.GetById(studentId);
        Rubric rubric = _rubrics.Get();
        return _calculator.Calculate(student, rubric);
    }
}
