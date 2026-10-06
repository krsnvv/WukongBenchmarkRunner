using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WukongBenchmarkRunner.Services;

public sealed class BenchmarkLocator
{
    private const string AppId = "3132990";

    public string FindInstallDirectory()
    {
        var steamPath = FindSteamPath();
        var libraries = FindSteamLibraries(steamPath);

        foreach (var library in libraries)
        {
            var steamAppsPath = Path.Combine(library, "steamapps");
            var manifestPath = Path.Combine(steamAppsPath, $"appmanifest_{AppId}.acf");

            if (!File.Exists(manifestPath))
                continue;

            var manifest = File.ReadAllText(manifestPath);
            var match = Regex.Match(manifest, @"""installdir""\s+""([^""]+)""");

            if (!match.Success)
                continue;

            var installDirName = match.Groups[1].Value;
            var installDirectory = Path.Combine(steamAppsPath, "common", installDirName);

            if (Directory.Exists(installDirectory))
                return installDirectory;
        }

        throw new DirectoryNotFoundException("Black Myth: Wukong Benchmark Tool не найден.");
    }

    public string FindConfigPath()
    {
        var configPath = Path.Combine(
            FindInstallDirectory(),
            "b1", "Saved", "Config", "Windows", "GameUserSettings.ini");

        if (!File.Exists(configPath))
            throw new FileNotFoundException("GameUserSettings.ini не найден.", configPath);

        return configPath;
    }

    public string FindExecutablePath()
    {
        var exePath = Path.Combine(FindInstallDirectory(), "b1_benchmark.exe");

        if (!File.Exists(exePath))
            throw new FileNotFoundException("b1_benchmark.exe не найден.", exePath);

        return exePath;
    }

    public string FindSavedDirectory()
    {
        var path = Path.Combine(FindInstallDirectory(), "b1", "Saved");
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Saved directory не найден: {path}");
        return path;
    }

    private static string FindSteamPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        var steamPath = key?.GetValue("SteamPath") as string;

        if (string.IsNullOrWhiteSpace(steamPath))
            throw new DirectoryNotFoundException("Steam не найден в реестре Windows.");

        steamPath = steamPath.Replace('/', Path.DirectorySeparatorChar);

        if (!Directory.Exists(steamPath))
            throw new DirectoryNotFoundException($"Папка Steam не существует: {steamPath}");

        return steamPath;
    }

    private static List<string> FindSteamLibraries(string steamPath)
    {
        var result = new List<string> { steamPath };
        var libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

        if (!File.Exists(libraryFile))
            return result;

        var text = File.ReadAllText(libraryFile);
        var matches = Regex.Matches(text, @"""path""\s+""([^""]+)""");

        foreach (Match match in matches)
        {
            var path = match.Groups[1].Value.Replace(@"\\", @"\");

            if (Directory.Exists(path) &&
                !result.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(path);
            }
        }

        return result;
    }
}
