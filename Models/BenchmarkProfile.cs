namespace WukongBenchmarkRunner.Models;

public sealed class BenchmarkProfile
{
    public required string Name { get; init; }

    public int ResolutionWidth { get; init; }
    public int ResolutionHeight { get; init; }
    public double ResolutionQuality { get; init; }

    public int QualityLevel { get; init; }

    public int ViewDistance { get; init; }
    public int AntiAliasing { get; init; }
    public int PostProcessing { get; init; }
    public int ShadowQuality { get; init; }
    public int TextureQuality { get; init; }
    public int FxQuality { get; init; }
    public int MaterialQuality { get; init; }
    public int VegetationQuality { get; init; }
    public int GlobalIllumination { get; init; }
    public int ReflectionQuality { get; init; }

    public bool RayTracing { get; init; }
    public int RayTracingLevel { get; init; }
    public bool FrameGeneration { get; init; }

    public int StreamingPoolSize { get; init; }
}
