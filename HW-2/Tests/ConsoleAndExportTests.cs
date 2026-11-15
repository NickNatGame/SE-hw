using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;
using Pos.Presentation;

namespace Pos.Tests;

public class ConsoleAndExportTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("pos-tests-").FullName;

    [Fact]
    public void DemoRunsWithoutInputAndExportsRestoredEvaluations()
    {
        using StringReader input = new StringReader("");
        using StringWriter output = new StringWriter();
        using ServiceProvider provider = Program.CreateProvider(new WeightedScoreStrategy(), false, input, output);
        provider.GetRequiredService<ConsoleApplication>().RunDemo();

        string text = output.ToString();
        string prefix = "Итоги экспортированы: ";
        string exportLine = Assert.Single(text.Split('\n'), line => line.StartsWith(prefix));
        string path = exportLine.Substring(prefix.Length).Trim();
        try
        {
            string normalized = text.Replace(',', '.');
            Assert.Contains("Начальный итог: 7.85/10", normalized);
            Assert.Contains("После исправления проекта с 15 на 20: 8.6/10", normalized);
            Assert.Contains("Ожидаемый отказ:", text);
            Assert.Contains("у студента «Никита Пичурин»", text);
            Assert.Contains("по критерию «Проект»", text);
            Assert.Contains("После повторного выставления проекта: 7.85/10", normalized);
            Assert.Contains(";\"Никита Пичурин\";7.85", File.ReadAllText(path));
            Assert.Equal(2, provider.GetRequiredService<GradingService>().GetEvaluations(
                provider.GetRequiredService<GradingService>().GetStudents()[0].Id).Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData(false, false, "7.85")]
    [InlineData(false, true, "7.85")]
    [InlineData(true, false, "7.75")]
    [InlineData(true, true, "7.75")]
    public void ConsoleScenarioUsesContainerAndExportsSelectedCalculation(bool average, bool rounded, string expected)
    {
        string path = Path.Combine(_directory, "results.csv");
        string commands = string.Join('\n', new string[]
        {
            "1", "Анна", "3", "ДЗ", "10", "70", "3", "Проект", "20", "30",
            "5", "1", "1", "8", "5", "1", "2", "15", "2", "4", "6", "1",
            "9", "1", "10", path, "0"
        });
        using StringReader input = new StringReader(commands);
        using StringWriter output = new StringWriter();
        IFinalScoreStrategy strategy = new WeightedScoreStrategy();
        if (average)
        {
            strategy = new AverageScoreStrategy();
        }

        using ServiceProvider provider = Program.CreateProvider(strategy, rounded, input, output);
        provider.GetRequiredService<ConsoleApplication>().Run();

        Assert.Contains($"Итог Анна: {expected}/10", output.ToString().Replace(',', '.'));
        Assert.Contains("Итоги экспортированы", output.ToString());
        Assert.Contains($";\"Анна\";{expected}", File.ReadAllText(path));
        Assert.Single(provider.GetRequiredService<IStudentRepository>().GetAll());
        Assert.Same(provider.GetRequiredService<IStudentRepository>(), provider.GetRequiredService<IStudentRepository>());
    }

    [Fact]
    public void ConsoleHandlesBadInputCorrectionCancellationAndEndOfInput()
    {
        string path = Path.Combine(_directory, "incomplete.csv");
        string commands = string.Join('\n', new string[]
        {
            "abc", "99", "9", "1", "  ", "Анна", "3", "ДЗ", "10", "1",
            "5", "1", "1", "abc", "11", "5", "1", "1", "8,5",
            "5", "1", "1", "9", "7", "1", "1", "-1", "9", "1",
            "7", "1", "1", "10", "9", "1", "8", "1", "1", "9", "1",
            "10", path
        });
        using StringReader input = new StringReader(commands);
        using StringWriter output = new StringWriter();
        using ServiceProvider provider = Program.CreateProvider(new WeightedScoreStrategy(), false, input, output);
        provider.GetRequiredService<ConsoleApplication>().Run();

        string text = output.ToString().Replace(',', '.');
        Assert.Contains("Введите целое число", text);
        Assert.Contains("Введите непустое значение", text);
        Assert.Contains("Введите число", text);
        Assert.Contains("Сначала добавьте студента", text);
        Assert.Contains("Балл вне диапазона", text);
        Assert.Contains("Оценка уже выставлена", text);
        Assert.Contains("Итог Анна: 8.5/10", text);
        Assert.Contains("Итог Анна: 10/10", text);
        Assert.Contains("Оценка отменена", text);
        Assert.Contains("не выставлена оценка", text);
        Assert.Contains("Ввод завершён", text);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void ExportValidatesAllStudentsBeforeCallingAdapter()
    {
        TestFinalScoreExporter exporter = new TestFinalScoreExporter();
        GradingService grading = new GradingService(new InMemoryStudentRepository(),
            new InMemoryRubricRepository(), new FinalScoreCalculator(new WeightedScoreStrategy()), exporter);
        Student first = grading.AddStudent("Анна");
        Student second = grading.AddStudent("Борис");
        Criterion criterion = grading.AddCriterion("ДЗ", 10, 1);
        grading.AddEvaluation(first.Id, criterion.Id, 8);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => grading.ExportFinalScores("results.csv"));
        Assert.Contains($"у студента «{second.Name}»", exception.Message);
        Assert.Contains($"по критерию «{criterion.Name}»", exception.Message);
        Assert.False(exporter.Called);

        grading.AddEvaluation(second.Id, criterion.Id, 0);
        grading.ExportFinalScores("results.csv");
        Assert.Equal(2, exporter.Results.Count);
        Assert.Equal(8m, exporter.Results[0].Score);
        Assert.Equal(0m, exporter.Results[1].Score);
    }

    [Fact]
    public void EmptyExportDoesNotCreateFile()
    {
        string path = Path.Combine(_directory, "empty.csv");
        using StringReader input = new StringReader("");
        using StringWriter output = new StringWriter();
        using ServiceProvider provider = Program.CreateProvider(new WeightedScoreStrategy(), false, input, output);

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<GradingService>().ExportFinalScores(path));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void CsvEscapesNamesUsesInvariantNumbersAndRefusesOverwrite()
    {
        CsvFinalScoreExporter exporter = new CsvFinalScoreExporter();
        string path = Path.Combine(_directory, "quoted.csv");
        List<FinalScoreResult> results = new List<FinalScoreResult>
        {
            new FinalScoreResult(Guid.NewGuid(), "Анна; \"Иванова\"\nВторая строка", 7.85m),
            new FinalScoreResult(Guid.NewGuid(), "=1+1", 0)
        };
        exporter.Export(path, results);
        string text = File.ReadAllText(path);

        Assert.StartsWith("StudentId;StudentName;FinalScore", text);
        Assert.Contains(";\"Анна; \"\"Иванова\"\"\nВторая строка\";7.85", text);
        Assert.Contains(";\"'=1+1\";0", text);
        Assert.Throws<IOException>(() => exporter.Export(path, results));
        Assert.Equal(text, File.ReadAllText(path));
    }

    [Fact]
    public void ConsoleReportsExportErrorsAndContinues()
    {
        string existing = Path.Combine(_directory, "existing.csv");
        File.WriteAllText(existing, "keep me");
        string missingDirectory = Path.Combine(_directory, "missing", "result.csv");
        string commands = string.Join('\n', new string[]
        {
            "1", "Анна", "3", "ДЗ", "10", "1", "5", "1", "1", "8",
            "10", existing, "10", missingDirectory, "9", "1", "0"
        });
        using StringReader input = new StringReader(commands);
        using StringWriter output = new StringWriter();
        using ServiceProvider provider = Program.CreateProvider(new WeightedScoreStrategy(), false, input, output);
        provider.GetRequiredService<ConsoleApplication>().Run();

        Assert.Contains("Ошибка файла", output.ToString());
        Assert.Contains("Итог Анна: 8/10", output.ToString());
        Assert.Equal("keep me", File.ReadAllText(existing));
        Assert.False(File.Exists(missingDirectory));
    }

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }
}
