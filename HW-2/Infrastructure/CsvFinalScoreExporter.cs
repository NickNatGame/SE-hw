using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public class CsvFinalScoreExporter : IFinalScoreExporter
{
    public void Export(string path, IReadOnlyList<FinalScoreResult> results)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(results);

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("StudentId;StudentName;FinalScore");
        foreach (FinalScoreResult result in results)
        {
            string name = EscapeName(result.StudentName);
            string score = result.Score.ToString(CultureInfo.InvariantCulture);
            csv.AppendLine($"{result.StudentId};{name};{score}");
        }

        using FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        using StreamWriter writer = new StreamWriter(file, new UTF8Encoding(true));
        writer.Write(csv.ToString());
    }

    private static string EscapeName(string name)
    {
        string trimmed = name.TrimStart();
        if (trimmed.StartsWith("=") || trimmed.StartsWith("+") ||
            trimmed.StartsWith("-") || trimmed.StartsWith("@"))
        {
            name = "'" + name;
        }

        return "\"" + name.Replace("\"", "\"\"") + "\"";
    }
}
