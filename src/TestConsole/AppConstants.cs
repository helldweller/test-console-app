namespace TestConsole;

/// <summary>
/// Fixed values of the launcher.
/// </summary>
internal static class AppConstants
{
    /// <summary>Directory whose nested subdirectories are searched for TestApp versions.</summary>
    public const string RootDirectory = @"C:\Prog\";

    /// <summary>TestApp configuration file name.</summary>
    public const string ConfigFileName = "config.xml";

    /// <summary>TestApp executable file name.</summary>
    public const string AppFileName = "TestApp.exe";

    /// <summary>Element in <see cref="ConfigFileName"/> that receives the first argument.</summary>
    public const string Arg1ElementName = "Arg1";

    /// <summary>Element in <see cref="ConfigFileName"/> that receives the second argument.</summary>
    public const string Arg2ElementName = "Arg2";

    /// <summary>
    /// Machine-wide mutex name. The <c>Global\</c> prefix makes it shared by all sessions and users.
    /// </summary>
    public const string SingleInstanceMutexName = @"Global\TestConsole";
}
