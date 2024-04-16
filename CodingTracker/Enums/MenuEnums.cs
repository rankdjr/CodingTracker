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
    LogSessionByDateAndDuration,
    LogSessionByStartEndTimes,
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
public enum ReportsMenuOptions
{
    Report1,
    Report2,
    Report3,
    ReturnToMainMenu
}
