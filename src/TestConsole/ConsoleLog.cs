using System;

namespace TestConsole;

/// <summary>
/// User-facing console output: information and warnings go to stdout, errors go to stderr.
/// </summary>
internal static class ConsoleLog
{
    private const string WarningPrefix = "Предупреждение: ";
    private const string ErrorPrefix = "Ошибка: ";

    public static void Info(string message) => Console.WriteLine(message);

    public static void Warning(string message) => Console.WriteLine(WarningPrefix + message);

    public static void Error(string message) => Console.Error.WriteLine(ErrorPrefix + message);
}
