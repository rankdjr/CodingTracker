using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Application;

public class AppStopWatchManager
{
    private readonly CodingSessionDAO _codingSessionDAO;
    private Utilities _appUtil;
    private InputHandler _inputHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppNewLogManager"/> class.
    /// </summary>
    /// <param name="codingSessionDAO">The service for managing habit data.</param>
    public AppStopWatchManager(CodingSessionDAO codingSessionDAO)
    {
        _codingSessionDAO = codingSessionDAO;
        _appUtil = new Utilities();
        _inputHandler = new InputHandler();
    }

    /// <summary>
    /// Starts the Session Manager menu loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            _appUtil.AnsiWriteLine(new Markup("[underline green]Select an option[/]\n"));
            var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Add New Session Log Records")
                .PageSize(10)
                .AddChoices(Enum.GetNames(typeof(LogManualSessionMenuOptions)).Select(_appUtil.SplitCamelCase)));

            switch (Enum.Parse<LogManualSessionMenuOptions>(option.Replace(" ", "")))
            {
                case LogManualSessionMenuOptions.LogSessionByDateAndDuration:
                    LogSessionByDateAndDuration();
                    break;
                case LogManualSessionMenuOptions.LogSessionByStartEndTimes:
                    LogSessionByStartEndTimes();
                    break;
                case LogManualSessionMenuOptions.ReturnToMainMenu:
                    return;
            }
        }
    }

    /// <summary>
    /// Prompts the user for a session date and duration, then logs the session accordingly.
    /// </summary>
    private void LogSessionByDateAndDuration()
    {
        AnsiConsole.Clear();

        DateTime sessionDate = _inputHandler.PromptForDate($"Enter the date for the log entry {ConfigSettings.DateFormatShort}:", DatePrompt.Short);
        TimeSpan duration = _inputHandler.PromptForTimeSpan($"Enter the duration of the session. Please use the format [[ {ConfigSettings.TimeFormatString} ]]:");

        // Create new coding session object via constructor (duration manually entered here)
        CodingSessionModel newSession = new CodingSessionModel(sessionDate, duration);

        int newRecordID = _codingSessionDAO.InsertNewSession(newSession);
        if (newRecordID != -1)
        {
            string successMessage = $"[green]Session successfully logged with SessionId [[ {newRecordID} ]]![/]";
            AnsiConsole.Write(new Markup(successMessage));
            _appUtil.PrintNewLines(1);
        }
        else
        {
            AnsiConsole.Markup("[red]Failed to log the session. Please try again or check the system logs.[/]");
        }

        _inputHandler.PauseForContinueInput();
    }

    /// <summary>
    /// Prompts the user for a start and end time, validates the entries, and logs the session if the end time is after the start time.
    /// </summary>
    private void LogSessionByStartEndTimes()
    {
        AnsiConsole.Clear();

        DateTime startTime = _inputHandler.PromptForDate($"Enter the Start Time for the log entry {ConfigSettings.DateFormatLong}:", DatePrompt.Long);
        DateTime endTime;

        do
        {
            endTime = _inputHandler.PromptForDate($"Enter the End Time for the session {ConfigSettings.DateFormatLong}:", DatePrompt.Long);
            if (endTime <= startTime)
            {
                AnsiConsole.Markup("[red]End time must be after start time. Please enter a valid end time.[/]\n");
            }
        } while (endTime <= startTime);

        // Create new coding session object via constructor (duration automatically calculated)
        CodingSessionModel newSession = new CodingSessionModel(startTime, endTime);
        int newRecordID = _codingSessionDAO.InsertNewSession(newSession);
        if (newRecordID != -1)
        {
            string successMessage = $"[green]Session successfully logged with SessionId [[ {newRecordID} ]]![/]";
            AnsiConsole.Write(new Markup(successMessage));
            _appUtil.PrintNewLines(1);
        }
        else
        {
            AnsiConsole.Markup("[red]Failed to log the session. Please try again or check the system logs.[/]");
        }

        _inputHandler.PauseForContinueInput();
    }
}
