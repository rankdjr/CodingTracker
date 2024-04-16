public enum DatePrompt
{
    Short,
    Long,
}
public enum DateTimePrompt
{
    DateOnly,   // For date only using "yyyy-MM-dd"
    DateTime,    // For date and time using "yyyy-MM-dd HH:mm:ss"
}

public enum TimePeriod
{
    Days,
    Weeks,
    Years,
}

public enum QueryOptions
{
    FilterByTimePeriod,
    OrderBy,
}

