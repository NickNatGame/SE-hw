namespace Pos.Domain;

public class AverageScoreStrategy : IFinalScoreStrategy
{
    public decimal CalculateByFormula(Rubric rubric, IReadOnlyDictionary<Guid, Evaluation> evaluations)
    {
        decimal sum = 0;

        foreach (Criterion criterion in rubric.Criteria)
        {
            Evaluation evaluation = evaluations[criterion.Id];
            sum += evaluation.Score / criterion.MaxScore;
        }

        return sum / rubric.Criteria.Count * 10;
    }
}
