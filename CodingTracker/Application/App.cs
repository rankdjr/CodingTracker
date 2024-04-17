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
    private bool _running = false;

    public App()
    {
        _running = true;

        // Setup database
        _dbContext = new DatabaseContext();
        DatabaseInitializer dbInitializer = new DatabaseInitializer(_dbContext);
        dbInitializer.Initialize();

        // Initialize services
        _inputHandler = new InputHandler();
        _codingSessionDAO = new CodingSessionDAO(_dbContext);
        _dbSeeder = new DatabaseSeeder(_codingSessionDAO, _inputHandler);

        // Setup child applications
        _appStopwatchManager = new AppStopwatchManager(_codingSessionDAO, _inputHandler);
        _newLogManager = new AppNewLogManager(_codingSessionDAO, _inputHandler);
        _sessionManager = new AppSessionManager(_codingSessionDAO, _inputHandler);
        _goalManager = new AppGoalManager();  
        _reportManager = new AppReportManager(_codingSessionDAO, _inputHandler);
    }

    public void Run()
    {
        while (_running)
        {
            AnsiConsole.Clear();
            displayMainScreenBanner();
            PromptForSessionAction();            
        }
    }

    private void displayMainScreenBanner()
    {
        AnsiConsole.Write(
            new FigletText("Coding Tracker")
                .LeftJustified()
                .Color(Color.SeaGreen1_1));

        Utilities.PrintNewLines(2);
    }

    private void PromptForSessionAction()
    {
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select an option:")
                .PageSize(10)
                .AddChoices(Enum.GetNames(typeof(MainMenuOption))
                .Select(Utilities.SplitCamelCase)));

        ExecuteSelectedOption(selectedOption);
    }

    private void ExecuteSelectedOption(string option)
    {
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
                _dbSeeder.SeedDatabase();
                break;
            case MainMenuOption.Exit:
                closeSession();
                break;
        }
    }

    private void closeSession()
    {
        _running = false;
        AnsiConsole.Markup("[teal]Goodbye![/]");
        Utilities.PrintNewLines(2);
    }
}
