namespace Pos.Domain;

public sealed record ScoreRange
{
    public decimal Maximum { get; }

    public ScoreRange(decimal maximum)
    {
        if (maximum <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Максимум должен быть положительным.");
        }

        Maximum = maximum;
    }

    public bool Contains(decimal score)
    {
        return score >= 0 && score <= Maximum;
    }
}
