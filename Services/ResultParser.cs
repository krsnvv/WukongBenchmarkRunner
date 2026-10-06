using System.Globalization;
using System.Text.RegularExpressions;
using WukongBenchmarkRunner.Models;

namespace WukongBenchmarkRunner.Services;

public sealed class ResultParser
{
    private static readonly string[] AllowedExtensions =
    [
        ".ini", ".txt", ".log", ".json", ".csv"
    ];

    public BenchmarkResult TryParse(
        string savedDirectory,
        string profileName,
        DateTime startedAt,
        DateTime finishedAt)
    {
        double? average = null;
        double? minimum = null;
        double? maximum = null;
        double? low5 = null;
        double? vram = null;
        string? sourceFile = null;
        string? raw = null;

        var candidates = Directory
            .EnumerateFiles(savedDirectory, "*", SearchOption.AllDirectories)
            .Where(path => AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Select(path => new FileInfo(path))
            .Where(info => info.Length <= 8 * 1024 * 1024)
            .Where(info => info.LastWriteTime >= startedAt.AddMinutes(-1))
            .OrderByDescending(info => info.LastWriteTime)
            .ToList();

        foreach (var file in candidates)
        {
            string text;

            try
            {
                text = File.ReadAllText(file.FullName);
            }
            catch
            {
                continue;
            }

            var localAverage = FindNumber(text,
                @"(?:Average\s*FPS|AverageFPS|Avg\.?\s*FPS)\s*[:=]\s*([0-9]+(?:[\.,][0-9]+)?)");
            var localMinimum = FindNumber(text,
                @"(?:Minimum\s*FPS|MinimumFPS|Min\.?\s*FPS)\s*[:=]\s*([0-9]+(?:[\.,][0-9]+)?)");
            var localMaximum = FindNumber(text,
                @"(?:Maximum\s*FPS|MaximumFPS|Max\.?\s*FPS)\s*[:=]\s*([0-9]+(?:[\.,][0-9]+)?)");
            var localLow5 = FindNumber(text,
                @"(?:Low\s*5(?:th)?|5(?:th)?\s*Percentile)\s*(?:FPS)?\s*[:=]\s*([0-9]+(?:[\.,][0-9]+)?)");
            var localVram = FindNumber(text,
                @"(?:VRAM|Vram)(?:\s*Usage)?\s*[:=]\s*([0-9]+(?:[\.,][0-9]+)?)");

            if (localAverage is null && localMinimum is null && localMaximum is null && localLow5 is null)
                continue;

            average ??= localAverage;
            minimum ??= localMinimum;
            maximum ??= localMaximum;
            low5 ??= localLow5;
            vram ??= localVram;
            sourceFile ??= file.FullName;
            raw ??= string.Join(Environment.NewLine,
                text.Split('\n')
                    .Where(line => Regex.IsMatch(line, "FPS|VRAM|Vram", RegexOptions.IgnoreCase))
                    .Take(20)
                    .Select(line => line.Trim()));

            if (average is not null && minimum is not null && maximum is not null)
                break;
        }

        return new BenchmarkResult
        {
            ProfileName = profileName,
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            AverageFps = average,
            MinimumFps = minimum,
            MaximumFps = maximum,
            Low5PercentFps = low5,
            VramGb = vram,
            SourceFile = sourceFile,
            RawMatchedText = raw
        };
    }

    private static double? FindNumber(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        var value = match.Groups[1].Value.Replace(',', '.');

        return double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var number)
            ? number
            : null;
    }
}
