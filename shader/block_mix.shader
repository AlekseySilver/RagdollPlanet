shader_type spatial;
render_mode async_visible,blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform vec3 uv1_scale;

varying vec3 wpos;
varying vec3 wnor;

void vertex() {
	wpos = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
	wnor = abs(normalize((WORLD_MATRIX * vec4(NORMAL, 0.0)).xyz));
	wnor *= 1.0 / (wnor.x + wnor.y + wnor.z);
}

void fragment() {
	vec3 p = wpos * uv1_scale.x;
	vec3 albedo_tex1 = texture(texture_albedo, p.xy).rgb;
	vec3 albedo_tex2 = texture(texture_albedo, p.yz).rgb;
	vec3 albedo_tex3 = texture(texture_albedo, p.zx).rgb;
	p = albedo_tex1 * wnor.z + albedo_tex2 * wnor.x + albedo_tex3 * wnor.y;
	ALBEDO = albedo.rgb * p + COLOR.rgb * uv1_scale.y;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	//ALPHA = albedo.a;
}
