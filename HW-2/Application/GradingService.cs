using Pos.Domain;

namespace Pos.Application;

public class GradingService
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly IFinalScoreCalculator _calculator;
    private readonly IFinalScoreExporter _exporter;

    public GradingService(IStudentRepository students, IRubricRepository rubrics,
        IFinalScoreCalculator calculator, IFinalScoreExporter exporter)
    {
        ArgumentNullException.ThrowIfNull(students);
        ArgumentNullException.ThrowIfNull(rubrics);
        ArgumentNullException.ThrowIfNull(calculator);
        ArgumentNullException.ThrowIfNull(exporter);

        _students = students;
        _rubrics = rubrics;
        _calculator = calculator;
        _exporter = exporter;
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

    public Criterion GetCriterion(Guid criterionId)
    {
        Rubric rubric = _rubrics.Get();
        return rubric.GetById(criterionId);
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

    public void ExportFinalScores(string path)
    {
        IReadOnlyList<Student> students = _students.GetAll();
        if (students.Count == 0)
        {
            throw new InvalidOperationException("Нет студентов для экспорта.");
        }

        List<FinalScoreResult> results = new List<FinalScoreResult>();
        foreach (Student student in students)
        {
            decimal score = CalculateFinalScore(student.Id);
            results.Add(new FinalScoreResult(student.Id, student.Name, score));
        }

        _exporter.Export(path, results);
    }
}
