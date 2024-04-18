using CodingTracker.Database;
using CodingTracker.Models;
using CodingTracker.Services;
using Dapper;
using System.Text;

namespace CodingTracker.DAO;

internal class CodingGoalDAO
{
    private readonly DatabaseContext _dbContext;

    public CodingGoalDAO(DatabaseContext context)
    {
        _dbContext = context;
    }

    public int InsertNewGoal(CodingGoalModel goal)
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = @"
                    INSERT INTO tb_CodingGoals (DateCreated, DateUpdated, TargetDuration, CurrentProgress, IsCompleted)
                    VALUES (@DateCreated, @DateUpdated, @TargetDuration, @CurrentProgress, @IsCompleted);
                    SELECT last_insert_rowid();";

                var id = connection.ExecuteScalar<int>(sql, goal);
                return id;  // Returns the auto-incremented Id
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error inserting new goal.", ex.Message);
            return -1;  // Indicate failure
        }
    }

    public List<CodingGoalModel> GetAllGoalRecords()
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = "SELECT * FROM tb_CodingGoals";
                var goals = connection.Query<CodingGoalModel>(sql).ToList();
                return goals;
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error retrieving goals", ex.Message);
            return new List<CodingGoalModel>();  // Return an empty list in case of error
        }
    }

    public List<CodingGoalModel> ExecutetGetGoalsQuery(string sqlstring)
    {
        try
        {
            using (var connection = _dbContext.GetNewDatabaseConnection())
            {
                string sql = sqlstring;
                var goals = connection.Query<CodingGoalModel>(sql).ToList();
                return goals;
            }
        }
        catch (Exception ex)
        {
            Utilities.DisplayExceptionErrorMessage("Error executing goal query", ex.Message);
            return new List<CodingGoalModel>();  // Return an empty list in case of error
        }
    }   
}
