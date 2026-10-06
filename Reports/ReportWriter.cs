using System.Text;
using System.Text.Json;
using WukongBenchmarkRunner.Models;

namespace WukongBenchmarkRunner.Reports;

public sealed class ReportWriter
{
    public (string JsonPath, string TextPath) Write(BenchmarkReport report)
    {
        var outputDirectory = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(outputDirectory);

        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var jsonPath = Path.Combine(outputDirectory, $"result-{stamp}.json");
        var textPath = Path.Combine(outputDirectory, $"result-{stamp}.txt");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(jsonPath, JsonSerializer.Serialize(report, options));
        File.WriteAllText(textPath, BuildText(report));

        return (jsonPath, textPath);
    }

    private static string BuildText(BenchmarkReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine("BLACK MYTH: WUKONG — ОТЧЁТ БЕНЧМАРКА");
        sb.AppendLine(new string('=', 50));
        sb.AppendLine($"CPU: {report.Hardware.Cpu}");
        sb.AppendLine($"GPU: {report.Hardware.Gpu}");
        sb.AppendLine($"RAM: {report.Hardware.Ram}");
        sb.AppendLine($"ОС:  {report.Hardware.Os}");
        sb.AppendLine();

        AppendRun(sb, report.CpuProfile, report.CpuResult);
        sb.AppendLine();
        AppendRun(sb, report.GpuProfile, report.GpuResult);

        return sb.ToString();
    }

    private static void AppendRun(
        StringBuilder sb,
        BenchmarkProfile profile,
        BenchmarkResult result)
    {
        sb.AppendLine($"ТЕСТ {profile.Name}");
        sb.AppendLine(new string('-', 50));
        sb.AppendLine($"Разрешение: {profile.ResolutionWidth}x{profile.ResolutionHeight}");
        sb.AppendLine($"Render Scale: {profile.ResolutionQuality}%");
        sb.AppendLine($"Уровень качества: {profile.QualityLevel}");
        sb.AppendLine($"Ray Tracing: {(profile.RayTracing ? "On" : "Off")}");
        sb.AppendLine($"Frame Generation: {(profile.FrameGeneration ? "On" : "Off")}");
        sb.AppendLine();
        sb.AppendLine($"Средний FPS: {Format(result.AverageFps)}");
        sb.AppendLine($"Минимальный FPS: {Format(result.MinimumFps)}");
        sb.AppendLine($"Максимальный FPS: {Format(result.MaximumFps)}");
        sb.AppendLine($"Low 5% FPS: {Format(result.Low5PercentFps)}");
        sb.AppendLine($"VRAM: {Format(result.VramGb)}");
        sb.AppendLine($"Источник результата: {result.SourceFile ?? "не найден"}");
    }

    private static string Format(double? value) =>
        value is null ? "N/A" : value.Value.ToString("0.##");
}
