namespace Pos.Domain;

public class Criterion
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public ScoreRange Range { get; }

    public decimal MaxScore
    {
        get
        {
            return Range.Maximum;
        }
    }

    public decimal Weight { get; }

    public Criterion(string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название критерия не может быть пустым.", nameof(name));
        }

        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Вес должен быть положительным.");
        }

        Name = name.Trim();
        Range = new ScoreRange(maxScore);
        Weight = weight;
    }
}
