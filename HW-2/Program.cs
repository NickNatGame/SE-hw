using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

var students = new InMemoryStudentRepository();
var rubrics = new InMemoryRubricRepository();
var calculator = new FinalScoreCalculator();
var grading = new GradingService(students, rubrics, calculator);

Student student = grading.AddStudent("Анна Иванова");
Criterion homework = grading.AddCriterion("Домашние задания", maxScore: 10, weight: 70);
Criterion project = grading.AddCriterion("Проект", maxScore: 20, weight: 30);

grading.AddEvaluation(student.Id, homework.Id, 8);
grading.AddEvaluation(student.Id, project.Id, 15);

Console.WriteLine($"Студент: {student.Name}");
foreach (Evaluation evaluation in grading.GetEvaluations(student.Id))
{
    Criterion criterion = rubrics.Get().GetById(evaluation.CriterionId);
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
