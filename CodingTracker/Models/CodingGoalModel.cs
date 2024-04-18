namespace CodingTracker.Models;

public class CodingGoalModel
{
    public int Id { get; set; }
    public TimeSpan TargetDuration { get; set; }
    public TimeSpan CurrentProgress { get; set; }
    public bool IsCompleted { get; set; }

    public void UpdateProgress(TimeSpan sessionDuration)
    {
        CurrentProgress += sessionDuration;
        IsCompleted = CurrentProgress >= TargetDuration;
    }
}

