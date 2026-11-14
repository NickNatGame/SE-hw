using System.Globalization;
using Pos.Application;
using Pos.Domain;

namespace Pos.Presentation;

public class ConsoleApplication
{
    private readonly GradingService _grading;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleApplication(GradingService grading, TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(grading);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        _grading = grading;
        _input = input;
        _output = output;
    }

    public void Run()
    {
        while (true)
        {
            try
            {
                _output.WriteLine("\n1 - добавить студента, 2 - список студентов");
                _output.WriteLine("3 - добавить критерий, 4 - список критериев");
                _output.WriteLine("5 - выставить оценку, 6 - посмотреть оценки");
                _output.WriteLine("7 - исправить оценку, 8 - отменить оценку");
                _output.WriteLine("9 - итог студента, 10 - экспорт всех итогов в CSV");
                _output.WriteLine("0 - выход");
                int choice = ReadInt("Команда: ", 0, 10);
                switch (choice)
                {
                    case 0:
                        return;
                    case 1:
                        AddStudent();
                        break;
                    case 2:
                        ShowStudents();
                        break;
                    case 3:
                        AddCriterion();
                        break;
                    case 4:
                        ShowCriteria();
                        break;
                    case 5:
                        ChangeEvaluation(false);
                        break;
                    case 6:
                        ShowEvaluations();
                        break;
                    case 7:
                        ChangeEvaluation(true);
                        break;
                    case 8:
                        CancelEvaluation();
                        break;
                    case 9:
                        ShowFinalScore();
                        break;
                    case 10:
                        string path = ReadText("Путь нового CSV-файла: ");
                        _grading.ExportFinalScores(path);
                        _output.WriteLine($"Итоги экспортированы: {Path.GetFullPath(path)}");
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                _output.WriteLine("\nВвод завершён.");
                return;
            }
            catch (ArgumentException exception)
            {
                _output.WriteLine($"Ошибка: {exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                _output.WriteLine($"Ошибка: {exception.Message}");
            }
            catch (IOException exception)
            {
                _output.WriteLine($"Ошибка файла: {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                _output.WriteLine($"Нет доступа к файлу: {exception.Message}");
            }
            catch (NotSupportedException exception)
            {
                _output.WriteLine($"Недопустимый путь: {exception.Message}");
            }
            catch (OverflowException)
            {
                _output.WriteLine("Числа слишком велики для расчёта. Используйте меньшие баллы и веса.");
            }
        }
    }

    private void AddStudent()
    {
        string name = ReadText("Имя студента: ");
        Student student = _grading.AddStudent(name);
        _output.WriteLine($"Студент добавлен: {student.Name}");
    }

    private void AddCriterion()
    {
        string name = ReadText("Название критерия: ");
        decimal maximum = ReadDecimal("Максимальный балл: ");
        decimal weight = ReadDecimal("Вес: ");
        Criterion criterion = _grading.AddCriterion(name, maximum, weight);
        _output.WriteLine($"Критерий добавлен: {criterion.Name}");
    }

    private void ShowStudents()
    {
        IReadOnlyList<Student> students = _grading.GetStudents();
        if (students.Count == 0)
        {
            _output.WriteLine("Студентов пока нет.");
        }

        for (int i = 0; i < students.Count; i++)
        {
            _output.WriteLine($"{i + 1}. {students[i].Name} (ID: {students[i].Id})");
        }
    }

    private void ShowCriteria()
    {
        IReadOnlyList<Criterion> criteria = _grading.GetCriteria();
        if (criteria.Count == 0)
        {
            _output.WriteLine("Критериев пока нет.");
        }

        for (int i = 0; i < criteria.Count; i++)
        {
            Criterion criterion = criteria[i];
            _output.WriteLine($"{i + 1}. {criterion.Name}: максимум {criterion.MaxScore}, вес {criterion.Weight}");
        }
    }

    private Student SelectStudent()
    {
        IReadOnlyList<Student> students = _grading.GetStudents();
        if (students.Count == 0)
        {
            throw new InvalidOperationException("Сначала добавьте студента.");
        }

        ShowStudents();
        int number = ReadInt("Номер студента: ", 1, students.Count);
        return students[number - 1];
    }

    private Criterion SelectCriterion()
    {
        IReadOnlyList<Criterion> criteria = _grading.GetCriteria();
        if (criteria.Count == 0)
        {
            throw new InvalidOperationException("Сначала добавьте критерий.");
        }

        ShowCriteria();
        int number = ReadInt("Номер критерия: ", 1, criteria.Count);
        return criteria[number - 1];
    }

    private void ChangeEvaluation(bool correct)
    {
        Student student = SelectStudent();
        Criterion criterion = SelectCriterion();
        decimal score = ReadDecimal("Балл: ");
        if (correct)
        {
            _grading.CorrectEvaluation(student.Id, criterion.Id, score);
            _output.WriteLine("Оценка исправлена.");
        }
        else
        {
            _grading.AddEvaluation(student.Id, criterion.Id, score);
            _output.WriteLine("Оценка выставлена.");
        }
    }

    private void CancelEvaluation()
    {
        Student student = SelectStudent();
        Criterion criterion = SelectCriterion();
        _grading.CancelEvaluation(student.Id, criterion.Id);
        _output.WriteLine("Оценка отменена. Итог недоступен до повторного выставления.");
    }

    private void ShowEvaluations()
    {
        Student student = SelectStudent();
        IReadOnlyList<Evaluation> evaluations = _grading.GetEvaluations(student.Id);
        if (evaluations.Count == 0)
        {
            _output.WriteLine("Оценок пока нет.");
        }

        foreach (Evaluation evaluation in evaluations)
        {
            Criterion criterion = _grading.GetCriterion(evaluation.CriterionId);
            _output.WriteLine($"{criterion.Name}: {evaluation.Score}/{criterion.MaxScore}");
        }
    }

    private void ShowFinalScore()
    {
        Student student = SelectStudent();
        decimal result = _grading.CalculateFinalScore(student.Id);
        _output.WriteLine($"Итог {student.Name}: {result:G29}/10");
    }

    private string ReadText(string prompt)
    {
        while (true)
        {
            _output.Write(prompt);
            string? text = _input.ReadLine();
            if (text == null)
            {
                throw new EndOfStreamException();
            }

            text = text.Trim();
            if (text.Length > 0)
            {
                return text;
            }

            _output.WriteLine("Введите непустое значение.");
        }
    }

    private int ReadInt(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            string text = ReadText(prompt);
            if (int.TryParse(text, out int value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            _output.WriteLine($"Введите целое число от {minimum} до {maximum}.");
        }
    }

    private decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            string text = ReadText(prompt).Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }

            _output.WriteLine("Введите число, например 8,5 или 8.5.");
        }
    }
}
