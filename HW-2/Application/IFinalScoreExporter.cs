namespace Pos.Application;

public interface IFinalScoreExporter
{
    void Export(string path, IReadOnlyList<FinalScoreResult> results);
}
