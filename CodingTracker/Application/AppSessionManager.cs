using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;
using System.Runtime.Intrinsics.X86;

namespace CodingTracker.Application;

public class AppSessionManager
{
    private readonly CodingSessionDAO _codingSessionDAO;
    private Utilities _appUtil;
    private InputHandler _inputHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSessionManager"/> class.
    /// </summary>
    /// <param name="sessionService">The service for managing habit data.</param>
    public AppSessionManager(CodingSessionDAO codingSessionDAO)
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
                .Title("Manage Coding Session Records")
                .PageSize(10)
                .AddChoices(Enum.GetNames(typeof(ManageSessionsMenuOptions))
                .Select(_appUtil.SplitCamelCase)));

            switch (Enum.Parse<ManageSessionsMenuOptions>(option.Replace(" ", "")))
            {
                case ManageSessionsMenuOptions.ViewAllSessions:
                    ViewSessions();
                    break;
                case ManageSessionsMenuOptions.UpdateSessionRecord:
                    EditSession();
                    break;
                case ManageSessionsMenuOptions.DeleteSessionRecord:
                    DeleteSession();
                    break;
                case ManageSessionsMenuOptions.DeleteAllSessions:
                    DeleteAllSession();
                    break;
                case ManageSessionsMenuOptions.ReturnToMainMenu:
                    return;
            }
        }


    }
    private void ViewSessions()
    {
        var sessions = _codingSessionDAO.GetAllSessionRecords();

        if (sessions.Count == 0)
        {
            _appUtil.AnsiWriteLine(new Markup("[yellow]No sessions found![/]"));
            _appUtil.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
            return;
        }

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Grey);
        table.Title("[yellow]Session Overview[/]");

        table.AddColumn(new TableColumn("[bold underline]ID[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold underline]Session Date[/]").Centered());
        table.AddColumn(new TableColumn("[bold underline]Duration[/]").Centered());
        table.AddColumn(new TableColumn("[bold underline]Start Time[/]").Centered());
        table.AddColumn(new TableColumn("[bold underline]End Time[/]").Centered());
        table.AddColumn(new TableColumn("[bold underline]Date Created[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold underline]Date Updated[/]").LeftAligned());

        foreach (var session in sessions)
        {
            table.AddRow(
                session.Id.ToString()!,
                session.SessionDate,
                session.Duration,
                session.StartTime ?? "N/A", // Handle nullable StartTime
                session.EndTime ?? "N/A",   // Handle nullable EndTime
                session.DateCreated,
                session.DateUpdated
            );
        }

        AnsiConsole.Write(table);
        _appUtil.PrintNewLines(2);
        _inputHandler.PauseForContinueInput();
    }

    private void EditSession()
    {
        List<CodingSessionModel> sessionLogs = _codingSessionDAO.GetAllSessionRecords();
        if (!sessionLogs.Any())
        {
            _appUtil.AnsiWriteLine(new Markup("[red]No log entries available to edit.[/]"));
            _appUtil.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
            return;
        }

        CodingSessionModel sessionEntrySelection = _inputHandler.PromptForSessionListSelection(
            sessionLogs, "[yellow]Which log entry would you like to edit?[/]");

        string promptMessage = "Select properties you want to edit:";
        List<CodingSessionModel.EditableProperties> propertiesToEdit = _inputHandler.PromptForSessionPropertiesSelection(promptMessage);

        if (!propertiesToEdit.Any()) 
        {
            AnsiConsole.Markup("[yellow]Update cancelled![/]");
            _appUtil.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
            return;
        }

        foreach (var property in propertiesToEdit)
        {
            DateTime newDate;
            TimeSpan newDuration;

            switch (property)
            {
                case CodingSessionModel.EditableProperties.SessionDate:
                    newDate = _inputHandler.PromptForDate($"Enter the SessionDate for log entry {ConfigSettings.DateFormatShort}:", DatePrompt.Short);
                    sessionEntrySelection.SetSessionDate(newDate);
                    break;
                case CodingSessionModel.EditableProperties.Duration:
                    newDuration = _inputHandler.PromptForTimeSpan($"Enter new Duration for log entry {ConfigSettings.TimeFormatString}:");
                    sessionEntrySelection.SetDuration(newDuration);
                    break;
                case CodingSessionModel.EditableProperties.StartTime:
                    newDate = _inputHandler.PromptForDate($"Enter new StartTime for log entry {ConfigSettings.DateFormatLong}:", DatePrompt.Long);
                    sessionEntrySelection.SetStartTime(newDate);
                    break;
                case CodingSessionModel.EditableProperties.EndTime:
                    newDate = _inputHandler.PromptForDate($"Enter the EndTime for log entry {ConfigSettings.DateFormatLong}:", DatePrompt.Long);
                    sessionEntrySelection.SetStartTime(newDate);
                    break;
            }
        }

        if (_codingSessionDAO.UpdateSession(sessionEntrySelection))
        {
            AnsiConsole.Markup("[green]Coding session successfully updated![/]");
            _appUtil.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
        }
        else
        {
            AnsiConsole.Markup("[red]No Coding sessions were updated.[/]");
            _inputHandler.PauseForContinueInput();
        }
    }

    private void DeleteSession()
    {
        List<CodingSessionModel> sessionLogs = _codingSessionDAO.GetAllSessionRecords();
        if (!sessionLogs.Any())
        {
            _appUtil.AnsiWriteLine(new Markup("[red]No log entries available to delete.[/]"));
            _inputHandler.PauseForContinueInput();
            return;
        }

        CodingSessionModel sessionEntrySelection = _inputHandler.PromptForSessionListSelection(
            sessionLogs, "[yellow]Which log entry would you like to delete?[/]");
        
        if (AnsiConsole.Confirm($"Are you sure you want to delete this log entry (ID: {sessionEntrySelection.Id})?"))
        {
            if (_codingSessionDAO.DeleteSessionRecord(sessionEntrySelection.Id!.Value))
            {
                _appUtil.AnsiWriteLine(new Markup("[green]Log entry successfully deleted![/]"));
                _appUtil.PrintNewLines(2);
            }
            else
            {
                _appUtil.AnsiWriteLine(new Markup("[red]Failed to delete log entry. It may no longer exist or the database could be locked.[/]"));
                _appUtil.PrintNewLines(2);
            }
        }
        else
        {
            _appUtil.AnsiWriteLine(new Markup("[yellow]Operation cancelled.[/]"));
            _appUtil.PrintNewLines(2);
        }

        _inputHandler.PauseForContinueInput();
    }

    private void DeleteAllSession()
    {
        if (_codingSessionDAO.DeleteAllSessions())
        {
            AnsiConsole.Markup("[green]All sessions have been successfully deleted![/]");
            _appUtil.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
        }
        else
        {
            AnsiConsole.Markup("[red]\"No sessions were deleted. (The table might have been empty).[/]");
            _appUtil.PrintNewLines(2);
            _inputHandler.PauseForContinueInput();
        }
    }
}
