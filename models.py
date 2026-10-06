from dataclasses import dataclass


@dataclass
class BenchmarkProfile:
    name: str

    resolution_width: int
    resolution_height: int
    resolution_quality: float

    quality_level: int

    view_distance: int
    anti_aliasing: int
    post_processing: int
    shadow_quality: int
    texture_quality: int
    fx_quality: int
    material_quality: int
    vegetation_quality: int
    global_illumination: int
    reflection_quality: int

    ray_tracing: bool
    ray_tracing_level: int

    frame_generation: bool

    streaming_pool_size: int