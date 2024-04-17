using CodingTracker.Services;
using System.Data.SQLite;

namespace CodingTracker.Database;

public class DatabaseInitializer
{
    private readonly DatabaseContext _dbContext;

    private static readonly string CreateSessionTable = @"
        CREATE TABLE IF NOT EXISTS tb_CodingSessions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT ,
            DateCreated TEXT NOT NULL,
            DateUpdated TEXT NOT NULL,
            SessionDate TEXT NOT NULL,
            Duration TEXT NOT NULL,
            StartTime TEXT NULL,
            EndTime TEXT NULL
        )";

    public DatabaseInitializer(DatabaseContext context)
    {
        _dbContext = context;
    }

    public void Initialize()
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = new SQLiteCommand(CreateSessionTable, connection, transaction);
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error initializing the database.", ex.Message);
            throw;
        }
    }
}
