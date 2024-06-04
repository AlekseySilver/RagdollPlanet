shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_disabled;

uniform sampler2D texture_albedo : hint_albedo;

void fragment() {
	ALBEDO = texture(texture_albedo,UV).rgb;
}
