using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace TestConsole;

/// <summary>
/// Finds the newest TestApp version under a root directory.
/// </summary>
internal static class TestAppLocator
{
    /// <summary>
    /// Searches all nested subdirectories of <paramref name="rootDirectory"/> (the root itself excluded)
    /// for directories that contain both the config file and the executable,
    /// and returns the one whose config file was modified last.
    /// </summary>
    /// <param name="rootDirectory">Directory to search.</param>
    /// <param name="onDirectorySkipped">Called for each subdirectory that cannot be listed; the search continues.</param>
    /// <returns>The newest installation, or <see langword="null"/> if none exists.</returns>
    /// <exception cref="IOException">The root directory does not exist or cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">No permission to list the root directory.</exception>
    public static TestAppInstallation? FindLatest(string rootDirectory, Action<string, Exception> onDirectorySkipped) =>
        EnumerateSubdirectories(rootDirectory, onDirectorySkipped)
            .Select(directory => new TestAppInstallation(directory))
            .Where(installation => installation.IsComplete)
            .OrderByDescending(installation => File.GetLastWriteTimeUtc(installation.ConfigPath))
            .FirstOrDefault();

    /// <summary>
    /// Depth-first walk that skips unreadable subdirectories instead of aborting,
    /// unlike <see cref="SearchOption.AllDirectories"/> on .NET Framework.
    /// </summary>
    private static IEnumerable<string> EnumerateSubdirectories(string rootDirectory, Action<string, Exception> onDirectorySkipped)
    {
        // Listing the root is deliberately unguarded: an unreadable root must fail the search.
        var pending = new Stack<string>(Directory.GetDirectories(rootDirectory));

        while (pending.Count > 0)
        {
            string directory = pending.Pop();
            yield return directory;

            string[] children;
            try
            {
                children = Directory.GetDirectories(directory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
            {
                onDirectorySkipped(directory, ex);
                continue;
            }

            foreach (string child in children)
            {
                pending.Push(child);
            }
        }
    }
}
