using Pos.Domain;

// Пока это только демонстрация: данные создаются в коде и хранятся в памяти.
var student = new Student("Анна Иванова");
var homework = new Criterion("Домашние задания", maxScore: 10, weight: 70);
var project = new Criterion("Проект", maxScore: 20, weight: 30);

var rubric = new Rubric();
rubric.Add(homework);
rubric.Add(project);

var evaluations = new List<Evaluation>
{
    new(student, rubric, homework.Id, 8),
    new(student, rubric, project.Id, 15)
};

var calculator = new FinalScoreCalculator();
decimal result = calculator.Calculate(student, rubric, evaluations);

Console.WriteLine($"Студент: {student.Name}");
foreach (Evaluation evaluation in evaluations)
{
    Criterion criterion = rubric.GetById(evaluation.CriterionId);
    Console.WriteLine($"{criterion.Name}: {evaluation.Score}/{criterion.MaxScore} (вес: {criterion.Weight})");
}
Console.WriteLine($"Итог: {result:0.##}/10");
