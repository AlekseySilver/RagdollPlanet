shader_type canvas_item;

uniform float value : hint_range(0, 1);
uniform vec4 fore_color : hint_color;
uniform vec4 back_color : hint_color;

void fragment() {
	vec2 uv = (UV - 0.5) * 2.0;
	
	float a = (atan(uv.x, uv.y) + 3.1415926535897932384626f) / (2.0f * 3.1415926535897932384626f);
	
	vec4 color = mix(back_color, fore_color, step(a, value));
	color.a *= step(dot(uv, uv), 0.9);
	
	COLOR = color;
}