shader_type canvas_item;

void fragment() {
    vec4 c = texture(TEXTURE, UV);
    COLOR = c;
}