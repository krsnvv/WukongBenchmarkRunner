import time

from window_controller import (
    find_benchmark_window,
    focus_window,
    click_relative,
)


hwnd = find_benchmark_window()

if hwnd is None:
    raise RuntimeError(
        "Окно Benchmark Tool не найдено."
    )

focus_window(hwnd)

print("Через 3 секунды нажимаю Confirm...")

time.sleep(3)

click_relative(
    hwnd,
    0.395,
    0.595,
)

print("Confirm нажат.")