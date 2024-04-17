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


    //// TODO: verify logic
    //public List<(CodingSessionModel.EditableProperties, SortDirection, int)> PromptForOrderByFilterOptions()
    //{
    //    string instructionText = "[grey](Press [blue]<space>[/] to toggle a selection, [green]<enter>[/] to accept, [red]<escape>[/] to skip ordering)[/]";

    //    var selectedProperties = AnsiConsole.Prompt(
    //        new MultiSelectionPrompt<CodingSessionModel.EditableProperties>()
    //            .Title("[blueviolet]Select Order By filter criteria (optional):[/]")
    //            .PageSize(10)
    //            .InstructionsText(instructionText)
    //            .UseConverter(options => Utilities.SplitCamelCase(options.ToString()))
    //            .AddChoices(Enum.GetValues<CodingSessionModel.EditableProperties>()));

    //    if (!selectedProperties.Any())
    //    {
    //        AnsiConsole.Markup("[yellow]No ordering selected. Displaying unsorted results.[/]\n");
    //        return new List<(CodingSessionModel.EditableProperties, SortDirection, int)>();
    //    }

    //    // Gather direction for each property
    //    List<(CodingSessionModel.EditableProperties property, SortDirection direction)> propertiesWithDirections = new List<(CodingSessionModel.EditableProperties, SortDirection)>();
    //    foreach (var property in selectedProperties)
    //    {
    //        var direction = AnsiConsole.Prompt(
    //            new SelectionPrompt<SortDirection>()
    //                .Title($"Select the sort direction for [blueviolet]{Utilities.SplitCamelCase(property.ToString())}[/]:")
    //                .AddChoices(Enum.GetValues<SortDirection>()));
    //        propertiesWithDirections.Add((property, direction));
    //    }

    //    // Prompt for ranking the properties
    //    AnsiConsole.WriteLine("\nPlease rank the selected properties in order of importance (1 being the most important):");
    //    foreach (var item in propertiesWithDirections)
    //    {
    //        AnsiConsole.WriteLine($"{Utilities.SplitCamelCase(item.property.ToString())} ({item.direction})");
    //    }

    //    AnsiConsole.WriteLine("Enter the ranks in the same order as the properties listed above, separated by spaces:");
    //    var ranks = Console.ReadLine().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
    //    List<(CodingSessionModel.EditableProperties, SortDirection, int)> rankedProperties = new List<(CodingSessionModel.EditableProperties, SortDirection, int)>();

    //    if (ranks.Length == propertiesWithDirections.Count)
    //    {
    //        for (int i = 0; i < ranks.Length; i++)
    //        {
    //            if (int.TryParse(ranks[i], out int rank) && rank > 0 && rank <= ranks.Length)
    //            {
    //                rankedProperties.Add((propertiesWithDirections[i].property, propertiesWithDirections[i].direction, rank));
    //            }
    //            else
    //            {
    //                AnsiConsole.Markup("[red]Invalid rank entered. Please ensure all ranks are numeric and correspond to the listed properties.[/]\n");
    //                return PromptForOrderByFilterOptions(); // Recurse on error
    //            }
    //        }
    //    }
    //    else
    //    {
    //        AnsiConsole.Markup("[red]The number of ranks must match the number of selected properties. Please try again.[/]\n");
    //        return PromptForOrderByFilterOptions(); // Recurse on error
    //    }

    //    // Sort the list based on rank
    //    rankedProperties.Sort((a, b) => a.Item3.CompareTo(b.Item3));
    //    return rankedProperties;
    //}


    //// TODO: save for reference until query filtering is done
    //public List<CodingSessionModel.EditableProperties> PromptForOrderByFilterOptions_OLD()
    //{
    //    string instructionText = "[grey](Press [blue]<space>[/] to toggle a selection, [green]<enter>[/] to accept)[/]";

    //    var selectedProperties = AnsiConsole.Prompt(
    //        new MultiSelectionPrompt<CodingSessionModel.EditableProperties>()
    //            .Title("Select [blueviolet]Order By[/] filter criteria:")
    //            .Required() // required if order by query option was selected
    //            .PageSize(10)
    //            .InstructionsText(instructionText)
    //            .UseConverter(options => Utilities.SplitCamelCase(options.ToString()))
    //            .AddChoices(Enum.GetValues<CodingSessionModel.EditableProperties>()));

    //    if (!selectedProperties.Any())
    //        return selectedProperties;

    //    // Ranking the selected properties
    //    List<CodingSessionModel.EditableProperties> rankedProperties = new List<CodingSessionModel.EditableProperties>();

    //    AnsiConsole.WriteLine("Please rank the selected properties in order of importance (1 being the most important):");
    //    foreach (var property in selectedProperties)
    //    {
    //        AnsiConsole.WriteLine($"{Utilities.SplitCamelCase(property.ToString())}");
    //    }

    //    AnsiConsole.WriteLine("Enter the ranks in the same order as the properties listed above:");
    //    var ranks = Console.ReadLine().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

    //    // Assuming the user enters the correct number of ranks
    //    try
    //    {
    //        int[] rankIndices = ranks.Select(int.Parse).ToArray();
    //        for (int i = 0; i < rankIndices.Length; i++)
    //        {
    //            // Insert the property into the ranked list based on user input
    //            rankedProperties.Insert(rankIndices[i] - 1, selectedProperties[i]);
    //        }
    //    }
    //    catch
    //    {
    //        AnsiConsole.Markup("[red]Error in parsing ranks. Please try again.[/]");
    //        // You might want to add some error handling or retry logic here
    //    }

    //    return rankedProperties;
    //}

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
