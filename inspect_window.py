import ctypes
import os
import subprocess
import time

from ctypes import wintypes

from benchmark_runner import find_benchmark_executable


TH32CS_SNAPPROCESS = 0x00000002
MAX_PATH = 260

kernel32 = ctypes.WinDLL("kernel32", use_last_error=True)
user32 = ctypes.WinDLL("user32", use_last_error=True)


class PROCESSENTRY32W(ctypes.Structure):
    _fields_ = [
        ("dwSize", wintypes.DWORD),
        ("cntUsage", wintypes.DWORD),
        ("th32ProcessID", wintypes.DWORD),
        ("th32DefaultHeapID", ctypes.c_size_t),
        ("th32ModuleID", wintypes.DWORD),
        ("cntThreads", wintypes.DWORD),
        ("th32ParentProcessID", wintypes.DWORD),
        ("pcPriClassBase", wintypes.LONG),
        ("dwFlags", wintypes.DWORD),
        ("szExeFile", wintypes.WCHAR * MAX_PATH),
    ]


kernel32.CreateToolhelp32Snapshot.argtypes = [
    wintypes.DWORD,
    wintypes.DWORD,
]
kernel32.CreateToolhelp32Snapshot.restype = wintypes.HANDLE

kernel32.Process32FirstW.argtypes = [
    wintypes.HANDLE,
    ctypes.POINTER(PROCESSENTRY32W),
]
kernel32.Process32FirstW.restype = wintypes.BOOL

kernel32.Process32NextW.argtypes = [
    wintypes.HANDLE,
    ctypes.POINTER(PROCESSENTRY32W),
]
kernel32.Process32NextW.restype = wintypes.BOOL


def get_processes():
    snapshot = kernel32.CreateToolhelp32Snapshot(
        TH32CS_SNAPPROCESS,
        0,
    )

    if snapshot == wintypes.HANDLE(-1).value:
        raise ctypes.WinError(ctypes.get_last_error())

    result = []

    try:
        entry = PROCESSENTRY32W()
        entry.dwSize = ctypes.sizeof(PROCESSENTRY32W)

        success = kernel32.Process32FirstW(
            snapshot,
            ctypes.byref(entry),
        )

        while success:
            result.append(
                {
                    "pid": entry.th32ProcessID,
                    "parent_pid": entry.th32ParentProcessID,
                    "name": entry.szExeFile,
                }
            )

            success = kernel32.Process32NextW(
                snapshot,
                ctypes.byref(entry),
            )

    finally:
        kernel32.CloseHandle(snapshot)

    return result


EnumWindowsProc = ctypes.WINFUNCTYPE(
    wintypes.BOOL,
    wintypes.HWND,
    wintypes.LPARAM,
)


def get_window_title(hwnd):
    length = user32.GetWindowTextLengthW(hwnd)

    buffer = ctypes.create_unicode_buffer(
        length + 1
    )

    user32.GetWindowTextW(
        hwnd,
        buffer,
        len(buffer),
    )

    return buffer.value


def get_window_class(hwnd):
    buffer = ctypes.create_unicode_buffer(256)

    user32.GetClassNameW(
        hwnd,
        buffer,
        len(buffer),
    )

    return buffer.value


def find_windows_for_pids(pids):
    windows = []

    @EnumWindowsProc
    def callback(hwnd, lparam):
        pid = wintypes.DWORD()

        user32.GetWindowThreadProcessId(
            hwnd,
            ctypes.byref(pid),
        )

        if pid.value in pids:
            windows.append(
                {
                    "pid": pid.value,
                    "hwnd": hwnd,
                    "title": get_window_title(hwnd),
                    "class": get_window_class(hwnd),
                    "visible": bool(
                        user32.IsWindowVisible(hwnd)
                    ),
                }
            )

        return True

    user32.EnumWindows(callback, 0)

    return windows


# --------------------------------
# Снимок ДО запуска
# --------------------------------

before = get_processes()
before_pids = {
    process["pid"]
    for process in before
}

exe_path = find_benchmark_executable()

print("Запускаю:")
print(exe_path)
print()

launcher = subprocess.Popen(
    [exe_path],
    cwd=os.path.dirname(exe_path),
)

print("PID launcher:", launcher.pid)
print("Жду загрузку...")

time.sleep(12)


# --------------------------------
# Снимок ПОСЛЕ запуска
# --------------------------------

after = get_processes()

new_processes = [
    process
    for process in after
    if process["pid"] not in before_pids
]


print()
print("=" * 70)
print("НОВЫЕ ПРОЦЕССЫ")
print("=" * 70)

for process in new_processes:
    print(
        f'PID={process["pid"]:<8} '
        f'PARENT={process["parent_pid"]:<8} '
        f'EXE={process["name"]}'
    )


# Дополнительно ищем всё похожее на Black Myth / b1
candidate_processes = []

for process in after:
    name = process["name"].lower()

    if (
        "b1" in name
        or "benchmark" in name
        or "wukong" in name
    ):
        candidate_processes.append(process)


print()
print("=" * 70)
print("КАНДИДАТЫ B1 / BENCHMARK")
print("=" * 70)

for process in candidate_processes:
    print(
        f'PID={process["pid"]:<8} '
        f'PARENT={process["parent_pid"]:<8} '
        f'EXE={process["name"]}'
    )


candidate_pids = {
    process["pid"]
    for process in candidate_processes
}


print()
print("=" * 70)
print("НАЙДЕННЫЕ ОКНА")
print("=" * 70)

windows = find_windows_for_pids(
    candidate_pids
)

if not windows:
    print("Окна не найдены.")
else:
    for window in windows:
        print()
        print("PID:    ", window["pid"])
        print("HWND:   ", window["hwnd"])
        print("TITLE:  ", repr(window["title"]))
        print("CLASS:  ", repr(window["class"]))
        print("VISIBLE:", window["visible"])


input(
    "\nBenchmark Tool не закрывай. "
    "Нажми Enter для завершения..."
)