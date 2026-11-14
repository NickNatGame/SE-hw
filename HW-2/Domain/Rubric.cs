namespace Pos.Domain;

public class Rubric
{
    private readonly List<Criterion> _criteria = new List<Criterion>();

    public IReadOnlyList<Criterion> Criteria
    {
        get
        {
            return _criteria.AsReadOnly();
        }
    }

    public void Add(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        foreach (Criterion existing in _criteria)
        {
            if (existing.Id == criterion.Id)
            {
                throw new InvalidOperationException("Критерий уже добавлен.");
            }
        }

        _criteria.Add(criterion);
    }

    public Criterion GetById(Guid id)
    {
        foreach (Criterion criterion in _criteria)
        {
            if (criterion.Id == id)
            {
                return criterion;
            }
        }

        throw new ArgumentException("Критерий отсутствует в рубрикаторе.", nameof(id));
    }
}
