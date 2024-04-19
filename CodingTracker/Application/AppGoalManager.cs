using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;
using System.Data;

namespace CodingTracker.Application;

public class AppGoalManager
{
    private InputHandler _inputHandler;
    CodingGoalDAO _codingGoalDAO;
    private bool _running;

    public AppGoalManager(CodingGoalDAO codingGoalDAO, InputHandler inputHandler)
    {
        _codingGoalDAO = codingGoalDAO;
        _inputHandler = inputHandler;
        _running = true;
    }

    public void Run()
    {
        while (_running)
        {
            AnsiConsole.Clear();
            PromptForSessionAction();
        }
    }

    private void PromptForSessionAction()
    {
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select an option:")
                .PageSize(10)
                .AddChoices(Enum.GetNames(typeof(ManageGoalsMenuOptions))
                .Select(Utilities.SplitCamelCase)));

        ExecuteSelectedOption(selectedOption);
    }

    private void ExecuteSelectedOption(string option)
    {
        switch (Enum.Parse<ManageGoalsMenuOptions>(option.Replace(" ", "")))
        {
            case ManageGoalsMenuOptions.ShowProgress:
                HandleShowProgressAction();
                break;
            case ManageGoalsMenuOptions.SetNewGoal:
                // do stuff
                break;
            case ManageGoalsMenuOptions.DeleteGoal:
                DeleteSpecificGoal();
                break;
            case ManageGoalsMenuOptions.DeleteAllGoals:
                DeleteAllGoals();
                break;
            case ManageGoalsMenuOptions.ReturnToMainMenu:
                _running = false;
                break;
        }
    }

    private void HandleShowProgressAction()
    {
        ShowGoalProgress();
        _inputHandler.PauseForContinueInput();
    }

    private void ShowGoalProgress()
    {
        List<CodingGoalModel> codingGoals = _codingGoalDAO.GetAllGoalRecords();
        List<(BreakdownChart, string)> breakdownCharts = BuildCodingGoalsBreakdownCharts(codingGoals);
        PrintBreakDownCharts(breakdownCharts);
    }

    private void PrintBreakDownCharts(List<(BreakdownChart, string)> charts)
    {
        Utilities.PrintNewLines(1);

        foreach (var chart in charts)
        {
            string chartHeader = $"[darkslategray2]  {chart.Item2}  [/]";

            var panel = new Panel(chart.Item1)
                .Header(chartHeader)
                .HeaderAlignment(Justify.Left)
                .BorderColor(Color.Grey);

            AnsiConsole.Write(panel);
        }

        Utilities.PrintNewLines(2);
    }

    private List<(BreakdownChart, string)> BuildCodingGoalsBreakdownCharts(List<CodingGoalModel> codingGoals)
    {
        List<(BreakdownChart, string)> breakdownCharts = new List<(BreakdownChart, string)>();

        foreach (var goal in codingGoals)
        {
            string goalDescription = goal.Description!;
            Color progressBarColor = Color.LightGreen;
            Color targetDurationBarColor = Color.Grey;

            int progressValue;
            int totalMinutesTarget;

            goal.GetProgressAsIntervals(out progressValue, out totalMinutesTarget);

            // TODO: Remove after testing
            Random random = new Random();
            goal.UpdateProgress(TimeSpan.FromMinutes(random.Next(1, totalMinutesTarget)));
            goal.GetProgressAsIntervals(out progressValue, out totalMinutesTarget);
            // end test code

            BreakdownChart breakdownChart = new BreakdownChart()
                                .Width((int)(Console.WindowWidth * 0.65))
                                .AddItem("", progressValue, progressBarColor)
                                .AddItem("", totalMinutesTarget, targetDurationBarColor);

            breakdownCharts.Add(new (breakdownChart, goal.Description!));
        }

        return breakdownCharts;
    }

    private void DeleteSpecificGoal()
    {
        List<CodingGoalModel> codingGoals = _codingGoalDAO.GetAllGoalRecords();
        if (codingGoals.Count == 0)
        {
            Utilities.DisplayWarningMessage("No goals found!");
            _inputHandler.PauseForContinueInput();
            return;
        }

        var selectedGoal = _inputHandler.PromptForGoalSelection(codingGoals, "Select a goal to delete:");

        bool goalDeletedResult = _codingGoalDAO.DeleteGoal(selectedGoal.Id);
        if (goalDeletedResult == true)
        {
            Utilities.DisplaySuccessMessage("Goal successfully deleted!");
        }
        else
        {
            Utilities.DisplayWarningMessage("Goal was not deleted.");
        }

        _inputHandler.PauseForContinueInput();
    }

    private void SetNewGoal()
    {

    }

    private void DeleteAllGoals()
    {
        bool goalDeletedResult = _codingGoalDAO.DeleteAllGoals();
        if (goalDeletedResult == true)
        {
            Utilities.DisplaySuccessMessage("Goals successfully deleted!");
        }
        else
        {
            Utilities.DisplayWarningMessage("No goals were deleted. (The table may have been empty).");
        }


        _inputHandler.PauseForContinueInput();
    }
}
