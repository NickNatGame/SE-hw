namespace Pos.Domain;

public class Student
{
    private readonly List<Evaluation> _evaluations = new List<Evaluation>();
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }

    public IReadOnlyList<Evaluation> Evaluations
    {
        get
        {
            return _evaluations.AsReadOnly();
        }
    }

    public Student(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Имя студента не может быть пустым.", nameof(name));
        }

        Name = name.Trim();
    }

    public void AddEvaluation(Rubric rubric, Guid criterionId, decimal score)
    {
        ArgumentNullException.ThrowIfNull(rubric);

        foreach (Evaluation evaluation in _evaluations)
        {
            if (evaluation.CriterionId == criterionId)
            {
                throw new InvalidOperationException("Оценка уже выставлена. Используйте исправление.");
            }
        }

        Evaluation newEvaluation = new Evaluation(this, rubric, criterionId, score);
        _evaluations.Add(newEvaluation);
    }

    public void CorrectEvaluation(Rubric rubric, Guid criterionId, decimal score)
    {
        ArgumentNullException.ThrowIfNull(rubric);

        int index = GetEvaluationIndex(criterionId);
        Evaluation correctedEvaluation = new Evaluation(this, rubric, criterionId, score);
        _evaluations[index] = correctedEvaluation;
    }

    public void CancelEvaluation(Guid criterionId)
    {
        int index = GetEvaluationIndex(criterionId);
        _evaluations.RemoveAt(index);
    }

    private int GetEvaluationIndex(Guid criterionId)
    {
        for (int i = 0; i < _evaluations.Count; i++)
        {
            if (_evaluations[i].CriterionId == criterionId)
            {
                return i;
            }
        }

        throw new ArgumentException("Оценка по этому критерию не выставлена.", nameof(criterionId));
    }
}
