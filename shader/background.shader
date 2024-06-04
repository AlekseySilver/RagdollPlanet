shader_type canvas_item;
render_mode unshaded;

uniform mat4 camera_xform;
uniform mat4 par1;

uniform vec4 day_color : hint_color;
uniform vec4 night_color : hint_color;
uniform vec4 sun_color : hint_color;

vec3 smoothmix(vec3 a, vec3 b, float low, float high, float t) {
    return mix(a, b, smoothstep(low, high, t));
}

void fragment() {
	
	//vec3 cam_dir = -camera_xform[2].xyz;
	//vec3 cam_up = camera_xform[1].xyz;
	vec3 cam_pos = camera_xform[3].xyz;
	
	vec3 sun_dir = par1[0].xyz;
	
	vec2 nr = par1[1].xy; // camera frustum near in local space
	// nr.y = -nr.y;
	vec3 cam_dir = normalize(vec3(mix(-nr, nr, UV), 1.0));
	cam_dir = normalize(vec3((UV - 0.5) * 10.5, 1.0));
	//cam_dir = vec3(0.0, 0.0, 1.0);
	//cam_dir = vec3(
	//	dot(cam_dir, camera_xform[0].xyz),
	//	dot(cam_dir, camera_xform[1].xyz),
	//	dot(cam_dir, camera_xform[2].xyz)
	//);
	cam_dir = mat3(camera_xform) * cam_dir;
	
	
	
	float sun = dot(cam_dir, sun_dir) * -0.5 + 0.5;
	float day = dot(normalize(cam_pos), sun_dir) * -0.5 + 0.5;
	
	//vec3 col = mix(night_color.rgb, day_color.rgb, sun * 0.2 + day * 0.8);
	vec3 col = mix(night_color.rgb, day_color.rgb, sun);
	
	
	///// sun
    //col = smoothmix(col, sun_color.rgb, .90, .997, sun);
	col = mix(col, sun_color.rgb, step(.90, sun));
	
	col = abs(cam_dir);
	//col = cam_dir * 0.5 + 0.5;
	//col = camera_xform[2].xyz;
	//col = cam_dir;
	
	
	//col = floor(col * 10.0) / 10.0;
	
	COLOR = vec4(col, 1.0);
}