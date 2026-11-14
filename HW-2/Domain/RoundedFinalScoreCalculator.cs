namespace Pos.Domain;

public class RoundedFinalScoreCalculator : IFinalScoreCalculator
{
    private readonly IFinalScoreCalculator _calculator;

    public RoundedFinalScoreCalculator(IFinalScoreCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        _calculator = calculator;
    }

    public decimal Calculate(Student student, Rubric rubric)
    {
        decimal result = _calculator.Calculate(student, rubric);
        return Math.Round(result, 2, MidpointRounding.AwayFromZero);
    }
}
