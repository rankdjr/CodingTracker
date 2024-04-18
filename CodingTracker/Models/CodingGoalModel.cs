using CodingTracker.Services;

namespace CodingTracker.Models;

public class CodingGoalModel
{
    public int Id { get; set; }
    public string DateCreated { get; set; } = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    public string? DateCompleted { get; set; }
    public string TargetDuration { get; set; }
    public string CurrentProgress { get; set; } = TimeSpan.Zero.ToString(ConfigSettings.TimeFormatType);
    public bool IsCompleted { get; set; }

    public enum GoalProperties
    {
        Id,
        DateCreated,
        DateCompleted,
        TargetDuration,
        CurrentProgress,
        IsCompleted
    }

    public enum EditableProperties
    {
        TargetDuration
    }

    public CodingGoalModel(TimeSpan targetDuration) 
    {
        TargetDuration = targetDuration.ToString(ConfigSettings.TimeFormatType);
    }

    public void UpdateProgress(TimeSpan sessionDuration)
    {
        var newDuration = TimeSpan.Parse(CurrentProgress) + sessionDuration;
        CurrentProgress = newDuration.ToString(ConfigSettings.TimeFormatType);
    }

    public void SetDateCompleted(DateTime date)
    {
        DateCompleted = date.ToString(ConfigSettings.DateFormatLong);
    }
}

