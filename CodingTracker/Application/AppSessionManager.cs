using CodingTracker.Services;
using CodingTracker.Util;
using Spectre.Console;

namespace CodingTracker.Application;

public class AppSessionManager
{
    private readonly SessionService _sessionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSessionManager"/> class.
    /// </summary>
    /// <param name="sessionService">The service for managing habit data.</param>
    public AppSessionManager(SessionService sessionService)
    {
        _sessionService = sessionService;
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
                .AddChoices(Enum.GetNames(typeof(ManageSessionsMenuOptions)).Select(AppUtil.SplitCamelCase)));

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
        var sessions = _sessionService.GetAllSessionRecords(); // This method needs to be implemented in your SessionService
        if (sessions.Count == 0)
        {
            AppUtil.AnsiWriteLine(new Markup("[yellow]No sessions found![/]"));
            AppUtil.PauseForContinueInput();
        }
        else
        {
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Grey);
            table.Title("[yellow]Session Overview[/]");

            // Adding columns with improved styling
            table.AddColumn(new TableColumn("[bold underline]ID[/]").LeftAligned());
            table.AddColumn(new TableColumn("[bold underline]Session Date[/]").Centered());
            table.AddColumn(new TableColumn("[bold underline]Duration (Hours)[/]").Centered());
            table.AddColumn(new TableColumn("[bold underline]Date Created[/]").LeftAligned());
            table.AddColumn(new TableColumn("[bold underline]Date Updated[/]").LeftAligned());

            // Add rows with conditional formatting
            foreach (var session in sessions)
            {
                table.AddRow(
                    session.SessionId.ToString()!,
                    session.SessionDate.ToString(),
                    session.Duration.ToString(),
                    session.DateCreated.ToString(),
                    session.DateUpdated.ToString()
                );
            }

            AnsiConsole.Write(table);
        }

        AppUtil.PauseForContinueInput();
    }

    private void EditSession()
    {
        // Implementation for editing an existing session
    }

    private void DeleteSession()
    {
        // Implementation for deleting a session
    }

    private void DeleteAllSession()
    {
        // Implementation for deleting a session
    }
}
