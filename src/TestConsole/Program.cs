using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Xml;

namespace TestConsole;

/// <summary>
/// Entry point. Runs the launch algorithm:
/// 1) single instance check, 2) TestApp search, 3) argument check,
/// 4) config update, 5) TestApp start.
/// </summary>
internal static class Program
{
    private const string UsageHint = "TestConsole.exe <Arg1> <Arg2>";

    private static int Main(string[] args)
    {
        try
        {
            return (int)Run(args);
        }
        catch (Exception ex)
        {
            // Last-resort handler: report any failure instead of crashing with an unhandled-exception dialog.
            ConsoleLog.Error($"Непредвиденная ошибка: {ex}");
            return (int)ExitCode.UnexpectedError;
        }
    }

    private static ExitCode Run(string[] args)
    {
        using SingleInstanceGuard? guard = AcquireSingleInstance();
        if (guard is null)
        {
            return ExitCode.AlreadyRunning;
        }

        TestAppInstallation? installation = LocateTestApp();
        if (installation is null)
        {
            return ExitCode.TestAppNotFound;
        }

        if (LaunchArguments.TryParse(args, out LaunchArguments arguments))
        {
            UpdateConfig(installation, arguments);
        }
        else
        {
            ConsoleLog.Warning($"Ожидаются 2 целочисленных аргумента: {UsageHint}. {AppConstants.ConfigFileName} не изменён.");
        }

        return StartTestApp(installation) ? ExitCode.Success : ExitCode.LaunchFailed;
    }

    /// <summary>Step 1.</summary>
    private static SingleInstanceGuard? AcquireSingleInstance()
    {
        SingleInstanceGuard? guard;
        try
        {
            guard = SingleInstanceGuard.TryAcquire(AppConstants.SingleInstanceMutexName);
        }
        catch (UnauthorizedAccessException)
        {
            ConsoleLog.Error("TestConsole.exe уже запущен (другим пользователем).");
            return null;
        }

        if (guard is null)
        {
            ConsoleLog.Error("TestConsole.exe уже запущен.");
        }

        return guard;
    }

    /// <summary>Step 2.</summary>
    private static TestAppInstallation? LocateTestApp()
    {
        TestAppInstallation? installation;
        try
        {
            installation = TestAppLocator.FindLatest(AppConstants.RootDirectory, ReportSkippedDirectory);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            ConsoleLog.Error($"Ошибка поиска в {AppConstants.RootDirectory}: {ex.Message}");
            return null;
        }

        if (installation is null)
        {
            ConsoleLog.Error($"В подкаталогах {AppConstants.RootDirectory} не найдены {AppConstants.ConfigFileName} и {AppConstants.AppFileName}.");
            return null;
        }

        ConsoleLog.Info($"Найдена версия: {installation.DirectoryPath}");
        return installation;
    }

    private static void ReportSkippedDirectory(string directory, Exception error) =>
        ConsoleLog.Warning($"Каталог {directory} пропущен: {error.Message}");

    /// <summary>Step 4. Any failure is only a warning: TestApp still starts with its current config.</summary>
    private static void UpdateConfig(TestAppInstallation installation, LaunchArguments arguments)
    {
        try
        {
            if (ConfigUpdater.TryApply(installation.ConfigPath, arguments))
            {
                ConsoleLog.Info($"{AppConstants.ConfigFileName} обновлён: Arg1={arguments.Arg1}, Arg2={arguments.Arg2}.");
            }
            else
            {
                ConsoleLog.Warning($"В {installation.ConfigPath} нет тэга <{AppConstants.Arg1ElementName}> или <{AppConstants.Arg2ElementName}>. Файл не изменён.");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or XmlException or SecurityException)
        {
            ConsoleLog.Warning($"Не удалось обновить {installation.ConfigPath}: {ex.Message}");
        }
    }

    /// <summary>Step 5.</summary>
    private static bool StartTestApp(TestAppInstallation installation)
    {
        try
        {
            TestAppLauncher.Start(installation);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            ConsoleLog.Error($"Не удалось запустить {AppConstants.AppFileName}: {ex.Message}");
            return false;
        }

        ConsoleLog.Info($"{AppConstants.AppFileName} запущен.");
        return true;
    }
}
