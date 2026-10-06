import os
import re
import winreg


APP_ID = "3132990"


def find_steam_path() -> str:
    key_path = r"Software\Valve\Steam"

    with winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path) as key:
        steam_path, _ = winreg.QueryValueEx(key, "SteamPath")

    steam_path = steam_path.replace("/", os.sep)

    if not os.path.isdir(steam_path):
        raise FileNotFoundError(
            f"Папка Steam не найдена: {steam_path}"
        )

    return steam_path


def find_steam_libraries(steam_path: str) -> list[str]:
    libraries = [steam_path]

    library_file = os.path.join(
        steam_path,
        "steamapps",
        "libraryfolders.vdf",
    )

    if not os.path.isfile(library_file):
        return libraries

    with open(
        library_file,
        "r",
        encoding="utf-8",
        errors="ignore",
    ) as file:
        text = file.read()

    matches = re.findall(
        r'"path"\s+"([^"]+)"',
        text,
    )

    for path in matches:
        path = path.replace(r"\\", "\\")

        if (
            os.path.isdir(path)
            and path not in libraries
        ):
            libraries.append(path)

    return libraries


def find_benchmark_install_dir() -> str:
    steam_path = find_steam_path()
    libraries = find_steam_libraries(steam_path)

    for library in libraries:
        steamapps_path = os.path.join(
            library,
            "steamapps",
        )

        manifest_path = os.path.join(
            steamapps_path,
            f"appmanifest_{APP_ID}.acf",
        )

        if not os.path.isfile(manifest_path):
            continue

        with open(
            manifest_path,
            "r",
            encoding="utf-8",
            errors="ignore",
        ) as file:
            manifest = file.read()

        match = re.search(
            r'"installdir"\s+"([^"]+)"',
            manifest,
        )

        if not match:
            continue

        install_dir_name = match.group(1)

        install_dir = os.path.join(
            steamapps_path,
            "common",
            install_dir_name,
        )

        if os.path.isdir(install_dir):
            return install_dir

    raise FileNotFoundError(
        "Black Myth: Wukong Benchmark Tool не найден."
    )


def find_config_path() -> str:
    install_dir = find_benchmark_install_dir()

    config_path = os.path.join(
        install_dir,
        "b1",
        "Saved",
        "Config",
        "Windows",
        "GameUserSettings.ini",
    )

    if not os.path.isfile(config_path):
        raise FileNotFoundError(
            f"GameUserSettings.ini не найден: {config_path}"
        )

    return config_path