using System.Diagnostics;
using WukongBenchmarkRunner.Models;

namespace WukongBenchmarkRunner.Services;

public sealed class BenchmarkRunner
{
    private const string ShippingProcessName = "b1-Win64-Shipping";

    private readonly string _executablePath;
    private readonly string _configPath;
    private readonly string _savedDirectory;
    private readonly WindowAutomation _windowAutomation = new();
    private readonly ResultParser _resultParser = new();

    public BenchmarkRunner(
        string executablePath,
        string configPath,
        string savedDirectory)
    {
        _executablePath = executablePath;
        _configPath = configPath;
        _savedDirectory = savedDirectory;
    }

    public async Task<BenchmarkResult> RunAsync(
        string profileName,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_executablePath))
            throw new FileNotFoundException("b1_benchmark.exe не найден.", _executablePath);

        var alreadyRunning = Process.GetProcessesByName(ShippingProcessName);
        if (alreadyRunning.Length > 0)
        {
            foreach (var process in alreadyRunning)
                process.Dispose();

            throw new InvalidOperationException(
                "Benchmark Tool уже запущен. Перед стартом программы его нужно полностью закрыть.");
        }

        var workingDirectory = Path.GetDirectoryName(_executablePath)
            ?? throw new InvalidOperationException("Не удалось определить папку Benchmark Tool.");

        using var launcher = Process.Start(new ProcessStartInfo
        {
            FileName = _executablePath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        }) ?? throw new InvalidOperationException("Не удалось запустить Benchmark Tool.");

        Console.WriteLine($"Launcher PID: {launcher.Id}");

        using var shippingProcess = await WaitForShippingProcessAsync(
            TimeSpan.FromSeconds(45),
            cancellationToken);

        Console.WriteLine($"Основной процесс PID: {shippingProcess.Id}");

        var hwnd = await WaitForWindowAsync(
            shippingProcess.Id,
            TimeSpan.FromSeconds(45),
            cancellationToken);

        await Task.Delay(1500, cancellationToken);
        _windowAutomation.PressEnter(hwnd);

        await Task.Delay(1800, cancellationToken);

        // Пункт Benchmark в главном меню.
        _windowAutomation.ClickRelative(hwnd, 0.08, 0.45);

        await Task.Delay(900, cancellationToken);

        var startedAt = DateTime.Now;

        // Кнопка Confirm в окне подтверждения запуска.
        _windowAutomation.ClickRelative(hwnd, 0.395, 0.595);

        Console.WriteLine($"{profileName}: тест запущен.");

        await WaitForBenchmarkCompletionAsync(
            shippingProcess,
            TimeSpan.FromMinutes(6),
            cancellationToken);

        var finishedAt = DateTime.Now;

        await Task.Delay(2500, cancellationToken);

        var parsed = _resultParser.TryParse(
            _savedDirectory,
            profileName,
            startedAt,
            finishedAt);

        _windowAutomation.Close(hwnd);

        try
        {
            await shippingProcess
                .WaitForExitAsync(cancellationToken)
                .WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
        }
        catch
        {
            if (!shippingProcess.HasExited)
                shippingProcess.Kill(entireProcessTree: true);
        }

        return parsed;
    }

    private static async Task<Process> WaitForShippingProcessAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var processes = Process.GetProcessesByName(ShippingProcessName);
            if (processes.Length > 0)
            {
                for (var i = 1; i < processes.Length; i++)
                    processes[i].Dispose();

                return processes[0];
            }

            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException($"Процесс {ShippingProcessName}.exe не появился.");
    }

    private async Task<IntPtr> WaitForWindowAsync(
        int processId,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hwnd = _windowAutomation.FindUnrealWindowForProcess(processId);
            if (hwnd != IntPtr.Zero)
                return hwnd;

            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException("Окно Benchmark Tool не появилось.");
    }

    private async Task WaitForBenchmarkCompletionAsync(
        Process process,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;

        // Сначала даём тесту гарантированно перейти в основной прогон.
        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        var lastConfigWrite = File.GetLastWriteTimeUtc(_configPath);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (process.HasExited)
                throw new InvalidOperationException("Benchmark Tool закрылся до окончания теста.");

            var currentWrite = File.GetLastWriteTimeUtc(_configPath);
            if (currentWrite > lastConfigWrite)
            {
                Console.WriteLine("Тест завершён: Benchmark Tool обновил GameUserSettings.ini.");
                return;
            }

            await Task.Delay(1000, cancellationToken);
        }

        throw new TimeoutException("Не удалось дождаться окончания теста за 6 минут.");
    }
}
