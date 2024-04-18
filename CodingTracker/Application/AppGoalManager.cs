using CodingTracker.DAO;
using CodingTracker.Models;
using CodingTracker.Services;
using Spectre.Console;

namespace CodingTracker.Application;

public class AppGoalManager
{
    private InputHandler _inputHandler;
    CodingGoalDAO _codingGoalDAO;
    private CodingGoalModel? _codingGoalModel;
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
                .AddChoices(Enum.GetNames(typeof(MainMenuOption))
                .Select(Utilities.SplitCamelCase)));

        ExecuteSelectedOption(selectedOption);
    }

    private void ExecuteSelectedOption(string option)
    {
        switch (Enum.Parse<ManageGoalsMenuOptions>(option.Replace(" ", "")))
        {
            case ManageGoalsMenuOptions.ShowProgress:
                // do stuff
                break;
            case ManageGoalsMenuOptions.SetNewGoal:
                // do stuff
                break;
            case ManageGoalsMenuOptions.EditExistingGoal:
                // do stuff
                break;
            case ManageGoalsMenuOptions.DeleteGoal:
                // do stuff
                break;
            case ManageGoalsMenuOptions.ReturnToMainMenu:
                return;
        }
    }
}
