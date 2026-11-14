namespace Pos.Domain;

public class FinalScoreCalculator
{
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

        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (Criterion criterion in rubric.Criteria)
        {
            if (!studentEvaluations.TryGetValue(criterion.Id, out Evaluation? evaluation))
            {
                throw new InvalidOperationException(
                    $"Нельзя рассчитать итог: не выставлена оценка по критерию «{criterion.Name}».");
            }

            decimal score = evaluation.Score;
            decimal contribution = score / criterion.MaxScore * criterion.Weight;

            weightedSum += contribution;
            totalWeight += criterion.Weight;
        }

        return weightedSum / totalWeight * 10;
    }
}
