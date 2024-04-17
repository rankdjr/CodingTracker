using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Application;

public class AppNewLogManager
{
    private readonly CodingSessionDAO _codingSessionDAO;
    private InputHandler _inputHandler;

    public AppNewLogManager(CodingSessionDAO codingSessionDAO, InputHandler inputHandler)
    {
        _codingSessionDAO = codingSessionDAO;
        _inputHandler = inputHandler;
    }

    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Add New Session Log Records")
                    .PageSize(10)
                    .AddChoices(Enum.GetNames(typeof(LogManualSessionMenuOptions))
                    .Select(Utilities.SplitCamelCase)));

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
            Utilities.PrintNewLines(1);
        }
        else
        {
            Utilities.DisplayWarningMessage("Failed to log the session. Please try again or check the system logs.");
        }

        _inputHandler.PauseForContinueInput();
    }

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
                Utilities.DisplayWarningMessage("End time must be after start time. Please enter a valid end time.");
            }
        } while (endTime <= startTime);

        // Create new coding session object via constructor (duration automatically calculated)
        CodingSessionModel newSession = new CodingSessionModel(startTime, endTime);
        int newRecordID = _codingSessionDAO.InsertNewSession(newSession);
        if (newRecordID != -1)
        {
            string successMessage = $"[green]Session successfully logged with SessionId [[ {newRecordID} ]]![/]";
            Utilities.DisplaySuccessMessage(successMessage);
        }
        else
        {
            Utilities.DisplayWarningMessage("Failed to log the session. Please try again or check the system logs.");
        }

        _inputHandler.PauseForContinueInput();
    }
}
