using System.Globalization;

namespace TestConsole;

/// <summary>
/// Command-line arguments of TestConsole.exe: exactly two integers.
/// </summary>
internal readonly struct LaunchArguments
{
    private const int ExpectedCount = 2;

    public LaunchArguments(int arg1, int arg2)
    {
        Arg1 = arg1;
        Arg2 = arg2;
    }

    /// <summary>Value for the <c>&lt;Arg1&gt;</c> element.</summary>
    public int Arg1 { get; }

    /// <summary>Value for the <c>&lt;Arg2&gt;</c> element.</summary>
    public int Arg2 { get; }

    /// <summary>
    /// Parses <paramref name="args"/> as exactly two <see cref="int"/> values, independent of the system culture.
    /// </summary>
    /// <returns><see langword="true"/> if parsing succeeded.</returns>
    public static bool TryParse(string[] args, out LaunchArguments result)
    {
        result = default;
        if (args.Length != ExpectedCount || !TryParseInt(args[0], out int arg1) || !TryParseInt(args[1], out int arg2))
        {
            return false;
        }

        result = new LaunchArguments(arg1, arg2);
        return true;
    }

    private static bool TryParseInt(string value, out int number) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
}
