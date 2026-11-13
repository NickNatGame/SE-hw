namespace Pos.Domain;

public class Evaluation
{
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public decimal Score { get; }

    public Evaluation(Student student, Rubric rubric, Guid criterionId, decimal score)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);

        Criterion criterion = rubric.GetById(criterionId);
        if (score < 0 || score > criterion.MaxScore)
            throw new ArgumentOutOfRangeException(nameof(score), "Балл вне диапазона критерия.");

        StudentId = student.Id;
        CriterionId = criterion.Id;
        Score = score;
    }
}
