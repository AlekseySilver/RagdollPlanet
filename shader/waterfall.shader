shader_type spatial;
render_mode blend_mix,depth_draw_opaque,diffuse_burley,specular_phong,cull_disabled;
uniform vec4 albedo : hint_color;
uniform vec4 albedo2 : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform sampler2D texture_normal : hint_normal;
uniform float normal_scale : hint_range(-16,16);
uniform vec3 uv1_scale;

uniform float fall_speed = 1.0;

uniform vec4 light_color : hint_color;
uniform vec4 dark_color : hint_color;
uniform sampler2D texture_noise;
uniform float displ_amount = 0.02;
uniform float speed = 1.0;


varying vec2 uv;

//void light() {
//	DIFFUSE_LIGHT = ALBEDO * max(0.0, dot(LIGHT, NORMAL));
//}

void vertex() {
	vec3 vx = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
	vec3 tn = normalize((WORLD_MATRIX * vec4(TANGENT, 0.0)).xyz);
	vec3 bn = normalize((WORLD_MATRIX * vec4(BINORMAL, 0.0)).xyz);
	
	uv = vec2(dot(vx, tn), dot(vx, bn)) * uv1_scale.xy;
	uv.x += TIME * fall_speed;
	
	float height = texture(texture_noise, uv * uv1_scale.z).x - 0.5;
	VERTEX.y += height;
}

void fragment() {
	vec2 displ = texture(texture_albedo, uv).xy;
	displ = (displ * 2.0 - 1.0) * displ_amount;
	
	float noise = texture(texture_noise, vec2(UV.x / 4.0 + TIME * speed, UV.y) - displ).x;
	noise = floor(noise * 4.0) / 4.0;
	
	vec4 col = mix(dark_color, light_color, noise);
	
	vec2 uv_movement = vec2(uv.x, UV.y);

	ALBEDO = col.rgb;
	NORMALMAP = texture(texture_normal, uv_movement).rgb;
	ROUGHNESS = 0.11;
	ALPHA = texture(texture_albedo, uv_movement).a * 0.75;
}
