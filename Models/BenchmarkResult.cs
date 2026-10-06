namespace WukongBenchmarkRunner.Models;

public sealed class BenchmarkResult
{
    public required string ProfileName { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime FinishedAt { get; init; }

    public double? AverageFps { get; init; }
    public double? MinimumFps { get; init; }
    public double? MaximumFps { get; init; }
    public double? Low5PercentFps { get; init; }
    public double? VramGb { get; init; }

    public string? SourceFile { get; init; }
    public string? RawMatchedText { get; init; }
}
