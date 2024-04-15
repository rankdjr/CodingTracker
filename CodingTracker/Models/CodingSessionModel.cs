﻿using CodingTracker.Services;

namespace CodingTracker.Models;

/// <summary>
/// Represents a coding session, allowing tracking of session duration on a specified date.
/// </summary>
public class CodingSessionModel
{
    public int? SessionId { get; set; }
    public string DateCreated { get; set; }
    public string DateUpdated { get; set; }
    public string SessionDate { get; set; }
    public string Duration { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }

    /// <summary>
    /// Initializes a new instance of the CodingSession class with a specified session date and duration.
    /// </summary>
    /// <param name="sessionDate">The date on which the session was held.</param>
    /// <param name="duration">The duration of the session in hours.</param>
    public CodingSessionModel(DateTime sessionDate, TimeSpan duration)
    {
        SessionDate = sessionDate.ToString(ConfigSettings.DateFormatShort);
        Duration = duration.ToString(ConfigSettings.TimeFormat);
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
        Duration = (endTime - startTime).ToString(ConfigSettings.TimeFormat);  // Calculate duration based on start and end times
        DateCreated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);
    }

    /// <summary>
    /// Updates the duration of the coding session.
    /// </summary>
    /// <param name="newDuration">New duration in hours.</param>
    public void UpdateDuration(TimeSpan newDuration)
    {
        Duration = newDuration.ToString(ConfigSettings.TimeFormat);
        DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong);  // Reflects the latest update time
    }
}
