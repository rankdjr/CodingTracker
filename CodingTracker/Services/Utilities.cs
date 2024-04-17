using Spectre.Console;
using System.Text.RegularExpressions;

namespace CodingTracker.Services;

public static class Utilities
{
    public static void AnsiWriteLine(Markup message)
    {
        AnsiConsole.Write(message);
        Console.WriteLine();
    }

    public static T FromFriendlyString<T>(string friendlyString) where T : struct, Enum
    {
        string enumString = friendlyString.Replace(" ", "");
        return Enum.Parse<T>(enumString, true);
    }

    public static string SplitCamelCase(string input)
    {
        return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
    }

    public static void PrintNewLines(int numOfNewLines)
    {
        for (int i = 0; i < numOfNewLines; i++) { Console.WriteLine(); }
    }

    public static void DisplayExceptionErrorMessage(string message, string exception) 
    {
        PrintNewLines(1);
        string errorMessage = $"[red]{message}[/]\n{exception}";
        AnsiConsole.MarkupLine(errorMessage);
        PrintNewLines(1);
    }
    public static void DisplaySuccessMessage(string message)
    {
        PrintNewLines(1);
        string successMessage = $"[chartreuse1]{message}[/]";
        AnsiConsole.MarkupLine(successMessage);
        PrintNewLines(1);
    }
    public static void DisplayWarningMessage(string message)
    {
        PrintNewLines(1);
        string successMessage = $"[lightgoldenrod2_2]{message}[/]";
        AnsiConsole.MarkupLine(successMessage);
        PrintNewLines(1);
    }

    public static void DisplayCancellationMessage(string message)
    {
        PrintNewLines(1);
        string successMessage = $"[blueviolet]{message}[/]";
        AnsiConsole.MarkupLine(successMessage);
        PrintNewLines(1);
    }
}
