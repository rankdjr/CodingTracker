using CodingTracker.Models;
using CodingTracker.Services;
using CodingTracker.Util;
using Spectre.Console;
using System.Runtime.Intrinsics.X86;

namespace CodingTracker.Application;

public class AppSessionManager
{
    private readonly SessionService _sessionService;
    private AppUtil _appUtil;
    private UserInput _userInput;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSessionManager"/> class.
    /// </summary>
    /// <param name="sessionService">The service for managing habit data.</param>
    public AppSessionManager(SessionService sessionService)
    {
        _sessionService = sessionService;
        _appUtil = new AppUtil();
        _userInput = new UserInput();
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
        var sessions = _sessionService.GetAllSessionRecords();

        if (sessions.Count == 0)
        {
            _appUtil.AnsiWriteLine(new Markup("[yellow]No sessions found![/]"));
            _userInput.PauseForContinueInput();
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
        _userInput.PauseForContinueInput();
    }

    private void EditSession()
    {
        // https://spectreconsole.net/prompts/multiselection
        // Allow users to select from session list
        // output selected session at top level
        // save coding session as new CodingSessionModel object
        // allow users to multi-select the fields they want to edit
        // prompt users for new entries based on multi-select
        // use CodingSessionModel to set new values

        // pass new object to DAO and update record by id
    }

    private void DeleteSession()
    {
        List<CodingSessionModel> sessionLogs = _sessionService.GetAllSessionRecords();
        if (!sessionLogs.Any())
        {
            _appUtil.AnsiWriteLine(new Markup("[red]No log entries available to delete.[/]"));
            _userInput.PauseForContinueInput();
            return;
        }

        CodingSessionModel sessionEntrySelection = _userInput.PromptForSessionSelection(
            sessionLogs, "[yellow]Which log entry would you like to delete?[/]");
        
        if (AnsiConsole.Confirm($"Are you sure you want to delete this log entry (ID: {sessionEntrySelection.Id})?"))
        {
            bool result = _sessionService.DeleteSessionRecord(sessionEntrySelection.Id!.Value);
            if (result)
                _appUtil.AnsiWriteLine(new Markup("[green]Log entry successfully deleted![/]"));
            else
                _appUtil.AnsiWriteLine(new Markup("[red]Failed to delete log entry. It may no longer exist or the database could be locked.[/]"));
        }
        else
        {
            _appUtil.AnsiWriteLine(new Markup("[yellow]Operation cancelled.[/]"));
        }

        _userInput.PauseForContinueInput();
    }

    private void DeleteAllSession()
    {
        if (_sessionService.DeleteAllSessions())
        {
            AnsiConsole.Markup("[green]All sessions have been successfully deleted![/]");
            _appUtil.PrintNewLines(1);
            _userInput.PauseForContinueInput();
        }
        else
        {
            AnsiConsole.Markup("[red]\"No sessions were deleted. (The table might have been empty).[/]");
            _userInput.PauseForContinueInput();
        }
    }
}
