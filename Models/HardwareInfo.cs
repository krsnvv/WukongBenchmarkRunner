namespace WukongBenchmarkRunner.Models;

public sealed class HardwareInfo
{
    public required string Cpu { get; init; }
    public required string Gpu { get; init; }
    public required string Ram { get; init; }
    public required string Os { get; init; }
}
