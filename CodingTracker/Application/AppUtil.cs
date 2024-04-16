﻿using CodingTracker.Database;
using CodingTracker.Services;
using Spectre.Console;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CodingTracker.Util;

/// <summary>
/// ApplicationHelper provides utility methods to enhance the user interface interactions and string manipulations.
/// It includes methods for displaying messages, parsing enums, and splitting camel case strings.
/// </summary>
public class AppUtil
{
    /// <summary>
    /// Writes a markup message to the console with a newline.
    /// </summary>
    /// <param name="message">Markup object containing the message to be displayed.</param>
    public static void AnsiWriteLine(Markup message)
    {
        AnsiConsole.Write(message);
        Console.WriteLine();
    }

    /// <summary>
    /// Converts a string with spaces into an enum value, ignoring case.
    /// </summary>
    /// <param name="friendlyString">String to be converted into an enum.</param>
    /// <returns>Enum value corresponding to the string.</returns>
    public static T FromFriendlyString<T>(string friendlyString) where T : struct, Enum
    {
        string enumString = friendlyString.Replace(" ", "");
        return Enum.Parse<T>(enumString, true);
    }

    /// <summary>
    /// Splits a camelCase string into words separated by spaces.
    /// </summary>
    /// <param name="input">The camelCase string to split.</param>
    /// <returns>A string with each word separated by a space.</returns>
    public static string SplitCamelCase(string input)
    {
        return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
    }

    /// <summary>
    /// Pauses execution and waits for the user to press any key, displaying a prompt message.
    /// </summary>
    public void PauseForContinueInput()
    {
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    /// <summary>
    /// Seeds the application's database with initial data for habits and log entries.
    /// This method is used prepopulate the database with test data.
    /// </summary>
    public void SeedDatabase(SessionService sessionService)
    {
        AnsiConsole.WriteLine("Starting database seeding...");

        try
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("Processing...", ctx =>
                {
                    // Call to seed sessions
                    DatabaseSeeder.SeedSessions(sessionService, ConfigSettings.NumberOfCodingSessionsToSeed);
                });

            AnsiConsole.Write(new Markup("\n[green]Database seeded successfully![/]\n"));
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Markup($"\n[red]Error seeding database: {ex.Message}[/]\n"));
        }
        finally
        {
            PauseForContinueInput();
        }
    }

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

    public void PrintNewLines(int numOfNewLines)
    {
        for (int i = 0; i < numOfNewLines; i++) { Console.WriteLine(); }
    }
}
