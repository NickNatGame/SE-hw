using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

var students = new InMemoryStudentRepository();
var rubrics = new InMemoryRubricRepository();
var strategy = new WeightedScoreStrategy();
var calculator = new FinalScoreCalculator(strategy);
var grading = new GradingService(students, rubrics, calculator);

Student student = grading.AddStudent("Анна Иванова");
Criterion homework = grading.AddCriterion("Домашние задания", maxScore: 10, weight: 70);
Criterion project = grading.AddCriterion("Проект", maxScore: 20, weight: 30);

grading.AddEvaluation(student.Id, homework.Id, 8);
grading.AddEvaluation(student.Id, project.Id, 15);

Console.WriteLine($"Студент: {student.Name}");
foreach (Evaluation evaluation in grading.GetEvaluations(student.Id))
{
    Criterion criterion = grading.GetCriterion(evaluation.CriterionId);
    Console.WriteLine($"{criterion.Name}: {evaluation.Score}/{criterion.MaxScore} (вес: {criterion.Weight})");
}
decimal result = grading.CalculateFinalScore(student.Id);
Console.WriteLine($"Итог: {result:0.##}/10");

grading.CorrectEvaluation(student.Id, project.Id, 20);
result = grading.CalculateFinalScore(student.Id);
Console.WriteLine($"После исправления проекта: {result:0.##}/10");

grading.CancelEvaluation(student.Id, project.Id);
try
{
    result = grading.CalculateFinalScore(student.Id);
    Console.WriteLine($"Итог: {result:0.##}/10");
}
catch (InvalidOperationException exception)
{
    Console.WriteLine(exception.Message);
}

grading.AddEvaluation(student.Id, project.Id, 15);
result = grading.CalculateFinalScore(student.Id);
Console.WriteLine($"После повторного выставления проекта: {result:0.##}/10");

var averageCalculator = new FinalScoreCalculator(new AverageScoreStrategy());
var averageGrading = new GradingService(students, rubrics, averageCalculator);

var roundedCalculator = new RoundedFinalScoreCalculator(averageCalculator);
var roundedGrading = new GradingService(students, rubrics, roundedCalculator);

grading.CorrectEvaluation(student.Id, project.Id, 15.123m);
decimal averageResult = averageGrading.CalculateFinalScore(student.Id);
decimal roundedResult = roundedGrading.CalculateFinalScore(student.Id);
Console.WriteLine($"Среднее долей выполнения без округления: {averageResult}/10");
Console.WriteLine($"Среднее долей выполнения с округлением: {roundedResult}/10");
