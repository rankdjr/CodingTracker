namespace CodingTracker.DAO;

public struct CodingSessionQuery
{
    public TimePeriod TimePeriod { get; }
    public int PeriodCount { get; }
    public SortField SortField { get; }
    public bool IsAscending { get; }

    public CodingSessionQuery(TimePeriod timePeriod, int periodCount, SortField sortField, bool isAscending)
    {
        TimePeriod = timePeriod;
        PeriodCount = periodCount;
        SortField = sortField;
        IsAscending = isAscending;
    }
}

