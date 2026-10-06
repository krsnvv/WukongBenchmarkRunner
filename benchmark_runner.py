import os
import subprocess

from benchmark_locator import find_benchmark_install_dir


def find_benchmark_executable() -> str:
    install_dir = find_benchmark_install_dir()

    direct_path = os.path.join(
        install_dir,
        "b1_benchmark.exe",
    )

    if os.path.isfile(direct_path):
        return direct_path

    # На случай, если exe находится глубже
    for root, _, files in os.walk(install_dir):
        if "b1_benchmark.exe" in files:
            return os.path.join(
                root,
                "b1_benchmark.exe",
            )

    raise FileNotFoundError(
        "b1_benchmark.exe не найден."
    )


def run_benchmark_tool() -> int:
    exe_path = find_benchmark_executable()

    print("Benchmark executable:")
    print(exe_path)
    print()

    process = subprocess.Popen(
        [exe_path],
        cwd=os.path.dirname(exe_path),
    )

    print(
        f"Benchmark Tool запущен. PID: {process.pid}"
    )

    print(
        "Ожидание закрытия Benchmark Tool..."
    )

    exit_code = process.wait()

    print(
        f"Benchmark Tool закрыт. Exit code: {exit_code}"
    )

    return exit_code