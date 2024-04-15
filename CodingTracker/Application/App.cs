using CodingTracker.Database;
using CodingTracker.Services;
using CodingTracker.Util;
using Spectre.Console;

namespace CodingTracker.Application;

/// <summary>
/// App manages the main application loop and user interactions for the Coding Tracker console application.
/// It orchestrates the flow of the application based on user input and coordinates between different parts of the application.
/// </summary>
public class App
{
    private SessionService _sessionService;
    private AppNewLogManager _newLogManager;
    private AppSessionManager _sessionManager;
    private AppGoalManager _goalManager;
    private AppReportManager _reportManager;
    private DatabaseContext _dbContext;


    /// <summary>
    /// Initializes a new instance of App with the necessary services to manage coding sessions, goals, and reports.
    /// </summary>
    /// <param name="sessionService">Service to manage session-related operations.</param>
    public App()
    {
        // Setup database
        _dbContext = new DatabaseContext();
        DatabaseInitializer dbInitializer = new DatabaseInitializer(_dbContext);
        dbInitializer.Initialize();

        // Initialize services
        _sessionService = new SessionService(_dbContext);
        _newLogManager = new AppNewLogManager(_sessionService);
        _sessionManager = new AppSessionManager(_sessionService);  
        _goalManager = new AppGoalManager();  
        _reportManager = new AppReportManager();
        
    }

    /// <summary>
    /// Starts the main execution loop of the application. This method continuously displays the main menu and processes user input
    /// until the user decides to exit the application.
    /// </summary>
    public void Run()
    {
        bool running = true;
        while (running)
        {
            AnsiConsole.Clear();
            AnsiConsole.Markup("[underline green]Welcome to Coding Tracker![/]\n");
            MainMenuOption selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<MainMenuOption>()
                    .Title("What would you like to do?")
                    .PageSize(10)
                    .AddChoices(Enum.GetValues<MainMenuOption>())
            );

            switch (selectedOption)
            {
                case MainMenuOption.StartNewSession:
                    _sessionManager.Run();
                    break;
                case MainMenuOption.LogManualSession:
                    _newLogManager.Run();
                    break;
                case MainMenuOption.ViewandEditPreviousSessions:
                    _sessionManager.Run();
                    break;
                case MainMenuOption.ViewAndEditGoals:
                    _goalManager.Run();
                    break;
                case MainMenuOption.ViewReports:
                    _reportManager.Run();
                    break;
                case MainMenuOption.SeedDatabase:
                    AppUtil.SeedDatabase(_sessionService);
                    break;
                case MainMenuOption.Exit:
                    running = false;
                    AnsiConsole.Markup("[grey]Goodbye![/]");
                    break;
                default:
                    AnsiConsole.Markup("[red]Invalid option selected.[/]");
                    break;
            }
        }
    }
}
