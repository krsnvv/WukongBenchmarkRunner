using WukongBenchmarkRunner.Interop;

namespace WukongBenchmarkRunner.Services;

public sealed class WindowAutomation
{
    public IntPtr FindUnrealWindowForProcess(int processId)
    {
        IntPtr result = IntPtr.Zero;

        NativeMethods.EnumWindows((hwnd, _) =>
        {
            NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);

            if (pid != (uint)processId || !NativeMethods.IsWindowVisible(hwnd))
                return true;

            var className = NativeMethods.GetWindowClass(hwnd);
            var title = NativeMethods.GetWindowTitle(hwnd).Trim();

            if (className == "UnrealWindow" && title.Equals("b1", StringComparison.OrdinalIgnoreCase))
            {
                result = hwnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return result;
    }

    public void Focus(IntPtr hwnd)
    {
        NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
        Thread.Sleep(250);
        NativeMethods.SetForegroundWindow(hwnd);
        Thread.Sleep(250);
    }

    public void PressEnter(IntPtr hwnd)
    {
        Focus(hwnd);

        NativeMethods.keybd_event(
            NativeMethods.VK_RETURN,
            0,
            0,
            UIntPtr.Zero);

        Thread.Sleep(50);

        NativeMethods.keybd_event(
            NativeMethods.VK_RETURN,
            0,
            NativeMethods.KEYEVENTF_KEYUP,
            UIntPtr.Zero);
    }

    public void ClickRelative(IntPtr hwnd, double xRatio, double yRatio)
    {
        if (!NativeMethods.GetWindowRect(hwnd, out var rect))
            throw new InvalidOperationException("Не удалось получить размеры окна Benchmark Tool.");

        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        var x = rect.Left + (int)(width * xRatio);
        var y = rect.Top + (int)(height * yRatio);

        Focus(hwnd);
        NativeMethods.SetCursorPos(x, y);
        Thread.Sleep(200);

        NativeMethods.mouse_event(
            NativeMethods.MOUSEEVENTF_LEFTDOWN,
            0, 0, 0, UIntPtr.Zero);

        Thread.Sleep(50);

        NativeMethods.mouse_event(
            NativeMethods.MOUSEEVENTF_LEFTUP,
            0, 0, 0, UIntPtr.Zero);
    }

    public void Close(IntPtr hwnd)
    {
        NativeMethods.PostMessage(hwnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }
}
