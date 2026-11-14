namespace Pos.Domain;

public class Evaluation
{
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public decimal Score { get; }

    private Evaluation(Guid studentId, Guid criterionId, decimal score)
    {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
    }

    public static Evaluation Create(Student student, Rubric rubric, Guid criterionId, decimal score)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);

        Criterion criterion = rubric.GetById(criterionId);
        if (!criterion.Range.Contains(score))
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Балл вне диапазона критерия.");
        }

        return new Evaluation(student.Id, criterion.Id, score);
    }
}
