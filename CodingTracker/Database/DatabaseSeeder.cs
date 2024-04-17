using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Database;

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


            Utilities.DisplaySuccessMessage("Database seeded successfully!");
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error seeding database", ex.Message);
        }
        finally
        {
            _inputHandler.PauseForContinueInput();
        }
    }
}
