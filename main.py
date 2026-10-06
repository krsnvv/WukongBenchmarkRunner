from benchmark_locator import find_config_path
from benchmark_runner import run_benchmark_tool
from models import BenchmarkProfile
from settings_manager import apply_profile


cpu_profile = BenchmarkProfile(
    name="CPU",

    resolution_width=1280,
    resolution_height=720,
    resolution_quality=50,

    quality_level=1,

    view_distance=1,
    anti_aliasing=1,
    post_processing=1,
    shadow_quality=1,
    texture_quality=1,
    fx_quality=1,
    material_quality=1,
    vegetation_quality=1,
    global_illumination=1,
    reflection_quality=1,

    ray_tracing=False,
    ray_tracing_level=1,

    frame_generation=False,

    streaming_pool_size=384,
)


config_path = find_config_path()

print("Config:")
print(config_path)
print()

apply_profile(
    config_path,
    cpu_profile,
)

print("CPU profile applied.")
print()

run_benchmark_tool()

print()
print("Done.")