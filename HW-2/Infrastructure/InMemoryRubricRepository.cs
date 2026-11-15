using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public class InMemoryRubricRepository : IRubricRepository
{
    private readonly Rubric _rubric = new Rubric();

    public Rubric Get()
    {
        return _rubric;
    }
}
