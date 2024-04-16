using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;
using System.Reflection.PortableExecutable;

namespace CodingTracker.Application;

public class AppStopwatchManager
{
    private readonly CodingSessionDAO _codingSessionDAO;
    private Utilities _utilities;
    private InputHandler _inputHandler;
    private CodingSessionModel? _sessionModel;
    private bool _stopwatchRunning;
    private DateTime _startTime;
    

    /// /// <summary>
    /// Runs the main application loop, providing a user interface for managing coding sessions.
    /// </summary>
    /// <param name="codingSessionDAO">The service for tracking current Coding Sessions.</param>
    /// <remarks>
    /// The method continuously displays a menu that adapts based on whether a session is currently active. If a session is active,
    /// options are provided for viewing the elapsed time and ending the session. If no session is active, options are provided to start a new session or exit.
    /// </remarks>
    public AppStopwatchManager(CodingSessionDAO codingSessionDAO)
    {
        _codingSessionDAO = codingSessionDAO;
        _utilities = new Utilities();
        _inputHandler = new InputHandler();
        _stopwatchRunning = false;
    }

    /// <summary>
    /// Runs the main application loop, providing a user interface for managing coding sessions.
    /// </summary>
    /// <remarks>
    /// The method continuously displays a dynamic menu that adapts based on whether a session is currently active.
    /// When a session is active, it shows session statistics and provides options to refresh elapsed time or end the session.
    /// When no session is active, it provides options to start a new session or return to the main menu.
    /// </remarks>
    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            List<StartSessionMenuOptions> options = new List<StartSessionMenuOptions>();

            if (_stopwatchRunning)
            {
                // Display current session info
                DateTime currentTime = DateTime.Now;
                TimeSpan elapsedTime = currentTime - _startTime;
                
                string panelSessionTitle = $"\n[bold]Current Session:[/] \n\n";
                string panelStartTime = $"[underline]Start Time[/]:       [royalblue1]{_startTime.ToString(ConfigSettings.DateFormatLong)}[/]\n";
                string panelElapsedTime = $"[underline]Elapsed Time[/]:     [steelblue1]{elapsedTime.ToString(@"hh\:mm\:ss")}[/]\n\n";
                string panelLastUpdated = $"Last Updated at [darkgoldenrod]  {currentTime.ToString(ConfigSettings.DateFormatLong)}[/]";

                string panelInformation = panelSessionTitle + panelStartTime + panelElapsedTime + panelLastUpdated;
                var panel = new Panel(new Markup(panelInformation));
                panel.Header = new PanelHeader("[mediumspringgreen]Session Running[/]", Justify.Center);
                panel.Padding = new Padding(2, 2, 2, 2);
                panel.Border = BoxBorder.Rounded;
                panel.Expand = true;
                AnsiConsole.Write(panel);

                _utilities.PrintNewLines(3);

                // Options available during an active session
                options.Add(StartSessionMenuOptions.RefreshElapsedTime);
                options.Add(StartSessionMenuOptions.EndCurrentSession);
            }
            else
            {
                // Options available when no session is active
                options.Add(StartSessionMenuOptions.StartSession);
                options.Add(StartSessionMenuOptions.ReturnToMainMenu);
            }

            _utilities.AnsiWriteLine(new Markup("[underline green]Select an option:[/]\n"));
            StartSessionMenuOptions selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<StartSessionMenuOptions>()
                    .Title("Start and Stop New Coding Session")
                    .PageSize(10)
                    .AddChoices(options)
                    .UseConverter(selectedOption => _utilities.SplitCamelCase(selectedOption.ToString())));  // Using SplitCamelCase to format enum values

            switch (selectedOption)
            {
                case StartSessionMenuOptions.StartSession:
                    StartSession();
                    break;
                case StartSessionMenuOptions.RefreshElapsedTime:
                    UpdateStopwatchPanel();
                    break;
                case StartSessionMenuOptions.EndCurrentSession:
                    EndSession();
                    break;
                case StartSessionMenuOptions.ReturnToMainMenu:
                    return;  
            }
        }
    }

    /// <summary>
    /// Starts a new coding session and initializes the stopwatch.
    /// </summary>
    /// <remarks>
    /// This method marks the current time as the start of a new session, displays a confirmation message,
    /// and sets the session running flag to true. It also handles the initialization of a new session model.
    /// </remarks>
    private void StartSession()
    {
        AnsiConsole.Clear();

        _startTime = DateTime.Now;
        _sessionModel = new CodingSessionModel(_startTime, _startTime);
        _stopwatchRunning = true;

        AnsiConsole.MarkupLine("[green]Session started. Stopwatch is now running.[/]");

        _utilities.PrintNewLines(2);
        _inputHandler.PauseForContinueInput();
    }

    /// <summary>
    /// Updates and displays the elapsed time for the current active session.
    /// </summary>
    /// <remarks>
    /// This method checks if a session is active and then calculates and displays the elapsed time since the session started.
    /// If no session is active, it displays an error message.
    /// </remarks>
    private void UpdateStopwatchPanel()
    {
        AnsiConsole.Clear();

        if (!_stopwatchRunning)
        {
            AnsiConsole.MarkupLine("[red]No active session to update.[/]");
            _utilities.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
            return;
        }
    }

    /// <summary>
    /// Ends the current active session, logs the session details, and stops the stopwatch.
    /// </summary>
    /// <remarks>
    /// The method confirms with the user before ending the session. If confirmed, it calculates the total duration, updates the session model,
    /// logs the session via the DAO, and then resets the session tracking state. A success message is displayed upon completion.
    /// </remarks>
    private void EndSession()
    {
        AnsiConsole.Clear();

        if (!_stopwatchRunning)
        {
            AnsiConsole.MarkupLine("[red]No active session to end.[/]");
            _utilities.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
            return;
        }

        // Confirm ending the session
        if (!AnsiConsole.Confirm("Are you sure you want to end the current session?"))
        {
            return;
        }

        DateTime endTime = DateTime.Now;
        _sessionModel!.SetEndTime(endTime);
        _sessionModel.SetDuration(_startTime, endTime);
        _codingSessionDAO.InsertNewSession(_sessionModel);
        _stopwatchRunning = false;

        AnsiConsole.MarkupLine("[green]Session ended and logged successfully.[/]");

        _utilities.PrintNewLines(2);
        _inputHandler.PauseForContinueInput();
    }
}
