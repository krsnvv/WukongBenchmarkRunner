import os
import subprocess
import time

from benchmark_runner import find_benchmark_executable
from window_controller import (
    find_benchmark_window,
    focus_window,
    press_enter,
)


print("Ищу уже открытый Benchmark Tool...")

hwnd = find_benchmark_window()


if hwnd is None:
    print("Окно не найдено.")
    print("Запускаю Benchmark Tool...")

    exe_path = find_benchmark_executable()

    subprocess.Popen(
        [exe_path],
        cwd=os.path.dirname(exe_path),
    )

    print("Жду появления UnrealWindow...")

    for attempt in range(30):
        time.sleep(1)

        hwnd = find_benchmark_window()

        if hwnd is not None:
            break


if hwnd is None:
    raise RuntimeError(
        "Benchmark Tool запущен, "
        "но окно UnrealWindow не найдено за 30 секунд."
    )


print()
print("Benchmark Tool найден.")
print("HWND:", hwnd)

focused = focus_window(hwnd)

print(
    "SetForegroundWindow:",
    focused,
)

print()
print("Через 5 секунд будет отправлен Enter.")
print("Смотри на окно Benchmark Tool.")

time.sleep(5)

press_enter()

print("Enter отправлен.")