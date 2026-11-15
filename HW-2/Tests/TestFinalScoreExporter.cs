using Pos.Application;

namespace Pos.Tests;

public class TestFinalScoreExporter : IFinalScoreExporter
{
    public IReadOnlyList<FinalScoreResult> Results { get; private set; } = new List<FinalScoreResult>();
    public bool Called { get; private set; }

    public void Export(string path, IReadOnlyList<FinalScoreResult> results)
    {
        Called = true;
        Results = results;
    }
}
