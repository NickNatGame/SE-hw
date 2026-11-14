namespace Pos.Application;

public class FinalScoreResult
{
    public Guid StudentId { get; }
    public string StudentName { get; }
    public decimal Score { get; }

    public FinalScoreResult(Guid studentId, string studentName, decimal score)
    {
        StudentId = studentId;
        StudentName = studentName;
        Score = score;
    }
}
