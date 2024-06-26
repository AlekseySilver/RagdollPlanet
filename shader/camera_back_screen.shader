shader_type spatial;
render_mode depth_draw_never, unshaded, shadows_disabled, ambient_light_disabled;
uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform float point_size : hint_range(0,128);
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;


void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}




void fragment() {
	//vec2 uv = UV;
	//vec4 albedo_tex = texture(texture_albedo,uv);
	//ALBEDO = vec3(uv.x, uv.y, 0.0); //albedo.rgb * albedo_tex.rgb;
	
	vec3 wvx = (CAMERA_MATRIX * vec4(VERTEX, 1.0)).xyz;
	
	ALBEDO = normalize(wvx) * .4 + .3;
}
