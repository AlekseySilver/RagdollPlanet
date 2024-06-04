shader_type spatial;


float random(in vec3 st) {
    return fract(sin(dot(st, vec3(12.9898, 78.233, 125.355))) * 43758.5453123);
}
vec2 random2(in vec2 p) {
    return fract(sin(vec2(dot(p,vec2(127.1,311.7)),dot(p,vec2(269.5,183.3))))*43758.5453);
}
vec2 point_in_cell(in vec2 cell, float time) {
    vec2 v = random2(cell);
    v = sin(v + time * (v + .5)) * 0.4 + .55;
    v += cell;
    return v;
}
float distance2(in vec2 a, in vec2 b) {
    vec2 v = a - b;
    return v.x * v.x + v.y * v.y;
}






void fragment() {
	NORMAL = normalize(VERTEX);
	
	vec2 st = UV;

    
    // Scale
    float scale = 50.296;
    st *= scale;

    // Tile the space
    vec2 i_st = floor(st);
    vec2 f_st = fract(st);

    
    float min_dist = scale * 2.;
    vec2 min_cell;
    float min_delta_dist;
    
    for (int j = -1; j <= 1; ++j) {
    	for (int i = -1; i <= 1; ++i) {
            vec2 cell = i_st + vec2(float(i), float(j));
        	vec2 point = point_in_cell(cell, TIME * .15);
            float dist = distance2(point, st);
            
            if (dist < min_dist) {
                min_delta_dist = min_dist - dist;
                min_cell = cell;
                min_dist = dist;
            }
    	}    
    }
    
    
    vec2 qw = fract(min_cell / 2.);
    vec3 color = mix(vec3(0.179,0.221,0.585), vec3(0.195,0.256,0.740), step(.1, qw.x + qw.y));

    color = mix(vec3(0.444,0.515,1.000), color, step(0.276, min_delta_dist / min_dist));



	//float dot_light_normal = dot(cLightDirPS.xyz, normal);
    //vec3 eye = normalize(cCameraPosPS - vWorldPos);
    //vec3 c = cross(eye, cLightDirPS.xyz);
    //float spec = max(dot_light_normal, MIN_LIGHT_RATE);
    //spec += pow((1. - abs(dot(c, normal))), 4.) * smoothstep(-0.1, 0.1, dot_light_normal);
    //color *= spec;


    ALBEDO = color;
	
	
	vec2 base_uv = UV;

}
