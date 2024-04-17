using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Application;

public class AppSessionManager
{
    private readonly CodingSessionDAO _codingSessionDAO;
    private InputHandler _inputHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSessionManager"/> class.
    /// </summary>
    /// <param name="sessionService">The service for managing habit data.</param>
    public AppSessionManager(CodingSessionDAO codingSessionDAO, InputHandler inputHandler)
    {
        _codingSessionDAO = codingSessionDAO;
        _inputHandler = inputHandler;
    }

    /// <summary>
    /// Starts the Session Manager menu loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Manage Coding Session Records")
                    .PageSize(10)
                    .AddChoices(Enum.GetNames(typeof(ManageSessionsMenuOptions))
                    .Select(Utilities.SplitCamelCase)));

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

    /// <summary>
    /// Displays all coding session records in a table format.
    /// </summary>
    /// <remarks>
    /// Retrieves all session records from the database and displays them. If no sessions are found, it displays 
    /// a message indicating that no sessions are available. This method allows the user to view details about 
    /// each session, such as ID, dates, times, and durations.
    /// </remarks>
    private void ViewSessions()
    {
        var sessions = _codingSessionDAO.GetAllSessionRecords();

        if (sessions.Count == 0)
        {
            Utilities.DisplayWarningMessage("]No sessions found!");
            _inputHandler.PauseForContinueInput();
            return;
        }

        // TODO: Build query option selections

        // TODO: prompt for time period selections 
        
        // TODO: multiselect prompt for columns and ASC or DESC

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
        Utilities.PrintNewLines(2);
        _inputHandler.PauseForContinueInput();
    }

    /// <summary>
    /// Allows the user to select and edit properties of a coding session.
    /// </summary>
    private void EditSession()
    {
        List<CodingSessionModel> sessionLogs = _codingSessionDAO.GetAllSessionRecords();
        if (!sessionLogs.Any())
        {
            Utilities.DisplayWarningMessage("No log entries available to edit.");
            _inputHandler.PauseForContinueInput();
            return;
        }

        CodingSessionModel sessionEntrySelection = _inputHandler.PromptForSessionListSelection(
            sessionLogs, "[yellow]Which log entry would you like to edit?[/]");

        string promptMessage = "Select properties you want to edit:";
        List<CodingSessionModel.EditableProperties> propertiesToEdit = _inputHandler.PromptForSessionPropertiesSelection(promptMessage);

        if (!propertiesToEdit.Any()) 
        {
            Utilities.DisplayCancellationMessage("Update cancelled!");
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
            Utilities.DisplaySuccessMessage("Coding session successfully updated!");
            _inputHandler.PauseForContinueInput();
        }
        else
        {
            Utilities.DisplayWarningMessage("No Coding sessions were updated.");
            _inputHandler.PauseForContinueInput();
        }
    }

    /// <summary>
    /// Allows the user to select and delete a specific coding session from the database.
    /// </summary>
    private void DeleteSession()
    {
        List<CodingSessionModel> sessionLogs = _codingSessionDAO.GetAllSessionRecords();
        if (!sessionLogs.Any())
        {
            Utilities.DisplayWarningMessage("No log entries available to delete.");
            _inputHandler.PauseForContinueInput();
            return;
        }

        CodingSessionModel sessionEntrySelection = _inputHandler.PromptForSessionListSelection(
            sessionLogs, "[yellow]Which log entry would you like to delete?[/]");
        
        if (AnsiConsole.Confirm($"Are you sure you want to delete this log entry (ID: {sessionEntrySelection.Id})?"))
        {
            if (_codingSessionDAO.DeleteSessionRecord(sessionEntrySelection.Id!.Value))
            {
                Utilities.DisplaySuccessMessage("Log entry successfully deleted!");
            }
            else
            {
                Utilities.DisplayWarningMessage("Failed to delete log entry. It may no longer exist or the database could be locked.");
            }
        }
        else
        {
            Utilities.DisplayCancellationMessage("]Operation cancelled.");
        }

        _inputHandler.PauseForContinueInput();
    }

    /// <summary>
    /// Deletes all coding session records from the database.
    /// </summary>
    private void DeleteAllSession()
    {
        if (_codingSessionDAO.DeleteAllSessions())
        {
            Utilities.DisplaySuccessMessage("All sessions have been successfully deleted!");
            _inputHandler.PauseForContinueInput();
        }
        else
        {
            Utilities.DisplayWarningMessage("No sessions were deleted. (The table might have been empty).");
            _inputHandler.PauseForContinueInput();
        }
    }
}
