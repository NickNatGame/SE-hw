namespace Pos.Domain;

public class Rubric
{
    private readonly List<Criterion> _criteria = new();

    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();

    public void Add(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        if (_criteria.Any(existing => existing.Id == criterion.Id))
            throw new InvalidOperationException("Критерий уже добавлен.");

        _criteria.Add(criterion);
    }

    public Criterion GetById(Guid id)
    {
        return _criteria.FirstOrDefault(criterion => criterion.Id == id)
            ?? throw new ArgumentException("Критерий отсутствует в рубрикаторе.", nameof(id));
    }
}
