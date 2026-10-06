using WukongBenchmarkRunner.Models;
using WukongBenchmarkRunner.Reports;
using WukongBenchmarkRunner.Services;

try
{
    var cpuProfile = new BenchmarkProfile
    {
        Name = "CPU",
        ResolutionWidth = 1280,
        ResolutionHeight = 720,
        ResolutionQuality = 50,
        QualityLevel = 1,
        ViewDistance = 1,
        AntiAliasing = 1,
        PostProcessing = 1,
        ShadowQuality = 1,
        TextureQuality = 1,
        FxQuality = 1,
        MaterialQuality = 1,
        VegetationQuality = 1,
        GlobalIllumination = 1,
        ReflectionQuality = 1,
        RayTracing = false,
        RayTracingLevel = 1,
        FrameGeneration = false,
        StreamingPoolSize = 384
    };

    var gpuProfile = new BenchmarkProfile
    {
        Name = "GPU",
        ResolutionWidth = 1920,
        ResolutionHeight = 1080,
        ResolutionQuality = 100,
        QualityLevel = 5,
        ViewDistance = 5,
        AntiAliasing = 5,
        PostProcessing = 5,
        ShadowQuality = 5,
        TextureQuality = 5,
        FxQuality = 5,
        MaterialQuality = 5,
        VegetationQuality = 5,
        GlobalIllumination = 5,
        ReflectionQuality = 5,
        RayTracing = true,
        RayTracingLevel = 2,
        FrameGeneration = false,
        StreamingPoolSize = 640
    };

    var locator = new BenchmarkLocator();
    var configPath = locator.FindConfigPath();
    var executablePath = locator.FindExecutablePath();
    var savedDirectory = locator.FindSavedDirectory();

    Console.WriteLine("Black Myth: Wukong Benchmark Runner");
    Console.WriteLine(new string('=', 48));
    Console.WriteLine($"Конфиг: {configPath}");
    Console.WriteLine($"Исполняемый файл: {executablePath}");
    Console.WriteLine();

    var hardware = new HardwareInfoService().GetHardwareInfo();
    Console.WriteLine($"CPU: {hardware.Cpu}");
    Console.WriteLine($"GPU: {hardware.Gpu}");
    Console.WriteLine($"RAM: {hardware.Ram}");
    Console.WriteLine($"ОС:  {hardware.Os}");
    Console.WriteLine();

    var settings = new SettingsManager(configPath);
    var runner = new BenchmarkRunner(executablePath, configPath, savedDirectory);

    Console.WriteLine("Применяю профиль CPU...");
    settings.Apply(cpuProfile);
    var cpuResult = await runner.RunAsync(cpuProfile.Name);
    PrintResult(cpuResult);

    Console.WriteLine();
    Console.WriteLine("Применяю профиль GPU...");
    settings.Apply(gpuProfile);
    var gpuResult = await runner.RunAsync(gpuProfile.Name);
    PrintResult(gpuResult);

    var report = new BenchmarkReport
    {
        Hardware = hardware,
        CpuProfile = cpuProfile,
        CpuResult = cpuResult,
        GpuProfile = gpuProfile,
        GpuResult = gpuResult
    };

    var (jsonPath, textPath) = new ReportWriter().Write(report);

    Console.WriteLine();
    Console.WriteLine("Готово.");
    Console.WriteLine($"JSON-отчёт: {jsonPath}");
    Console.WriteLine($"TXT-отчёт:  {textPath}");
}
catch (Exception exception)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("Ошибка:");
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine(exception);
    Environment.ExitCode = 1;
}

static void PrintResult(BenchmarkResult result)
{
    Console.WriteLine($"Прогон {result.ProfileName} завершён.");
    Console.WriteLine($"Средний FPS: {Format(result.AverageFps)}");
    Console.WriteLine($"Минимальный FPS: {Format(result.MinimumFps)}");
    Console.WriteLine($"Максимальный FPS: {Format(result.MaximumFps)}");
    Console.WriteLine($"Low 5% FPS: {Format(result.Low5PercentFps)}");

    if (result.SourceFile is null)
    {
        Console.WriteLine("FPS не удалось найти в текстовых файлах Benchmark Tool.");
    }
    else
    {
        Console.WriteLine($"Источник результата: {result.SourceFile}");
    }
}

static string Format(double? value) =>
    value is null ? "N/A" : value.Value.ToString("0.##");
