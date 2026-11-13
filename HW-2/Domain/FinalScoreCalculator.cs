namespace Pos.Domain;

public class FinalScoreCalculator
{
    // Отсутствующая оценка считается нулём; результат выражен по 10-балльной шкале.
    public decimal Calculate(Student student, Rubric rubric, IEnumerable<Evaluation> evaluations)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(evaluations);

        if (rubric.Criteria.Count == 0)
            throw new InvalidOperationException("Нельзя рассчитать итог без критериев.");

        var studentEvaluations = evaluations.Where(evaluation => evaluation.StudentId == student.Id)
            .ToDictionary(evaluation => evaluation.CriterionId);
        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (Criterion criterion in rubric.Criteria)
        {
            studentEvaluations.TryGetValue(criterion.Id, out Evaluation? evaluation);
            weightedSum += (evaluation?.Score ?? 0) / criterion.MaxScore * criterion.Weight;
            totalWeight += criterion.Weight;
        }

        return 10 * weightedSum / totalWeight;
    }
}
