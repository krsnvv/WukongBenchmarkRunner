import re
import shutil
from pathlib import Path

from models import BenchmarkProfile


def _replace_line(text: str, key: str, value: str) -> str:
    pattern = rf"(?m)^{re.escape(key)}=.*$"

    if not re.search(pattern, text):
        raise ValueError(f"Параметр '{key}' не найден в конфиге.")

    return re.sub(
        pattern,
        f"{key}={value}",
        text,
    )


def _replace_ui_value(text: str, key: str, value: str) -> str:
    pattern = rf'\("{re.escape(key)}",\s*"[^"]*"\)'

    if not re.search(pattern, text):
        raise ValueError(
            f"UI-параметр '{key}' не найден в конфиге."
        )

    return re.sub(
        pattern,
        f'("{key}", "{value}")',
        text,
    )


def _to_scalability(ui_value: int) -> int:
    return ui_value - 1


def apply_profile(
    config_path: str,
    profile: BenchmarkProfile,
) -> None:
    path = Path(config_path)

    if not path.is_file():
        raise FileNotFoundError(
            f"GameUserSettings.ini не найден: {config_path}"
        )

    backup_path = path.with_suffix(
        path.suffix + ".bak"
    )

    if not backup_path.exists():
        shutil.copy2(path, backup_path)

    text = path.read_text(
        encoding="utf-8",
        errors="ignore",
    )

    desired_width = int(
        profile.resolution_width
        * profile.resolution_quality
        / 100
    )

    desired_height = int(
        profile.resolution_height
        * profile.resolution_quality
        / 100
    )

    # Разрешение экрана
    text = _replace_line(
        text,
        "ResolutionSizeX",
        str(profile.resolution_width),
    )

    text = _replace_line(
        text,
        "ResolutionSizeY",
        str(profile.resolution_height),
    )

    text = _replace_line(
        text,
        "LastUserConfirmedResolutionSizeX",
        str(profile.resolution_width),
    )

    text = _replace_line(
        text,
        "LastUserConfirmedResolutionSizeY",
        str(profile.resolution_height),
    )

    # Внутреннее render resolution
    text = _replace_line(
        text,
        "DesiredScreenWidth",
        str(desired_width),
    )

    text = _replace_line(
        text,
        "DesiredScreenHeight",
        str(desired_height),
    )

    text = _replace_line(
        text,
        "LastUserConfirmedDesiredScreenWidth",
        str(desired_width),
    )

    text = _replace_line(
        text,
        "LastUserConfirmedDesiredScreenHeight",
        str(desired_height),
    )

    text = _replace_line(
        text,
        "sg.ResolutionQuality",
        str(profile.resolution_quality),
    )

    # Общие benchmark-настройки
    text = _replace_line(
        text,
        "bUseVSync",
        "False",
    )

    text = _replace_line(
        text,
        "bUseDynamicResolution",
        "False",
    )

    text = _replace_line(
        text,
        "FrameRateLimit",
        "0.000000",
    )

    # UI
    ui_values = {
        "QualityLevel": profile.quality_level,
        "ViewDistance": profile.view_distance,
        "AntiAliasing": profile.anti_aliasing,
        "PostProcessing": profile.post_processing,
        "ShadowQuality": profile.shadow_quality,
        "TextureQuality": profile.texture_quality,
        "FxQuality": profile.fx_quality,
        "MaterialQuality": profile.material_quality,
        "VegetationQuality": profile.vegetation_quality,
        "GlobalIllumination": profile.global_illumination,
        "ReflectionQuality": profile.reflection_quality,
        "InsertFrame": 1 if profile.frame_generation else 0,
        "Rtx": 1 if profile.ray_tracing else 0,
        "RtxLevel": profile.ray_tracing_level,
        "Vsync": 0,
        "LockFrameRate": 0,
        "ImageQuality": desired_height,
    }

    for key, value in ui_values.items():
        text = _replace_ui_value(
            text,
            key,
            str(value),
        )

    # Ray Tracing
    text = _replace_line(
        text,
        "r.RayTracing.EnableInGame",
        "True" if profile.ray_tracing else "False",
    )

    # Scalability
    scalability_values = {
        "sg.ViewDistanceQuality":
            _to_scalability(profile.view_distance),

        "sg.AntiAliasingQuality":
            _to_scalability(profile.anti_aliasing),

        "sg.ShadowQuality":
            _to_scalability(profile.shadow_quality),

        "sg.GlobalIlluminationQuality":
            _to_scalability(profile.global_illumination),

        "sg.ReflectionQuality":
            _to_scalability(profile.reflection_quality),

        "sg.PostProcessQuality":
            _to_scalability(profile.post_processing),

        "sg.TextureQuality":
            _to_scalability(profile.texture_quality),

        "sg.EffectsQuality":
            _to_scalability(profile.fx_quality),

        "sg.FoliageQuality":
            _to_scalability(profile.vegetation_quality),

        "sg.ShadingQuality":
            _to_scalability(profile.material_quality),
    }

    for key, value in scalability_values.items():
        text = _replace_line(
            text,
            key,
            str(value),
        )

    if (
        not profile.ray_tracing
        and profile.quality_level == 1
    ):
        text = _replace_line(
            text,
            "sg.RayTracingQuality",
            "0",
        )

    text = _replace_line(
        text,
        "GSStreamingPoolSize",
        str(profile.streaming_pool_size),
    )

    path.write_text(
        text,
        encoding="utf-8",
    )