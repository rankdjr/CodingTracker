using CodingTracker.Models;
using CodingTracker.Services;
using CodingTracker.Util;
using Spectre.Console;

namespace CodingTracker.Application;

public class AppNewLogManager
{
    private readonly SessionService _sessionService;
    private AppUtil _appUtil;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppNewLogManager"/> class.
    /// </summary>
    /// <param name="sessionService">The service for managing habit data.</param>
    public AppNewLogManager(SessionService sessionService)
    {
        _sessionService = sessionService;
        _appUtil = new AppUtil();
    }

    /// <summary>
    /// Starts the Session Manager menu loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AppUtil.AnsiWriteLine(new Markup("[underline green]Select an option[/]\n"));
            var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Manage Habits")
                .PageSize(10)
                .AddChoices(Enum.GetNames(typeof(LogManualSessionMenuOptions)).Select(AppUtil.SplitCamelCase)));

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

        DateTime sessionDate = _appUtil.PromptForDate($"Enter the date for the log entry {ConfigSettings.DateFormatShort}:", DatePrompt.Short);
        TimeSpan duration = _appUtil.PromptForTimeSpan($"Enter the duration of the session. Please use the format [[ {ConfigSettings.TimeFormat} ]]:");

        CodingSessionModel newSession = new CodingSessionModel(sessionDate, duration);

        if (_sessionService.InsertNewSession(newSession))
        {
            AnsiConsole.Markup("[green]Session successfully logged![/]");
        }
        else
        {
            AnsiConsole.Markup("[red]Failed to log the session. Please try again or check the system logs.[/]");
        }

        AppUtil.PauseForContinueInput();
    }

    private void LogSessionByStartEndTimes()
    {
        AnsiConsole.Clear();

        DateTime startTime = _appUtil.PromptForDate($"Enter the date for the log entry {ConfigSettings.DateFormatShort}:", DatePrompt.Short);
        DateTime endTime = _appUtil.PromptForDate($"Enter the date for the log entry {ConfigSettings.DateFormatShort}:", DatePrompt.Short);

        CodingSessionModel newSession = new CodingSessionModel(startTime, endTime);

        if (_sessionService.InsertNewSession(newSession))
        {
            AnsiConsole.Markup("[green]Session successfully logged![/]");
        }
        else
        {
            AnsiConsole.Markup("[red]Failed to log the session. Please try again or check the system logs.[/]");
        }

        AppUtil.PauseForContinueInput();
    }
}
