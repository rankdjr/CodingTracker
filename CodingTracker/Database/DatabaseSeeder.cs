using CodingTracker.Models;
using CodingTracker.Services;
using System.Collections.Generic;

namespace CodingTracker.Database;

/// <summary>
/// DatabaseSeeder contains methods to seed the database with initial data for coding sessions.
/// It utilizes random data generation for dates and durations to provide a variety of sample data.
/// </summary>
public class DatabaseSeeder
{
    private static Random _random = new Random();

    static DatabaseSeeder()
    {
        // Using the current time to generate a seed
        int seed = (int)DateTime.Now.Ticks;
        _random = new Random(seed);
    }

    /// <summary>
    /// Seeds the database with log entries for existing habits, creating multiple logs per habit.
    /// </summary>
    /// <param name="sessionService">Service used to interact with the session logs table.</param>
    /// <param name="numOfSessions">Number of session log entries to create.</param>
    public static void SeedSessions(SessionService sessionService, int numOfSessions)
    {
        for (int i = 0; i < numOfSessions % 2; i++)
        {
            int hours = _random.Next(0, 12); // Random value limited to 12 hours
            int minutes = _random.Next(0, 60); // Random value limited to 59 minutes
            TimeSpan duration = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            DateTime sessionDate = DateTime.Today.AddDays(-_random.Next(1, 30));

            CodingSessionModel newSession = new CodingSessionModel(sessionDate, duration);
            sessionService.InsertNewSession(newSession);  
        }

        for (int i = 0; i < numOfSessions % 2; i++)
        {
            DateTime endTime = DateTime.Today.AddDays(-_random.Next(1, 30));
            DateTime startTime = endTime.AddMinutes(-_random.Next(1,180));
            
            CodingSessionModel newSession = new CodingSessionModel(startTime, endTime);
            sessionService.InsertNewSession(newSession);
        }
    }
}
