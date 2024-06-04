shader_type spatial;
render_mode async_visible,blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx,unshaded;
uniform vec4 albedo : hint_color;
uniform float speed;
uniform float scale;
uniform float part : hint_range(0,1);

varying float y;

void vertex() {
	vec3 vx = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
	vec3 bn = normalize((WORLD_MATRIX * vec4(BINORMAL, 0.0)).xyz);
	
	y = dot(vx, bn) * scale + TIME * speed;
}

void fragment() {
	if (fract(y) < part)
		discard;
	ALBEDO = albedo.rgb;
	ALPHA = albedo.a;
}
