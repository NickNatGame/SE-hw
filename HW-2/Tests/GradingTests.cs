using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Tests;

public class GradingTests
{
    private readonly GradingService _grading = new GradingService(
        new InMemoryStudentRepository(),
        new InMemoryRubricRepository(),
        new FinalScoreCalculator());

    [Fact]
    public void CompleteScenarioCalculatesCorrectsCancelsAndRestoresEvaluation()
    {
        Student student = _grading.AddStudent("Анна");
        Criterion homework = _grading.AddCriterion("ДЗ", 10, 70);
        Criterion project = _grading.AddCriterion("Проект", 20, 30);
        _grading.AddEvaluation(student.Id, homework.Id, 8);
        _grading.AddEvaluation(student.Id, project.Id, 15);

        Assert.Equal(7.85m, _grading.CalculateFinalScore(student.Id));

        _grading.CorrectEvaluation(student.Id, project.Id, 20);
        Assert.Equal(8.6m, _grading.CalculateFinalScore(student.Id));
        Assert.Equal(2, _grading.GetEvaluations(student.Id).Count);

        _grading.CancelEvaluation(student.Id, project.Id);
        Assert.Single(_grading.GetEvaluations(student.Id));
        Assert.Throws<InvalidOperationException>(() => _grading.CalculateFinalScore(student.Id));

        _grading.AddEvaluation(student.Id, project.Id, 15);
        Assert.Equal(7.85m, _grading.CalculateFinalScore(student.Id));
    }

    [Fact]
    public void MissingEvaluationPreventsCalculationButExplicitZeroIsAccepted()
    {
        Student student = _grading.AddStudent("Анна");
        Criterion criterion = _grading.AddCriterion("ДЗ", 10, 1);

        Assert.Throws<InvalidOperationException>(() => _grading.CalculateFinalScore(student.Id));

        _grading.AddEvaluation(student.Id, criterion.Id, 0);
        Assert.Equal(0m, _grading.CalculateFinalScore(student.Id));
    }

    [Fact]
    public void AddingCriterionMakesPreviouslyCompleteResultIncomplete()
    {
        Student student = _grading.AddStudent("Анна");
        Criterion homework = _grading.AddCriterion("ДЗ", 10, 1);
        _grading.AddEvaluation(student.Id, homework.Id, 10);
        Assert.Equal(10m, _grading.CalculateFinalScore(student.Id));

        _grading.AddCriterion("Проект", 20, 1);

        Assert.Throws<InvalidOperationException>(() => _grading.CalculateFinalScore(student.Id));
    }

    [Fact]
    public void DuplicateEvaluationIsRejectedByStudent()
    {
        Student student = new Student("Анна");
        Rubric rubric = new Rubric();
        Criterion criterion = new Criterion("ДЗ", 10, 1);
        rubric.Add(criterion);
        student.AddEvaluation(rubric, criterion.Id, 8);

        Assert.Throws<InvalidOperationException>(() => student.AddEvaluation(rubric, criterion.Id, 9));
        Assert.Equal(8m, student.Evaluations[0].Score);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void InvalidAdditionAndCorrectionDoNotChangeExistingEvaluations(int score)
    {
        Student student = _grading.AddStudent("Анна");
        Criterion criterion = _grading.AddCriterion("ДЗ", 10, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _grading.AddEvaluation(student.Id, criterion.Id, score));
        Assert.Empty(_grading.GetEvaluations(student.Id));

        _grading.AddEvaluation(student.Id, criterion.Id, 8);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _grading.CorrectEvaluation(student.Id, criterion.Id, score));
        Assert.Equal(8m, _grading.GetEvaluations(student.Id)[0].Score);
    }

    [Fact]
    public void UnknownCriterionAndStudentAreRejected()
    {
        Student student = _grading.AddStudent("Анна");
        Criterion criterion = _grading.AddCriterion("ДЗ", 10, 1);

        Assert.Throws<ArgumentException>(() => _grading.AddEvaluation(student.Id, Guid.NewGuid(), 5));
        Assert.Throws<ArgumentException>(() => _grading.AddEvaluation(Guid.NewGuid(), criterion.Id, 5));
        Assert.Empty(_grading.GetEvaluations(student.Id));
    }

    [Fact]
    public void MissingEvaluationCannotBeCorrectedOrCancelled()
    {
        Student student = _grading.AddStudent("Анна");
        Criterion criterion = _grading.AddCriterion("ДЗ", 10, 1);

        Assert.Throws<ArgumentException>(() => _grading.CorrectEvaluation(student.Id, criterion.Id, 5));
        Assert.Throws<ArgumentException>(() => _grading.CancelEvaluation(student.Id, criterion.Id));
    }

    [Fact]
    public void StudentsHaveIndependentEvaluations()
    {
        Student first = _grading.AddStudent("Анна");
        Student second = _grading.AddStudent("Борис");
        Criterion criterion = _grading.AddCriterion("ДЗ", 10, 1);
        _grading.AddEvaluation(first.Id, criterion.Id, 8);
        _grading.AddEvaluation(second.Id, criterion.Id, 4);

        Assert.Equal(8m, _grading.CalculateFinalScore(first.Id));
        Assert.Equal(4m, _grading.CalculateFinalScore(second.Id));
        Assert.Equal(2, _grading.GetStudents().Count);
        Assert.Single(_grading.GetCriteria());
    }

    [Fact]
    public void EmptyRubricPreventsCalculation()
    {
        Student student = _grading.AddStudent("Анна");

        Assert.Throws<InvalidOperationException>(() => _grading.CalculateFinalScore(student.Id));
    }

    [Fact]
    public void ScoreRangeHasValueEqualityAndIncludesBothBoundaries()
    {
        ScoreRange range = new ScoreRange(10);

        Assert.Equal(range, new ScoreRange(10));
        Assert.NotEqual(range, new ScoreRange(20));
        Assert.True(range.Contains(0));
        Assert.True(range.Contains(10));
        Assert.False(range.Contains(-1));
        Assert.False(range.Contains(11));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveMaximumAndWeightAreRejected(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreRange(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Criterion("ДЗ", value, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Criterion("ДЗ", 10, value));
    }

    [Fact]
    public void StudentRepositoryRejectsDuplicateIdentityButAllowsSameName()
    {
        InMemoryStudentRepository repository = new InMemoryStudentRepository();
        Student student = new Student("Анна");
        repository.Add(student);

        Assert.Throws<InvalidOperationException>(() => repository.Add(student));
        repository.Add(new Student("Анна"));
        Assert.Equal(2, repository.GetAll().Count);
        Assert.Same(student, repository.GetById(student.Id));
    }

    [Fact]
    public void RubricRejectsDuplicateCriterionIdentity()
    {
        Rubric rubric = new Rubric();
        Criterion criterion = new Criterion("ДЗ", 10, 1);
        rubric.Add(criterion);

        Assert.Throws<InvalidOperationException>(() => rubric.Add(criterion));
        Assert.Single(rubric.Criteria);
    }
}
