using CodingTracker.Models;
using Spectre.Console;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CodingTracker.Services;

public class InputHandler
{
    public DateTime PromptForDate(string promptMessage, DatePrompt datePromptFormat)
    {
        var dateTimeFormat = datePromptFormat == DatePrompt.Short ? ConfigSettings.DateFormatShort : ConfigSettings.DateFormatLong;

        string dateTimeInput = AnsiConsole.Prompt(
            new TextPrompt<string>(promptMessage)
                .PromptStyle("yellow")
                .Validate(input =>
                {
                    if (DateTime.TryParseExact(input.Trim(), dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        if (parsedDate <= DateTime.Now && parsedDate > DateTime.MinValue)
                        {
                            return ValidationResult.Success();
                        }
                        else
                        {
                            var errorMessage = $"[red]Date cannot be in the future.[/]";
                            return ValidationResult.Error(errorMessage.ToString());
                        }
                    }
                    else
                    {
                        var errorMessage = $"[red]Invalid date format. Please use the format {dateTimeFormat}.[/]";
                        return ValidationResult.Error(errorMessage.ToString());
                    }
                }));

        return DateTime.ParseExact(dateTimeInput, dateTimeFormat, CultureInfo.InvariantCulture);
    }

    public TimeSpan PromptForTimeSpan(string promptMessage)
    {
        string durationInput = AnsiConsole.Prompt(
            new TextPrompt<string>(promptMessage)
                .PromptStyle("yellow")
                .Validate(input =>
                {
                    var regex = new System.Text.RegularExpressions.Regex(@"^\d{2}:\d{2}$");
                    if (!regex.IsMatch(input))
                    {
                        var errorMessage = $"[red]Invalid time format. Please use the format {ConfigSettings.TimeFormatString}.[/]";
                        return ValidationResult.Error(errorMessage.ToString());
                    }
                    else if (TimeSpan.TryParseExact(input.Trim(), ConfigSettings.TimeFormatType, CultureInfo.InvariantCulture, out TimeSpan parsedTime))
                    {
                        return ValidationResult.Success();
                    }
                    else
                    {
                        var errorMessage = $"[red]Invalid time duration. Please ensure the input format is correct.[/]";
                        return ValidationResult.Error(errorMessage.ToString());
                    }
                }));

        return TimeSpan.ParseExact(durationInput, ConfigSettings.TimeFormatType, CultureInfo.InvariantCulture);
    }

    public CodingSessionModel PromptForSessionListSelection(List<CodingSessionModel> sessionLogs, string promptMessage)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<CodingSessionModel>()
                .Title(promptMessage)
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to see more log entries)[/]")
                .UseConverter(entry =>
                    $"[bold yellow]ID:[/] {entry.Id}, " +
                    $"[bold cyan]Session Date:[/] {entry.SessionDate}, " +
                    (entry.StartTime != null ? $"[bold green]Start Time:[/] {entry.StartTime}, " : "") +
                    (entry.EndTime != null ? $"[bold magenta]End Time:[/] {entry.EndTime}, " : "") +
                    $"[bold blue]Duration:[/] {entry.Duration}")
                .AddChoices(sessionLogs));
    }

    public List<CodingSessionModel.EditableProperties> PromptForSessionPropertiesSelection(string promptMessage)
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<CodingSessionModel.EditableProperties>()
                .Title("Select properties you want to edit:")
                .NotRequired()
                .PageSize(10)
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle a property, [green]<enter>[/] to accept, [yellow]<enter>[/] with no selections will cancel update)[/]")
                .AddChoices(Enum.GetValues<CodingSessionModel.EditableProperties>()));
    }

    public (TimePeriod? periodFilter, int? numOfPeriods) PromptForTimePeriodAndCount()
    {
        TimePeriod? periodFilter = null;
        int? numOfPeriods = null;

        if (AnsiConsole.Confirm("Would you like to filter by past Time Periods (days, weeks, years)?"))
        {
            periodFilter = PromptForQueryTimePeriodOptions();
            string promptMessage = $"Please enter number of {periodFilter} to retrieve:";
            numOfPeriods = PromptForPositiveInteger(promptMessage);
        }

        return (periodFilter, numOfPeriods);
    }

    public TimePeriod? PromptForQueryTimePeriodOptions()
    {
        try
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<TimePeriod>()
                    .Title("Select [blueviolet]TimePeriod[/] filter criteria:")
                    .PageSize(10)
                    .UseConverter(options => Utilities.SplitCamelCase(options.ToString()))
                    .AddChoices(Enum.GetValues<TimePeriod>()));
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }


    public List<(CodingSessionModel.EditableProperties, SortDirection, int)> PromptForOrderByFilterOptions()
    {
        AnsiConsole.Clear();
        string instructionText = "[grey](Press [blue]<space>[/] to toggle a selection, [green]<enter>[/] to accept, [red]<escape>[/] to skip ordering)[/]";

        // Selection of properties
        var selectedProperties = AnsiConsole.Prompt(
            new MultiSelectionPrompt<CodingSessionModel.EditableProperties>()
                .Title("[blueviolet]Select Order By filter criteria (optional):[/]")
                .NotRequired() // Order by sorting not required
                .PageSize(10)
                .InstructionsText(instructionText)
                .UseConverter(options => Utilities.SplitCamelCase(options.ToString()))
                .AddChoices(Enum.GetValues<CodingSessionModel.EditableProperties>()));

        if (!selectedProperties.Any())
            return new List<(CodingSessionModel.EditableProperties, SortDirection, int)>();

        // Initialize the list with null directions and not set ranks
        List<(CodingSessionModel.EditableProperties property, SortDirection? direction, int? rank)> propertiesWithDirectionsRanks = selectedProperties
            .Select(property => (property, (SortDirection?)null, (int?)null))
            .ToList();

        DisplayCurrentSelections(propertiesWithDirectionsRanks);

        // Iterate through properties to select direction
        for (int i = 0; i < propertiesWithDirectionsRanks.Count; i++)
        {
            var direction = AnsiConsole.Prompt(
                new SelectionPrompt<SortDirection>()
                    .Title($"Select the sort direction for [blueviolet]{Utilities.SplitCamelCase(propertiesWithDirectionsRanks[i].property.ToString())}[/]:")
                    .AddChoices(Enum.GetValues<SortDirection>()));

            propertiesWithDirectionsRanks[i] = (propertiesWithDirectionsRanks[i].property, direction, propertiesWithDirectionsRanks[i].rank);

            // Update the display after each direction is selected
            DisplayCurrentSelections(propertiesWithDirectionsRanks);
        }

        // Interactively ask for the rank for each property
        for (int i = 0; i < propertiesWithDirectionsRanks.Count; i++)
        {
            int rank = AnsiConsole.Prompt(
                new TextPrompt<int>($"Enter the rank for [blueviolet]{Utilities.SplitCamelCase(propertiesWithDirectionsRanks[i].property.ToString())}[/]:")
                    .Validate(input =>
                    {
                        if (input < 1 || input > propertiesWithDirectionsRanks.Count || propertiesWithDirectionsRanks.Any(p => p.rank == input))
                            return ValidationResult.Error("[red]Invalid rank. Ensure ranks are unique and within the correct range.[/]");
                        return ValidationResult.Success();
                    }));

            propertiesWithDirectionsRanks[i] = (propertiesWithDirectionsRanks[i].property, propertiesWithDirectionsRanks[i].direction, rank);

            // Update the display after each rank is set
            DisplayCurrentSelections(propertiesWithDirectionsRanks);
        }

        // Final confirmation before executing the query
        AnsiConsole.Clear();
        DisplayCurrentSelections(propertiesWithDirectionsRanks);

        if (AnsiConsole.Confirm("Run query with these properties and directions?"))
        {
            // Sort the list based on rank and return
            return propertiesWithDirectionsRanks
                .Select(p => (p.property, p.direction!.Value, p.rank!.Value))
                .OrderBy(p => p.Item3)
                .ToList();
        }
        else
        {
            return PromptForOrderByFilterOptions(); // Recurse on modifications
        }
    }

    private void DisplayCurrentSelections(List<(CodingSessionModel.EditableProperties property, SortDirection? direction, int? rank)> properties)
    {
        var table = new Table();
        table.AddColumn("[blueviolet]Property[/]");
        table.AddColumn("[blueviolet]Direction[/]");
        table.AddColumn("[blueviolet]Rank[/]");
        table.Border(TableBorder.Rounded);

        foreach (var (property, direction, rank) in properties)
        {
            table.AddRow(Utilities.SplitCamelCase(property.ToString()), direction?.ToString() ?? "Not Set", rank?.ToString() ?? "Not Set");
        }

        // Clear the previous output before displaying the updated table
        AnsiConsole.Clear();
        AnsiConsole.Write(table);
    }

    public int PromptForPositiveInteger(string promptMessage)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>(promptMessage)
                .Validate(input =>
                {
                    // Attempt to parse the input as an integer
                    if (!int.TryParse(input.ToString().Trim(), out int parsedQuantity))
                    {
                        return ValidationResult.Error("[red]Please enter a valid integer number.[/]");
                    }

                    // Check if the parsed integer is positive
                    if (parsedQuantity <= 0)
                    {
                        return ValidationResult.Error("[red]Please enter a positive number.[/]");
                    }

                    return ValidationResult.Success();
                }));
    }

    public void PauseForContinueInput()
    {
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    public bool ConfirmAction(string actionPromptMessage) 
    {
        if (!AnsiConsole.Confirm(actionPromptMessage))
        {
            Utilities.DisplayCancellationMessage("Operation cancelled.");
            PauseForContinueInput();
            return false;
        }

        return true;
    }
}
