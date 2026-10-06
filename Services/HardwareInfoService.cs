using System.Runtime.InteropServices;
using Microsoft.Win32;
using WukongBenchmarkRunner.Interop;
using WukongBenchmarkRunner.Models;

namespace WukongBenchmarkRunner.Services;

public sealed class HardwareInfoService
{
    public HardwareInfo GetHardwareInfo()
    {
        return new HardwareInfo
        {
            Cpu = GetCpuName(),
            Gpu = GetGpuName(),
            Ram = GetRam(),
            Os = RuntimeInformation.OSDescription
        };
    }

    private static string GetCpuName()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");

        return key?.GetValue("ProcessorNameString")?.ToString()?.Trim()
               ?? "Не удалось определить CPU";
    }

    private static string GetGpuName()
    {
        var names = new List<string>();

        for (uint i = 0; i < 16; i++)
        {
            var device = new NativeMethods.DISPLAY_DEVICE
            {
                cb = Marshal.SizeOf<NativeMethods.DISPLAY_DEVICE>()
            };

            if (!NativeMethods.EnumDisplayDevices(null, i, ref device, 0))
                break;

            if (!string.IsNullOrWhiteSpace(device.DeviceString) &&
                !names.Contains(device.DeviceString, StringComparer.OrdinalIgnoreCase))
            {
                names.Add(device.DeviceString.Trim());
            }
        }

        return names.Count == 0
            ? "Не удалось определить GPU"
            : string.Join("; ", names);
    }

    private static string GetRam()
    {
        var memory = NativeMethods.MEMORYSTATUSEX.Create();

        if (!NativeMethods.GlobalMemoryStatusEx(ref memory))
            return "Не удалось определить RAM";

        var gib = memory.ullTotalPhys / 1024d / 1024d / 1024d;
        return $"{gib:F1} GB";
    }
}
