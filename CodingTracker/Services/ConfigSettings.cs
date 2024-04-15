using System.Configuration;

namespace CodingTracker.Services;

public class ConfigSettings
{
    public static string DbFilePath => ConfigurationManager.AppSettings["DatabaseFilePath"]!;
    public static string DbConnectionString => ConfigurationManager.AppSettings["DatabaseConnectionString"]!;
    public static string DateFormatShort => ConfigurationManager.AppSettings["DateFormatShort"]!;
    public static string DateFormatLong => ConfigurationManager.AppSettings["DateFormatLong"]!;
    public static string TimeFormat => ConfigurationManager.AppSettings["TimeFormat"]!;
    public static int NumberOfCodingSessionsToSeed => Int32.Parse(ConfigurationManager.AppSettings["NumberOfCodingSessionsToSeed"]!);

}
