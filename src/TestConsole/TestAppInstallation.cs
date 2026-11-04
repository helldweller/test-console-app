using System.IO;

namespace TestConsole;

/// <summary>
/// One TestApp version: a directory and the paths of its files.
/// </summary>
internal sealed class TestAppInstallation
{
    public TestAppInstallation(string directoryPath)
    {
        DirectoryPath = directoryPath;
        ConfigPath = Path.Combine(directoryPath, AppConstants.ConfigFileName);
        ExecutablePath = Path.Combine(directoryPath, AppConstants.AppFileName);
    }

    public string DirectoryPath { get; }

    public string ConfigPath { get; }

    public string ExecutablePath { get; }

    /// <summary>
    /// <see langword="true"/> if both <see cref="ConfigPath"/> and <see cref="ExecutablePath"/> exist.
    /// </summary>
    public bool IsComplete => File.Exists(ConfigPath) && File.Exists(ExecutablePath);
}
