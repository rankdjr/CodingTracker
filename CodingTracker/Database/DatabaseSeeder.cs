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
        int seed = (int)DateTime.Now.Ticks;
        _random = new Random(seed);
        _inputHandler = inputHandler;
        _codingSessionDAO = codingSessionDAO;
    }

    public void SeedSessions(int numOfSessions)
    {
        // number of seeds is in App.config

        int lowerBoundarySessionDuration = 60; // 60 minutes loweer boundary for duration
        int upperBoundarySessionDuration = 240; // 4 hour upper boundary for duration
        int lowerBoundarySeedDate = Utilities.GetDaysMultiplier(TimePeriod.Years) * 3; // 3 year lower seed boundary

        for (int i = 0; i < numOfSessions; i++)
        {
            DateTime endTime = DateTime.Today.AddDays(-_random.Next(1, lowerBoundarySeedDate));
            DateTime startTime = endTime.AddMinutes(-_random.Next(lowerBoundarySessionDuration, upperBoundarySessionDuration));

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
