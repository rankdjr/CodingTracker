using CodingTracker.DAO;
using CodingTracker.Database;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Application;

/// <summary>
/// App manages the main application loop and user interactions for the Coding Tracker console application.
/// It orchestrates the flow of the application based on user input and coordinates between different parts of the application.
/// </summary>
public class App
{
    private AppStopwatchManager _appStopwatchManager;
    private AppNewLogManager _newLogManager;
    private AppSessionManager _sessionManager;
    private AppGoalManager _goalManager;
    private AppReportManager _reportManager;
    private DatabaseContext _dbContext;
    private DatabaseSeeder _dbSeeder;
    private CodingSessionDAO _codingSessionDAO;
    private InputHandler _inputHandler;


    /// <summary>
    /// Initializes a new instance of App with the necessary services to manage coding sessions, goals, and reports.
    /// </summary>
    /// <param name="sessionService">Service to manage session-related operations.</param>
    public App()
    {
        // Setup database
        _dbContext = new DatabaseContext();
        _dbSeeder = new DatabaseSeeder();
        _codingSessionDAO = new CodingSessionDAO(_dbContext);
        DatabaseInitializer dbInitializer = new DatabaseInitializer(_dbContext);
        dbInitializer.Initialize();

        // Initialize services
        _inputHandler = new InputHandler();
        _appStopwatchManager = new AppStopwatchManager(_codingSessionDAO, _inputHandler);
        _newLogManager = new AppNewLogManager(_codingSessionDAO, _inputHandler);
        _sessionManager = new AppSessionManager(_codingSessionDAO, _inputHandler);
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
            Utilities.AnsiWriteLine(new Markup("[underline green]Select an option[/]\n"));
            var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10)
                .AddChoices(Enum.GetNames(typeof(MainMenuOption))
                .Select(Utilities.SplitCamelCase)));

            switch (Enum.Parse<MainMenuOption>(option.Replace(" ", "")))
            {
                case MainMenuOption.StartNewSession:
                    _appStopwatchManager.Run();
                    break;
                case MainMenuOption.LogManualSession:
                    _newLogManager.Run();
                    break;
                case MainMenuOption.ViewAndEditPreviousSessions:
                    _sessionManager.Run();
                    break;
                case MainMenuOption.ViewAndEditGoals:
                    _goalManager.Run();
                    break;
                case MainMenuOption.ViewReports:
                    _reportManager.Run();
                    break;
                case MainMenuOption.SeedDatabase:
                    _dbSeeder.SeedDatabase(_codingSessionDAO);
                    break;
                case MainMenuOption.Exit:
                    running = false;
                    AnsiConsole.Markup("[grey]Goodbye![/]");
                    Utilities.PrintNewLines(2);
                    break;
                default:
                    AnsiConsole.Markup("[red]Invalid option selected.[/]");
                    break;
            }
        }
    }
}
