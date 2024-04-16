using CodingTracker.Database;
using CodingTracker.Models;
using Dapper;

namespace CodingTracker.Services;

/// <summary>
/// SessionService handles all database operations related to coding sessions, including CRUD operations and queries.
/// It uses the DatabaseContext to create connections and execute SQL commands data.
/// </summary>
public class SessionService
{
    private readonly DatabaseContext dbContext;

    /// <summary>
    /// Constructs a SessionService with a specified DatabaseContext for database operations.
    /// </summary>
    /// <param name="context">Database context to manage connections.</param>
    public SessionService(DatabaseContext context)
    {
        dbContext = context;
    }

    /// <summary>
    /// Inserts a new coding session record into the database and returns a boolean indicating success.
    /// </summary>
    /// <param name="session">The coding session to insert.</param>
    /// <returns>True if the session was successfully added; otherwise, false.</returns>
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
            Console.WriteLine($"Error inserting new session.");
            Console.WriteLine($"{ex.Message}");
            return -1;  // Indicate failure
        }
    }

    /// <summary>
    /// Retrieves all session records from the database.
    /// </summary>
    /// <returns>A list of all coding sessions.</returns>
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
            Console.WriteLine($"Error retrieving sessions: {ex.Message}");
            return new List<CodingSessionModel>();  // Return an empty list in case of error
        }
    }

    /// <summary>
    /// Deletes all coding session records from the database.
    /// </summary>
    /// <returns>True if the records were successfully deleted; otherwise, false.</returns>
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
            Console.WriteLine($"Error deleting sessions: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes a single coding session record from the database based on the specified session ID.
    /// </summary>
    /// <param name="sessionId">The ID of the session to be deleted. This is an integer representing the unique identifier for a session.</param>
    /// <returns>Returns <c>true</c> if the deletion was successful and affected one or more rows in the database; otherwise, returns <c>false</c>.</returns>
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
            Console.WriteLine($"Error deleting session with ID {sessionId}: {ex.Message}");
            return false;
        }
    }
}
