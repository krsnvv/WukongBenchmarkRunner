namespace WukongBenchmarkRunner.Models;

public sealed class BenchmarkReport
{
    public required HardwareInfo Hardware { get; init; }
    public required BenchmarkProfile CpuProfile { get; init; }
    public required BenchmarkResult CpuResult { get; init; }
    public required BenchmarkProfile GpuProfile { get; init; }
    public required BenchmarkResult GpuResult { get; init; }
}
