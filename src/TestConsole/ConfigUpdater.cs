using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TestConsole;

/// <summary>
/// Writes <see cref="LaunchArguments"/> into the TestApp config file.
/// </summary>
internal static class ConfigUpdater
{
    private const string TempFileSuffix = ".tmp";

    /// <summary>
    /// Sets every <c>&lt;Arg1&gt;</c> and <c>&lt;Arg2&gt;</c> element of the file, at any depth and in any XML namespace.
    /// The change is all-or-nothing: if either element is missing, the file is left untouched.
    /// </summary>
    /// <returns><see langword="false"/> if <c>&lt;Arg1&gt;</c> or <c>&lt;Arg2&gt;</c> is missing.</returns>
    /// <exception cref="IOException">The file cannot be read or replaced.</exception>
    /// <exception cref="UnauthorizedAccessException">No permission to read or replace the file.</exception>
    /// <exception cref="System.Xml.XmlException">The file is not valid XML.</exception>
    public static bool TryApply(string configPath, LaunchArguments arguments)
    {
        XDocument document = XDocument.Load(configPath, LoadOptions.PreserveWhitespace);
        List<XElement> arg1Elements = FindElements(document, AppConstants.Arg1ElementName);
        List<XElement> arg2Elements = FindElements(document, AppConstants.Arg2ElementName);

        if (arg1Elements.Count == 0 || arg2Elements.Count == 0)
        {
            return false;
        }

        SetValue(arg1Elements, arguments.Arg1);
        SetValue(arg2Elements, arguments.Arg2);
        SaveAtomically(document, configPath);
        return true;
    }

    private static List<XElement> FindElements(XDocument document, string localName) =>
        document.Descendants().Where(element => element.Name.LocalName == localName).ToList();

    private static void SetValue(IEnumerable<XElement> elements, int value)
    {
        string text = value.ToString(CultureInfo.InvariantCulture);
        foreach (XElement element in elements)
        {
            element.Value = text;
        }
    }

    /// <summary>
    /// Writes to a temp file next to <paramref name="path"/> and swaps it in,
    /// so an interrupted write never corrupts the original file.
    /// </summary>
    private static void SaveAtomically(XDocument document, string path)
    {
        string tempPath = path + TempFileSuffix;
        try
        {
            document.Save(tempPath, SaveOptions.DisableFormatting);
            File.Replace(tempPath, path, destinationBackupFileName: null);
        }
        finally
        {
            DeleteQuietly(tempPath);
        }
    }

    private static void DeleteQuietly(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best effort: a leftover temp file is harmless and must not hide the original error.
        }
    }
}
