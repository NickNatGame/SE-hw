namespace Pos.Domain;

public interface IFinalScoreStrategy
{
    decimal CalculateByFormula(Rubric rubric, IReadOnlyDictionary<Guid, Evaluation> evaluations);
}
