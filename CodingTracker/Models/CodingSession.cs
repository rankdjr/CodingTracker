namespace CodingTracker.Models;

/// <summary>
/// Represents a coding session with start and end times, and provides methods to manage session timing.
/// </summary>
public class CodingSession
{
    public int? SessionId { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public DateTime? SessionStartTime { get; set; }
    public DateTime? SessionEndTime { get; set; }
    public float Duration{ get; set; }

    /// <summary>
    /// Starts the session by setting the current UTC time as the start time and updates the session.
    /// </summary>
    public void StartSession()
    {
        SessionStartTime = DateTime.UtcNow;
        DateUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Ends the session if it has started, setting the current UTC time as the end time, updating the session, and calculating the session length.
    /// </summary>
    public void EndSession()
    {
        if (SessionStartTime.HasValue)
        {
            SessionEndTime = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow; 
            UpdateSessionLength(); 
        }
    }

    /// <summary>
    /// Calculates and updates the length of the session based on the start and end times.
    /// </summary>
    private void UpdateSessionLength()
    {
        if (SessionStartTime.HasValue && SessionEndTime.HasValue)
        {
            TimeSpan duration = SessionEndTime.Value - SessionStartTime.Value;
            Duration = (float)duration.TotalHours;
        }
    }
}
