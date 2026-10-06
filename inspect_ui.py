import subprocess
import time

from pywinauto import Desktop

from benchmark_runner import find_benchmark_executable


exe_path = find_benchmark_executable()

print("Запускаю:")
print(exe_path)

process = subprocess.Popen(
    [exe_path],
    cwd=__import__("os").path.dirname(exe_path),
)

print("PID:", process.pid)
print("Жду загрузку окна...")

time.sleep(10)

desktop = Desktop(backend="uia")

print()
print("Найденные окна:")
print("-" * 60)

found = False

for window in desktop.windows():
    title = window.window_text()

    if not title:
        continue

    if (
        "wukong" in title.lower()
        or "black myth" in title.lower()
        or "悟空" in title.lower()
    ):
        found = True

        print("ОКНО:", title)
        print("-" * 60)

        try:
            window.print_control_identifiers()
        except Exception as error:
            print("Не удалось получить элементы:")
            print(error)

if not found:
    print("Окно Benchmark Tool по названию не найдено.")

input("\nНажми Enter, чтобы завершить скрипт...")