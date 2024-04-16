using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;
using System.Globalization;

namespace CodingTracker.Application;

public class UserInput
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
            new TextPrompt<String>(promptMessage)
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
    public CodingSessionModel PromptForSessionSelection(List<CodingSessionModel> sessionLogs, string promptMessage)
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

        /// <summary>
        /// Pauses execution and waits for the user to press any key, displaying a prompt message.
        /// </summary>
        public void PauseForContinueInput()
    {
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
