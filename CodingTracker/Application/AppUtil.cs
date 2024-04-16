using CodingTracker.Application;
using CodingTracker.Database;
using CodingTracker.Services;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace CodingTracker.Util;

/// <summary>
/// ApplicationHelper provides utility methods to enhance the user interface interactions and string manipulations.
/// It includes methods for displaying messages, parsing enums, and splitting camel case strings.
/// </summary>
public class AppUtil
{
    private UserInput _userInput;

    public AppUtil()
    {
        _userInput = new UserInput();
    }

    /// <summary>
    /// Writes a markup message to the console with a newline.
    /// </summary>
    /// <param name="message">Markup object containing the message to be displayed.</param>
    public void AnsiWriteLine(Markup message)
    {
        AnsiConsole.Write(message);
        Console.WriteLine();
    }

    /// <summary>
    /// Converts a string with spaces into an enum value, ignoring case.
    /// </summary>
    /// <param name="friendlyString">String to be converted into an enum.</param>
    /// <returns>Enum value corresponding to the string.</returns>
    public T FromFriendlyString<T>(string friendlyString) where T : struct, Enum
    {
        string enumString = friendlyString.Replace(" ", "");
        return Enum.Parse<T>(enumString, true);
    }

    /// <summary>
    /// Splits a camelCase string into words separated by spaces.
    /// </summary>
    /// <param name="input">The camelCase string to split.</param>
    /// <returns>A string with each word separated by a space.</returns>
    public string SplitCamelCase(string input)
    {
        return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
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
            _userInput.PauseForContinueInput();
        }
    }

    public void PrintNewLines(int numOfNewLines)
    {
        for (int i = 0; i < numOfNewLines; i++) { Console.WriteLine(); }
    }
}
