﻿using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Database;

public class DatabaseSeeder
{
    private static Random _random = new Random();
    private InputHandler _inputHandler;
    private CodingSessionDAO _codingSessionDAO;
    private CodingGoalDAO _codingGoalDAO;

    public DatabaseSeeder(CodingSessionDAO codingSessionDAO, CodingGoalDAO codingGoalDAO, InputHandler inputHandler)
    {
        int seed = (int)DateTime.Now.Ticks;
        _random = new Random(seed);
        _inputHandler = inputHandler;
        _codingSessionDAO = codingSessionDAO;
        _codingGoalDAO = codingGoalDAO;
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

            // TODO: Implement CodingActivityManager to insert session activities and update goal progress; return boolean instead of ID

            _codingSessionDAO.InsertNewSession(newSession);
        }
    }

    public void SeedGoals(int numOfGoals)
    {
        TimeSpan lowerBoundaryGoalDuration = new TimeSpan(0, 30, 0); // 30 minutes lower boundary for duration
        TimeSpan upperBoundaryGoalDuration = new TimeSpan(100, 0, 0); // 100 hour upper boundary for duration

        for (int i = 0; i < numOfGoals; i++)
        {
            TimeSpan duration = new TimeSpan(0, _random.Next(lowerBoundaryGoalDuration.Minutes, upperBoundaryGoalDuration.Minutes), 0);

            CodingGoalModel newGoal = new CodingGoalModel(duration);
            _codingGoalDAO.InsertNewGoal(newGoal);
        }

        // Create a master goal with a duration of 10,000 hours
        TimeSpan masterDuration = new TimeSpan(10000, 0, 0);
        CodingGoalModel masterGoal = new CodingGoalModel(masterDuration);
        _codingGoalDAO.InsertNewGoal(masterGoal);
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
                    SeedGoals(ConfigSettings.NumberOfCodingGoalsToSeed);
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
