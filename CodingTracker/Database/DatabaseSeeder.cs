using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Database;

/// <summary>
/// DatabaseSeeder contains methods to seed the database with initial data for coding sessions.
/// It utilizes random data generation for dates and durations to provide a variety of sample data.
/// </summary>
public class DatabaseSeeder
{
    private static Random _random = new Random();
    private InputHandler _inputHandler;
    private CodingSessionDAO _codingSessionDAO;

    public DatabaseSeeder(CodingSessionDAO codingSessionDAO, InputHandler inputHandler)
    {
        // Using the current time to generate a seed
        int seed = (int)DateTime.Now.Ticks;
        _random = new Random(seed);
        _inputHandler = inputHandler;
        _codingSessionDAO = codingSessionDAO;
    }

    /// <summary>
    /// Seeds the database with log entries for existing habits, creating multiple logs per habit.
    /// </summary>
    /// <param name="numOfSessions">Number of session log entries to create.</param>
    public void SeedSessions(int numOfSessions)
    {
        for (int i = 0; i < numOfSessions / 2; i++)
        {
            int hours = _random.Next(0, 12); // Random value limited to 12 hours
            int minutes = _random.Next(0, 60); // Random value limited to 59 minutes
            TimeSpan duration = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            DateTime sessionDate = DateTime.Today.AddDays(-_random.Next(1, 30));

            CodingSessionModel newSession = new CodingSessionModel(sessionDate, duration);
            _codingSessionDAO.InsertNewSession(newSession);
        }

        for (int i = 0; i < numOfSessions / 2; i++)
        {
            DateTime endTime = DateTime.Today.AddDays(-_random.Next(1, 30));
            DateTime startTime = endTime.AddMinutes(-_random.Next(1, 180));

            CodingSessionModel newSession = new CodingSessionModel(startTime, endTime);
            _codingSessionDAO.InsertNewSession(newSession);
        }
    }

    /// <summary>
    /// Seeds the application's database with initial data for habits and log entries.
    /// This method is used prepopulate the database with test data.
    /// </summary>
    public void SeedDatabase()
    {
        AnsiConsole.WriteLine("Starting database seeding...");

        try
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("Processing...", ctx =>
                {
                    // Call to seed sessions
                    SeedSessions(ConfigSettings.NumberOfCodingSessionsToSeed);
                });

            AnsiConsole.Write(new Markup("\n[green]Database seeded successfully![/]\n"));
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Markup($"\n[red]Error seeding database: {ex.Message}[/]\n"));
        }
        finally
        {
            _inputHandler.PauseForContinueInput();
        }
    }
}
