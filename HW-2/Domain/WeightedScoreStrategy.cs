namespace Pos.Domain;

public class WeightedScoreStrategy : IFinalScoreStrategy
{
    public decimal CalculateByFormula(Rubric rubric, IReadOnlyDictionary<Guid, Evaluation> evaluations)
    {
        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (Criterion criterion in rubric.Criteria)
        {
            Evaluation evaluation = evaluations[criterion.Id];
            decimal contribution = evaluation.Score / criterion.MaxScore * criterion.Weight;
            weightedSum += contribution;
            totalWeight += criterion.Weight;
        }

        return weightedSum / totalWeight * 10;
    }
}
