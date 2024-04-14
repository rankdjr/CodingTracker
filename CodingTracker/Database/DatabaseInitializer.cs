namespace CodingTracker.Database;

/// <summary>
/// DatabaseInitializer is responsible for setting up the initial structure of the database.
/// It creates the necessary tables and views required for the application to function.
/// This class ensures that the database is ready for use by the application upon initialization.
/// </summary>
public class DatabaseInitializer
{
    private readonly DatabaseContext dbContext;

    private static readonly string CreateSessionTable = @"
        CREATE TABLE IF NOT EXISTS tb_CodingSessions (
            Id INTEGER PRIMARY KEY,
            DateCreated TEXT NOT NULL,
            DateUpdated TEXT NOT NULL,
            SessionStartTime TEXT NULL,
            SessionEndTime TEXT NULL,
            Duration Float NOT NULL,
        )";

    /// <summary>
    /// Initializes a new instance of the DatabaseInitializer class with a DatabaseContext.
    /// </summary>
    /// <param name="context">The database context used for obtaining database connections.</param>
    public DatabaseInitializer(DatabaseContext context)
    {
        dbContext = context;
    }

    /// <summary>
    /// Executes the SQL commands to create the initial database schema including tables and views.
    /// Uses transactions to ensure that all operations are completed atomically.
    /// Handles any exceptions during initialization and logs them appropriately.
    /// </summary>
    public void Initialize()
    {
        try
        {
            using (var connection = dbContext.GetNewDatabaseConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        DbHelper.ExecuteCommand(CreateSessionTable, connection, transaction: transaction);
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
            Console.WriteLine($"Error initializing the database: {ex.Message}");
            throw;
        }
    }
}
