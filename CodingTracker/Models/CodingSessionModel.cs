using CodingTracker.Services;
using System.Security.Cryptography;

namespace CodingTracker.Models;

/// <summary>
/// Represents a coding session, allowing tracking of session duration on a specified date.
/// </summary>
public class CodingSessionModel
{
    public int? Id { get; set; }
    public string DateCreated { get; set; } = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    public string DateUpdated { get; set; } = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    public string SessionDate { get; set; }= DateTime.UtcNow.ToString(ConfigSettings.DateFormatShort);
    public string Duration { get; set; } = TimeSpan.Zero.ToString(ConfigSettings.TimeFormatType);
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public enum EditableProperties
    {
        SessionDate,
        Duration,
        StartTime,
        EndTime
    }

    public CodingSessionModel() { }

    /// <summary>
    /// Initializes a new instance of the CodingSession class with a specified session date and duration.
    /// </summary>
    /// <param name="sessionDate">The date on which the session was held.</param>
    /// <param name="duration">The duration of the session in hours.</param>
    public CodingSessionModel(DateTime sessionDate, TimeSpan duration)
    {
        SessionDate = sessionDate.ToString(ConfigSettings.DateFormatShort);
        Duration = duration.ToString(ConfigSettings.TimeFormatType);
        DateCreated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }

    /// <summary>
    /// Initializes a new instance of the CodingSession class with a specified session date, start time, and end time.
    /// Automatically calculates the session duration.
    /// </summary>
    /// <param name="sessionDate">The date on which the session was held.</param>
    /// <param name="startTime">Start time of the session.</param>
    /// <param name="endTime">End time of the session.</param>
    public CodingSessionModel(DateTime startTime, DateTime endTime)
    {
        SessionDate = startTime.ToString(ConfigSettings.DateFormatShort);
        StartTime = startTime.ToString(ConfigSettings.DateFormatLong);
        EndTime = endTime.ToString(ConfigSettings.DateFormatLong);
        Duration = (endTime - startTime).ToString(ConfigSettings.TimeFormatType);  // Calculate duration based on start and end times
        DateCreated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }

    /// <summary>
    /// Updates the duration of the coding session.
    /// </summary>
    /// <param name="newDuration">New duration in hours.</param>
    public void SetDuration(TimeSpan newDuration)
    {
        Duration = newDuration.ToString(ConfigSettings.TimeFormatType);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }

    /// <summary>
    /// Updates the duration of the coding session.
    /// </summary>
    /// <param name="newDuration">New duration in hours.</param>
    public void SetDuration(DateTime startTime, DateTime endTime)
    {
        TimeSpan elapsedTime = endTime - startTime;
        Duration = elapsedTime.ToString(ConfigSettings.TimeFormatType);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }

    /// <summary>
    /// Sets the session date property to a formatted string based on the provided DateTime value.
    /// Also updates the 'DateUpdated' property to the current UTC date and time.
    /// </summary>
    /// <param name="date">The DateTime to be formatted and set as the session date.</param>
    public void SetSessionDate(DateTime date)
    {
        SessionDate = date.ToString(ConfigSettings.DateFormatShort);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }

    /// <summary>
    /// Sets the start time property to a formatted string based on the provided DateTime value.
    /// Also updates the 'DateUpdated' property to the current UTC date and time.
    /// </summary>
    /// <param name="date">The DateTime to be formatted and set as the start time.</param>
    public void SetStartTime(DateTime date)
    {
        SessionDate = date.ToString(ConfigSettings.DateFormatLong);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }

    /// <summary>
    /// Sets the end time property to a formatted string based on the provided DateTime value.
    /// Also updates the 'DateUpdated' property to the current UTC date and time.
    /// </summary>
    /// <param name="date">The DateTime to be formatted and set as the end time.</param>

    public void SetEndTime(DateTime date)
    {
        SessionDate = date.ToString(ConfigSettings.DateFormatLong);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }
}
