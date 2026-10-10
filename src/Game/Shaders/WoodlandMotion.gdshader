shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx, cull_disabled;

// CPU writes just three shared presentation uniforms; TIME and gameplay RNG are
// deliberately not used, so owned fixed-clock screenshots are reproducible.
global uniform float odot_weather_time;
global uniform float odot_foliage_wind;
global uniform float odot_water_motion;
uniform bool wind_surface = false;
uniform bool water_surface = false;
uniform vec4 albedo_color : source_color = vec4(1.0);
uniform sampler2D albedo_texture : source_color, hint_default_white, filter_linear_mipmap, repeat_enable;
uniform sampler2D roughness_texture : hint_default_white, filter_linear_mipmap, repeat_enable;
uniform sampler2D normal_texture : hint_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D metallic_texture : hint_default_white, filter_linear_mipmap, repeat_enable;
uniform vec4 metallic_channel = vec4(0.0, 0.0, 1.0, 0.0);
uniform vec4 roughness_channel = vec4(1.0, 0.0, 0.0, 0.0);
uniform float roughness = 1.0;
uniform float metallic = 0.0;
uniform float specular = 0.5;
uniform bool normal_enabled = false;
uniform float normal_scale = 1.0;
uniform vec3 uv_scale = vec3(1.0);
uniform vec3 uv_offset = vec3(0.0);
varying vec3 woodland_world;

void vertex() {
    UV = UV * uv_scale.xy + uv_offset.xy;
    woodland_world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    if (wind_surface) {
        float phase = odot_weather_time * 1.15 + woodland_world.x * 0.7 + woodland_world.z * 0.4;
        // At most five centimetres in WORLD space regardless of imported scale.
        // Bark, trunks, roots, structures, units, equipment and terrain never bend.
        float bend = sin(phase) * 0.05 * odot_foliage_wind * clamp(abs(VERTEX.y), 0.15, 1.0);
        VERTEX += (inverse(MODEL_MATRIX) * vec4(bend, 0.0, bend * 0.4, 0.0)).xyz;
    }
}
void fragment() {
    ALBEDO = albedo_color.rgb * texture(albedo_texture, UV).rgb;
    METALLIC = metallic * dot(texture(metallic_texture, UV), metallic_channel);
    SPECULAR = specular;
    ROUGHNESS = roughness * dot(texture(roughness_texture, UV), roughness_channel);
    if (normal_enabled) {
        NORMAL_MAP = texture(normal_texture, UV).rgb;
        NORMAL_MAP_DEPTH = normal_scale;
    }
    if (water_surface) {
        // The authored surface includes painted bank details. Only cool-colored
        // water pixels receive a tiny glint/normal variation; no UV scroll, albedo
        // repaint, height displacement or moving shoreline is introduced.
        float water = smoothstep(0.01, 0.12, ALBEDO.b - ALBEDO.r) * odot_water_motion;
        float ripple = sin(odot_weather_time * 0.8 + woodland_world.x * 2.0 + woodland_world.z);
        ROUGHNESS = clamp(ROUGHNESS + ripple * water * 0.025, 0.05, 1.0);
        if (normal_enabled) NORMAL_MAP.xy += vec2(ripple, -ripple) * water * 0.006;
    }
}
