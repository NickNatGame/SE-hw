using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Tests;

public class PatternTests
{
    [Fact]
    public void EvaluationFactoryCreatesValidatedEvaluation()
    {
        Student student = new Student("Анна");
        Rubric rubric = new Rubric();
        Criterion criterion = new Criterion("ДЗ", 10, 1);
        rubric.Add(criterion);

        Evaluation evaluation = Evaluation.Create(student, rubric, criterion.Id, 8);

        Assert.Equal(student.Id, evaluation.StudentId);
        Assert.Equal(criterion.Id, evaluation.CriterionId);
        Assert.Equal(8m, evaluation.Score);
        Assert.Throws<ArgumentOutOfRangeException>(() => Evaluation.Create(student, rubric, criterion.Id, 11));
        Assert.Throws<ArgumentException>(() => Evaluation.Create(student, rubric, Guid.NewGuid(), 8));
        Assert.Throws<ArgumentNullException>(() => Evaluation.Create(null!, rubric, criterion.Id, 8));
        Assert.Throws<ArgumentNullException>(() => Evaluation.Create(student, null!, criterion.Id, 8));
    }

    [Theory]
    [InlineData(false, false, 7.85)]
    [InlineData(false, true, 7.85)]
    [InlineData(true, false, 7.75)]
    [InlineData(true, true, 7.75)]
    public void StrategiesAndDecoratorWorkThroughFacade(bool average, bool rounded, double expected)
    {
        GradingService grading = new GradingService(
            new InMemoryStudentRepository(),
            new InMemoryRubricRepository(),
            CreateCalculator(average, rounded),
            new TestFinalScoreExporter());
        Student student = grading.AddStudent("Анна");
        Criterion homework = grading.AddCriterion("ДЗ", 10, 70);
        Criterion project = grading.AddCriterion("Проект", 20, 30);
        grading.AddEvaluation(student.Id, homework.Id, 8);
        grading.AddEvaluation(student.Id, project.Id, 15);

        Assert.Equal((decimal)expected, grading.CalculateFinalScore(student.Id));
        Assert.Same(project, grading.GetCriterion(project.Id));
        Assert.Throws<ArgumentException>(() => grading.GetCriterion(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DecoratorRoundsEitherStrategyWithoutChangingOriginalCalculator(bool average)
    {
        Student student = new Student("Анна");
        Rubric rubric = new Rubric();
        Criterion criterion = new Criterion("ДЗ", 3, 1);
        rubric.Add(criterion);
        student.AddEvaluation(rubric, criterion.Id, 1);
        IFinalScoreCalculator calculator = CreateCalculator(average, false);
        IFinalScoreCalculator rounded = new RoundedFinalScoreCalculator(calculator);

        decimal exactResult = calculator.Calculate(student, rubric);
        Assert.InRange(exactResult, 3.333333333333333333333333333m, 3.333333333333333333333333334m);
        Assert.Equal(3.33m, rounded.Calculate(student, rubric));
        Assert.Equal(exactResult, calculator.Calculate(student, rubric));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 10)]
    [InlineData(7.845, 7.85)]
    public void DecoratorPreservesBoundariesAndRoundsMidpointsUp(double value, double expected)
    {
        Student student = new Student("Анна");
        Rubric rubric = new Rubric();
        Criterion criterion = new Criterion("ДЗ", 10, 1);
        rubric.Add(criterion);
        student.AddEvaluation(rubric, criterion.Id, (decimal)value);
        IFinalScoreCalculator calculator = CreateCalculator(false, true);

        Assert.Equal((decimal)expected, calculator.Calculate(student, rubric));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void AllCombinationsRejectIncompleteResultsAndAcceptExplicitZero(bool average, bool rounded)
    {
        Student student = new Student("Анна");
        Rubric rubric = new Rubric();
        IFinalScoreCalculator calculator = CreateCalculator(average, rounded);
        Assert.Throws<InvalidOperationException>(() => calculator.Calculate(student, rubric));

        Criterion criterion = new Criterion("ДЗ", 10, 1);
        rubric.Add(criterion);
        Assert.Throws<InvalidOperationException>(() => calculator.Calculate(student, rubric));

        student.AddEvaluation(rubric, criterion.Id, 0);
        Assert.Equal(0m, calculator.Calculate(student, rubric));

        student.CorrectEvaluation(rubric, criterion.Id, 10);
        Assert.Equal(10m, calculator.Calculate(student, rubric));

        student.CancelEvaluation(criterion.Id);
        Assert.Throws<InvalidOperationException>(() => calculator.Calculate(student, rubric));
    }

    [Fact]
    public void CalculatorAndDecoratorRejectNullDependenciesAndArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new FinalScoreCalculator(null!));
        Assert.Throws<ArgumentNullException>(() => new RoundedFinalScoreCalculator(null!));
        IFinalScoreCalculator calculator = CreateCalculator(false, true);

        Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null!, new Rubric()));
        Assert.Throws<ArgumentNullException>(() => calculator.Calculate(new Student("Анна"), null!));
    }

    private static IFinalScoreCalculator CreateCalculator(bool average, bool rounded)
    {
        IFinalScoreStrategy strategy = new WeightedScoreStrategy();
        if (average)
        {
            strategy = new AverageScoreStrategy();
        }

        IFinalScoreCalculator calculator = new FinalScoreCalculator(strategy);
        if (rounded)
        {
            calculator = new RoundedFinalScoreCalculator(calculator);
        }

        return calculator;
    }
}
