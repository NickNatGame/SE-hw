namespace Pos.Domain;

public interface IFinalScoreCalculator
{
    decimal Calculate(Student student, Rubric rubric);
}
