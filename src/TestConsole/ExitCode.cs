namespace TestConsole;

/// <summary>
/// Process exit codes of TestConsole.exe.
/// </summary>
internal enum ExitCode
{
    /// <summary>TestApp was started. Warnings may have been reported.</summary>
    Success = 0,

    /// <summary>Another TestConsole.exe instance is already running.</summary>
    AlreadyRunning = 1,

    /// <summary>The search failed or no directory with both TestApp files exists.</summary>
    TestAppNotFound = 2,

    /// <summary>TestApp.exe was found but could not be started.</summary>
    LaunchFailed = 3,

    /// <summary>An error that no step handles.</summary>
    UnexpectedError = 4,
}
