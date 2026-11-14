namespace Pos.Domain;

public class FinalScoreCalculator : IFinalScoreCalculator
{
    private readonly IFinalScoreStrategy _strategy;

    public FinalScoreCalculator(IFinalScoreStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategy = strategy;
    }

    public decimal Calculate(Student student, Rubric rubric)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);

        if (rubric.Criteria.Count == 0)
        {
            throw new InvalidOperationException("Нельзя рассчитать итог без критериев.");
        }

        var studentEvaluations = new Dictionary<Guid, Evaluation>();
        foreach (Evaluation evaluation in student.Evaluations)
        {
            studentEvaluations.Add(evaluation.CriterionId, evaluation);
        }

        foreach (Criterion criterion in rubric.Criteria)
        {
            if (!studentEvaluations.ContainsKey(criterion.Id))
            {
                throw new InvalidOperationException(
                    $"Нельзя рассчитать итог: не выставлена оценка по критерию «{criterion.Name}».");
            }
        }

        return _strategy.CalculateByFormula(rubric, studentEvaluations);
    }
}
