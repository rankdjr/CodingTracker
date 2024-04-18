public enum MainMenuOption
{
    StartNewSession,
    LogManualSession,
    ViewAndEditPreviousSessions,
    ViewAndEditGoals,
    ViewReports,
    SeedDatabase,
    Exit
}

public enum StartSessionMenuOptions
{
    StartSession,
    RefreshElapsedTime,
    EndCurrentSession,
    ReturnToMainMenu
}

public enum LogManualSessionMenuOptions
{
    LogSessionManualSession,
    ReturnToMainMenu
}

public enum ManageSessionsMenuOptions
{
    ViewAllSessions,
    UpdateSessionRecord,
    DeleteSessionRecord,
    DeleteAllSessions,
    ReturnToMainMenu
}

public enum ManageGoalsMenuOptions
{
    ShowProgress,
    SetNewGoal,
    EditExistingGoal,
    DeleteGoal,
    ReturnToMainMenu
}
