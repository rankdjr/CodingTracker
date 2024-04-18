namespace CodingTracker.Models;

public class CodingGoal
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

