import sys
import time

from window_controller import (
    find_benchmark_window,
    focus_window,
    press_key,
    VK_TAB,
    VK_UP,
    VK_DOWN,
    VK_LEFT,
    VK_RIGHT,
    VK_RETURN,
    VK_SPACE,
)


KEYS = {
    "tab": VK_TAB,
    "up": VK_UP,
    "down": VK_DOWN,
    "left": VK_LEFT,
    "right": VK_RIGHT,
    "enter": VK_RETURN,
    "space": VK_SPACE,
}


if len(sys.argv) != 2:
    print("Использование:")
    print("python test_key.py tab")
    print("python test_key.py down")
    print("python test_key.py up")
    print("python test_key.py enter")
    raise SystemExit(1)


key_name = sys.argv[1].lower()

if key_name not in KEYS:
    raise ValueError(
        f"Неизвестная клавиша: {key_name}"
    )


hwnd = find_benchmark_window()

if hwnd is None:
    raise RuntimeError(
        "Окно Benchmark Tool не найдено."
    )


focus_window(hwnd)

print(
    f"Через 2 секунды отправляю: {key_name}"
)

time.sleep(2)

press_key(KEYS[key_name])

print("Готово.")