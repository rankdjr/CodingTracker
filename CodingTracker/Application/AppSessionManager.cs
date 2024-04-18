using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Application;

public class AppSessionManager
{
    private readonly CodingSessionDAO _codingSessionDAO;
    private  InputHandler _inputHandler;

    public AppSessionManager(CodingSessionDAO codingSessionDAO, InputHandler inputHandler)
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

    private void ViewSessions()
    {
        List<CodingSessionModel> codingSessions = new List<CodingSessionModel>();

        var (periodFilter, numOfPeriods, orderByOptions) = PromptForQueryOptions(); 

        codingSessions = _codingSessionDAO.GetSessionsRecords(periodFilter, numOfPeriods, orderByOptions);

        if (codingSessions.Count == 0)
        {
            Utilities.DisplayWarningMessage("No sessions found!");
            _inputHandler.PauseForContinueInput();
            return;
        }

        Table sessionsViewTable = BuildCodingSessionsViewTable(codingSessions);

        AnsiConsole.Clear();
        AnsiConsole.Write(sessionsViewTable);
        Utilities.PrintNewLines(2);

        _inputHandler.PauseForContinueInput();
    }

    private (TimePeriod? periodFilter, int? numOfPeriods, List<(CodingSessionModel.EditableProperties, SortDirection, int)> orderByOptions) PromptForQueryOptions()
    {
        (TimePeriod? periodFilter, int? numOfPeriods) = _inputHandler.PromptForTimePeriodAndCount();
        List<(CodingSessionModel.EditableProperties, SortDirection, int)> filterAndSortOptions = _inputHandler.PromptForOrderByFilterOptions();
        
        return (periodFilter, numOfPeriods, filterAndSortOptions);
    }

    private Table BuildCodingSessionsViewTable(List<CodingSessionModel> codingSessions)
    {
        string TableTitle = "[yellow]Session Overview[/]";

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Grey);
        table.Title(TableTitle);

        foreach (var property in Enum.GetValues<CodingSessionModel.SessionProperties>())
        {
            string columnName = $"[bold underline]{property}[/]";
            table.AddColumn(new TableColumn(columnName).Centered());
        }

        foreach (var session in codingSessions)
        {
            table.AddRow(
                session.Id.ToString()!,
                session.SessionDate,
                session.Duration,
                session.StartTime,
                session.EndTime,
                session.DateCreated,
                session.DateUpdated
            );
        }

        return table;
    }

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

            switch (property)
            {
                case CodingSessionModel.EditableProperties.SessionDate:
                    newDate = _inputHandler.PromptForDate($"Enter the SessionDate for log entry {ConfigSettings.DateFormatShort}:", DatePrompt.Short);
                    sessionEntrySelection.SetSessionDate(newDate);
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

        // TODO: Implement CodingActivityManager to insert session activities and update goal progress; return boolean instead of ID

        if (_codingSessionDAO.UpdateSession(sessionEntrySelection))
        {
            Utilities.DisplaySuccessMessage("Coding session successfully updated!");
        }
        else
        {
            Utilities.DisplayWarningMessage("No Coding sessions were updated.");
        }


        _inputHandler.PauseForContinueInput();
    }

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
            // TODO: Implement CodingActivityManager to insert session activities and update goal progress; return boolean instead of ID

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

    private void DeleteAllSession()
    {
        // TODO: Implement CodingActivityManager to insert session activities and update goal progress; return boolean instead of ID

        if (_codingSessionDAO.DeleteAllSessions())
        {
            Utilities.DisplaySuccessMessage("All sessions have been successfully deleted!");
            _inputHandler.PauseForContinueInput();
        }
        else
        {
            Utilities.DisplayWarningMessage("No sessions were deleted. (The table may have been empty).");
            _inputHandler.PauseForContinueInput();
        }
    }
}
