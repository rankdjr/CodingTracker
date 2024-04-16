using CodingTracker.Models;
using Spectre.Console;
using System.Globalization;

namespace CodingTracker.Services;

public class InputHandler
{
    /// <summary>
    /// Prompts the user for a date input with custom messaging and validates the input.
    /// The input must be a valid date in the format "yyyy-MM-dd" and cannot be a future date.
    /// </summary>
    /// <param name="promptMessage">The message displayed to the user when asking for the date input.</param>
    /// <returns>The validated DateTime input from the user. If the input is not a valid date or is a future date, 
    /// the function continues to prompt the user until a valid date is entered.</returns>
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

    /// <summary>
    /// Prompts the user for a TimeSpan input with custom messaging and validates the input.
    /// The input must be a valid TimeSpan in the format "hh:mm:ss".
    /// </summary>
    /// <param name="promptMessage">The message displayed to the user when asking for the TimeSpan input.</param>
    /// <returns>The validated TimeSpan input from the user. If the input is not a valid TimeSpan, 
    /// the function continues to prompt the user until a valid TimeSpan is entered.</returns>
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

    /// <summary>
    /// Prompts the user to select a coding session from a list of sessions using a selection menu.
    /// Each session is displayed with details such as session ID, date, start time, end time, and duration.
    /// </summary>
    /// <param name="sessionLogs">A list of CodingSessionModel instances representing the available sessions for selection.</param>
    /// <param name="promptMessage">The message displayed to the user above the selection menu.</param>
    /// <returns>The selected CodingSessionModel instance from the user. The selection is made through a console-based interactive menu.</returns>
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

    // TODO: Refactor into custom query class
    public List<QueryOptions> PromptForQueryOptions()
    {
        string instructionText = "[grey](Press [blue]<space>[/] to toggle a selection, [green]<enter>[/] to accept, or [yellow]<enter>[/] with no selections to bypass)[/]";

        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<QueryOptions>()
                .Title("Select filter criteria [blueviolet]TimePeriod[/] and/or [blueviolet]Order By[/]?")
                .NotRequired()  // Not required to have filter criteria
                .PageSize(10)
                .InstructionsText(instructionText)
                .UseConverter(options => Utilities.SplitCamelCase(options.ToString()))
                .AddChoices(Enum.GetValues<QueryOptions>()));
    }

    // TODO: Refactor into custom query class
    public void PromptForQueryFilterOptions()
    {
        TimePeriod selectedTimePeriod = AnsiConsole.Prompt(
            new SelectionPrompt<TimePeriod>()
                .Title("Select [blueviolet]TimePeriod[/] filter criteria:")
                .PageSize(10)
                .UseConverter(options => Utilities.SplitCamelCase(options.ToString()))
                .AddChoices(Enum.GetValues<TimePeriod>()));

        int numOfPeriods = PromptForPositiveInteger($"Please enter number of {selectedTimePeriod} to retrieve:");

        Console.WriteLine($"{selectedTimePeriod}\t {numOfPeriods}");  
    }

    // TODO: Refactor into query custom class
    public List<CodingSessionModel.EditableProperties> PromptForOrderByFilterOptions()
    {
        string instructionText = "[grey](Press [blue]<space>[/] to toggle a selection, [green]<enter>[/] to accept)[/]";

        var selectedProperties = AnsiConsole.Prompt(
            new MultiSelectionPrompt<CodingSessionModel.EditableProperties>()
                .Title("Select [blueviolet]Order By[/] filter criteria:")
                .Required() // required if order by query option was selected
                .PageSize(10)
                .InstructionsText(instructionText)
                .UseConverter(options => Utilities.SplitCamelCase(options.ToString()))
                .AddChoices(Enum.GetValues<CodingSessionModel.EditableProperties>()));

        if (!selectedProperties.Any())
            return selectedProperties;

        // Ranking the selected properties
        List<CodingSessionModel.EditableProperties> rankedProperties = new List<CodingSessionModel.EditableProperties>();

        AnsiConsole.WriteLine("Please rank the selected properties in order of importance (1 being the most important):");
        foreach (var property in selectedProperties)
        {
            AnsiConsole.WriteLine($"{Utilities.SplitCamelCase(property.ToString())}");
        }

        AnsiConsole.WriteLine("Enter the ranks in the same order as the properties listed above:");
        var ranks = Console.ReadLine().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

        // Assuming the user enters the correct number of ranks
        try
        {
            int[] rankIndices = ranks.Select(int.Parse).ToArray();
            for (int i = 0; i < rankIndices.Length; i++)
            {
                // Insert the property into the ranked list based on user input
                rankedProperties.Insert(rankIndices[i] - 1, selectedProperties[i]);
            }
        }
        catch
        {
            AnsiConsole.Markup("[red]Error in parsing ranks. Please try again.[/]");
            // You might want to add some error handling or retry logic here
        }

        return rankedProperties;
    }

    /// <summary>
    /// Prompts the user for a positive integer input with custom messaging and validates the input.
    /// </summary>
    /// <param name="promptMessage">The message displayed to the user when asking for input.</param>
    /// <returns>The validated positive integer input from the user. If the input is not a valid integer or is not positive, 
    /// the function continues to prompt the user until a valid positive integer is entered.</returns>
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

    /// <summary>
    /// Pauses execution and waits for the user to press any key, displaying a prompt message.
    /// </summary>
    public void PauseForContinueInput()
    {
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
