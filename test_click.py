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

print("Окно найдено.")
print("Через 3 секунды кликну Benchmark...")

time.sleep(3)

click_relative(
    hwnd,
    0.08,   # 8% ширины окна
    0.45,   # 45% высоты окна
)

print("Клик выполнен.")