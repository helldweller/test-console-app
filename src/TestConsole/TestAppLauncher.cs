using System.Diagnostics;

namespace TestConsole;

/// <summary>
/// Starts TestApp.exe.
/// </summary>
internal static class TestAppLauncher
{
    /// <summary>
    /// Starts the executable without waiting for it to exit.
    /// The working directory is the installation directory, so TestApp finds its config by relative path.
    /// </summary>
    /// <exception cref="System.ComponentModel.Win32Exception">The executable cannot be started.</exception>
    public static void Start(TestAppInstallation installation)
    {
        var startInfo = new ProcessStartInfo(installation.ExecutablePath)
        {
            WorkingDirectory = installation.DirectoryPath,

            // Shell execute lets Windows show the UAC prompt if TestApp's manifest requires elevation.
            UseShellExecute = true,
        };

        // Disposing releases only our process handle; TestApp keeps running.
        Process.Start(startInfo)?.Dispose();
    }
}
