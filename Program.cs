using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

static class Program
{
    const string Root = @"C:\Prog\";
    const string ConfigName = "config.xml";
    const string AppName = "TestApp.exe";

    static int Main(string[] args)
    {
        // 1. Single instance across the whole machine.
        Mutex mutex;
        try
        {
            mutex = new Mutex(true, @"Global\TestConsole", out bool createdNew);
            if (!createdNew)
            {
                mutex.Dispose();
                return Error(1, "TestConsole.exe уже запущен.");
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Mutex exists but was created by another user.
            return Error(1, "TestConsole.exe уже запущен (другим пользователем).");
        }

        using (mutex)
        {
            // 2. Find the actual TestApp version.
            string dir;
            try
            {
                // ponytail: AllDirectories aborts on an inaccessible subfolder; switch to manual recursion if that happens.
                dir = Directory.EnumerateDirectories(Root, "*", SearchOption.AllDirectories)
                    .Where(d => File.Exists(Path.Combine(d, ConfigName)) && File.Exists(Path.Combine(d, AppName)))
                    .OrderByDescending(d => File.GetLastWriteTimeUtc(Path.Combine(d, ConfigName)))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                return Error(2, $"Ошибка поиска в {Root}: {ex.Message}");
            }
            if (dir == null)
                return Error(2, $"В подкаталогах {Root} не найдены {ConfigName} и {AppName}.");

            Console.WriteLine($"Найдена версия: {dir}");

            // 3. Validate arguments; 4. update config.xml.
            if (TryParseArgs(args, out int arg1, out int arg2))
                UpdateConfig(Path.Combine(dir, ConfigName), arg1, arg2);
            else
                Warn("Ожидаются 2 целочисленных аргумента: TestConsole.exe <Arg1> <Arg2>. config.xml не изменён.");

            // 5. Start TestApp.exe and exit.
            try
            {
                Process.Start(new ProcessStartInfo(Path.Combine(dir, AppName)) { WorkingDirectory = dir })?.Dispose();
            }
            catch (Exception ex)
            {
                return Error(3, $"Не удалось запустить {AppName}: {ex.Message}");
            }

            Console.WriteLine($"{AppName} запущен.");
            return 0;
        }
    }

    static bool TryParseArgs(string[] args, out int arg1, out int arg2)
    {
        arg1 = arg2 = 0;
        return args.Length == 2
            && int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out arg1)
            && int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out arg2);
    }

    static void UpdateConfig(string path, int arg1, int arg2)
    {
        try
        {
            var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            var tags1 = doc.Descendants().Where(e => e.Name.LocalName == "Arg1").ToList();
            var tags2 = doc.Descendants().Where(e => e.Name.LocalName == "Arg2").ToList();
            if (tags1.Count == 0 || tags2.Count == 0)
            {
                Warn($"В {path} нет тэга <Arg1> или <Arg2>. Файл не изменён.");
                return;
            }

            tags1.ForEach(e => e.Value = arg1.ToString(CultureInfo.InvariantCulture));
            tags2.ForEach(e => e.Value = arg2.ToString(CultureInfo.InvariantCulture));

            // Write to a temp file first so a failed write never corrupts config.xml.
            var tmp = path + ".tmp";
            doc.Save(tmp, SaveOptions.DisableFormatting);
            File.Replace(tmp, path, null);
            Console.WriteLine($"config.xml обновлён: Arg1={arg1}, Arg2={arg2}.");
        }
        catch (Exception ex)
        {
            Warn($"Не удалось обновить {path}: {ex.Message}");
        }
    }

    static void Warn(string message) => Console.WriteLine("Предупреждение: " + message);

    static int Error(int code, string message)
    {
        Console.Error.WriteLine("Ошибка: " + message);
        return code;
    }
}
