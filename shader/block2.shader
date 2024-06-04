shader_type spatial;
render_mode blend_mix,depth_draw_opaque,diffuse_burley,specular_schlick_ggx;
uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform sampler2D texture_normal : hint_normal;
uniform float normal_scale : hint_range(-16,16);
uniform vec3 uv1_scale;

varying vec2 uv;

//void light() {
//	DIFFUSE_LIGHT = ALBEDO * max(0.0, dot(LIGHT, NORMAL));
//}

void vertex() {
	vec3 n = abs(NORMAL);
	n = vec3(step(n.x, n.y), step(n.y, n.z), step(n.z, n.x));
	vec4 gl = vec4((1.0 - n) * n.zxy, 0.0); // greater axis in local space
	vec3 tn = gl.ywx + gl.zww; // tangent in local space
	vec3 bn = gl.wxy + gl.wzw; // binormal in local space

	mat3 basis = mat3(WORLD_MATRIX);
	tn = normalize(basis * tn);
	bn = normalize(basis * bn);
	n = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
	uv = vec2(dot(n, tn), dot(n, bn)) * uv1_scale.xy;
}

void fragment() {
	vec2 b = abs(UV - 0.5);
	b.x = max(b.x, b.y);

	vec4 albedo_tex = texture(texture_albedo, uv);
	ALBEDO = mix(albedo.rgb * albedo_tex.rgb, vec3(0.0), smoothstep(0.49, 0.5, b.x));
	
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	NORMALMAP = texture(texture_normal, uv * uv1_scale.z).rgb;
	NORMALMAP_DEPTH = normal_scale;
}
