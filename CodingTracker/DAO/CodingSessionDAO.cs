using CodingTracker.Database;
using CodingTracker.Models;
using CodingTracker.Services;
using Dapper;

namespace CodingTracker.DAO;

public class CodingSessionDAO
{
    private readonly DatabaseContext dbContext;

    public CodingSessionDAO(DatabaseContext context)
    {
        dbContext = context;
    }

    public int InsertNewSession(CodingSessionModel session)
    {
        try
        {
            using (var connection = dbContext.GetNewDatabaseConnection())
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
            using (var connection = dbContext.GetNewDatabaseConnection())
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

    public bool DeleteAllSessions()
    {
        try
        {
            using (var connection = dbContext.GetNewDatabaseConnection())
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
            using (var connection = dbContext.GetNewDatabaseConnection())
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
            using (var connection = dbContext.GetNewDatabaseConnection())
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

}
