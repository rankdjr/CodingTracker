using CodingTracker.Database;
using CodingTracker.Models;
using CodingTracker.Services;
using Dapper;
using System.Text;

namespace CodingTracker.DAO;

public class CodingSessionDAO
{
    private readonly DatabaseContext _dbContext;
    private CodingGoalDAO _codingGoalDAO;

    public CodingSessionDAO(DatabaseContext context, CodingGoalDAO codingGoalDAO)
    {
        _dbContext = context;
        _codingGoalDAO = codingGoalDAO;
    }

    public int InsertNewSession(CodingSessionModel session)
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = @"
                    INSERT INTO tb_CodingSessions (DateCreated, DateUpdated, SessionDate, Duration, StartTime, EndTime)
                    VALUES (@DateCreated, @DateUpdated, @SessionDate, @Duration, @StartTime, @EndTime);
                    SELECT last_insert_rowid();";

                var id = connection.ExecuteScalar<int>(sql, session);
                return id;  // Returns the auto-incremented Id
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error inserting new session.", ex.Message);
            return -1;  // Indicate failure
        }
    }

    public List<CodingSessionModel> GetAllSessionRecords()
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = "SELECT * FROM tb_CodingSessions";
                var sessions = connection.Query<CodingSessionModel>(sql).ToList();
                return sessions;
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error retrieving sessions", ex.Message);
            return new List<CodingSessionModel>();  // Return an empty list in case of error
        }
    }

    public List<CodingSessionModel> ExecutetGetSessionsQuery(string sqlstring)
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = sqlstring;
                var sessions = connection.Query<CodingSessionModel>(sql).ToList();
                return sessions;
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error retrieving sessions", ex.Message);
            return new List<CodingSessionModel>();  // Return an empty list in case of error
        }
    }

    public List<CodingSessionModel> GetSessionsRecords(TimePeriod? periodFilter, int? numOfPeriods, List<(CodingSessionModel.EditableProperties, SortDirection, int)> orderByOptions)
    {
        var sbSql = new StringBuilder("SELECT * FROM tb_CodingSessions");

        if (periodFilter.HasValue && numOfPeriods.HasValue)
        {
            string timePeriodValue = $"{numOfPeriods.Value * Utilities.GetDaysMultiplier(periodFilter.Value)} days";
            sbSql.Append($" WHERE SessionDate >= date('now', '-{timePeriodValue}')");
        }

        if (orderByOptions.Any())
        {
            sbSql.Append(" ORDER BY ");
            sbSql.Append(string.Join(", ", orderByOptions.OrderBy(o => o.Item3)
                .Select(o => $"{o.Item1} {(o.Item2 == SortDirection.ASC ? "ASC" : "DESC")}")));
        }

        return ExecutetGetSessionsQuery(sbSql.ToString());
    }

    public bool DeleteAllSessions()
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = "DELETE FROM tb_CodingSessions";
                int result = connection.Execute(sql);
                return result > 0;  // Returns true if any rows were affected
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error deleting sessions", ex.Message);
            return false;
        }
    }

    public bool DeleteSessionRecord(int sessionId)
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = "DELETE FROM tb_CodingSessions WHERE Id = @SessionId";
                int result = connection.Execute(sql, new { SessionId = sessionId });
                return result > 0;
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage($"Error deleting session with ID {sessionId}", ex.Message);
            return false;
        }
    }

    public bool UpdateSession(CodingSessionModel session)
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = @"
                UPDATE tb_CodingSessions
                SET
                    DateUpdated = @DateUpdated,
                    SessionDate = @SessionDate,
                    Duration = @Duration,
                    StartTime = @StartTime,
                    EndTime = @EndTime
                WHERE Id = @Id;";

                int rowsAffected = connection.Execute(sql, new
                {
                    DateUpdated = DateTime.UtcNow.ToString(ConfigSettings.DateFormatLong),
                    session.SessionDate,
                    session.Duration,
                    session.StartTime,
                    session.EndTime,
                    session.Id
                });

                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage($"Error updating session with ID {session.Id}.", ex.Message);
            return false;
        }
    }

    public SessionStatistics GetSessionStatistics(TimePeriod period, int numberOfPeriods)
    {
        string sql = $@"
        SELECT 
            COUNT(*) AS TotalSessions, 
            AVG(Duration) AS AverageDuration 
        FROM tb_CodingSessions
        WHERE SessionDate >= date('now', '-{Utilities.GetDaysMultiplier(period) * numberOfPeriods} days')";

        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                var result = connection.QuerySingleOrDefault<SessionStatistics>(sql);
                if (result == null)
                {
                    return new SessionStatistics { TotalSessions = 0, AverageDuration = 0.0 };
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error retrieving session statistics", ex.Message);
            return new SessionStatistics();  // Return an empty/default SessionStatistics in case of error
        }
    }
}
