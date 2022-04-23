shader_type canvas_item;

uniform sampler2D base_texture;
uniform sampler2D remap_texture;
uniform sampler2D foreground_palette_texture;
uniform sampler2D background_palette_texture;
uniform sampler2D mode_palette_texture; // (is_selecting, unselected_mode, ..., ...)
uniform sampler2D terrain_texture;

uniform float width = 0.001;

// linear transformation y = b + k * x
uniform float remap_k = 0.99609375; // 255.0 / 256.0
uniform vec2 remap_b = vec2(0.001953125, 0.001953125); // 1/256/2
uniform vec4 selecting_color : hint_color = vec4(0.5, 0.0, 1.0, 1.0);
// Also see texelFetch:
// https://stackoverflow.com/questions/45613310/switching-from-texture-to-texelfetch

uniform int mode; // 0 => region, 1 => terrain

//https://gist.github.com/sugi-cho/6a01cae436acddd72bdf
//http://gamedev.stackexchange.com/questions/59797/glsl-shader-change-hue-saturation-brightness
vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

//https://github.com/Jam3/glsl-hsl2rgb/blob/master/index.glsl
vec3 hsl2rgb( in vec3 c )
{
    vec3 rgb = clamp( abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0 );

    return c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
}

// https://www.shadertoy.com/view/XljGzV
vec3 rgb2hsl( in vec3 c ){
  	float h = 0.0;
	float s = 0.0;
	float l = 0.0;
	float r = c.r;
	float g = c.g;
	float b = c.b;
	float cMin = min( r, min( g, b ) );
	float cMax = max( r, max( g, b ) );

	l = ( cMax + cMin ) / 2.0;
	if ( cMax > cMin ) {
		float cDelta = cMax - cMin;
        
        //s = l < .05 ? cDelta / ( cMax + cMin ) : cDelta / ( 2.0 - ( cMax + cMin ) ); Original
		s = l < .0 ? cDelta / ( cMax + cMin ) : cDelta / ( 2.0 - ( cMax + cMin ) );
        
		if ( r == cMax ) {
			h = ( g - b ) / cDelta;
		} else if ( g == cMax ) {
			h = 2.0 + ( b - r ) / cDelta;
		} else {
			h = 4.0 + ( r - g ) / cDelta;
		}

		if ( h < 0.0) {
			h += 6.0;
		}
		h = h / 6.0;
	}
	return vec3( h, s, l );
}


vec4 band_color(vec2 uv, vec4 foreground_palette_color, vec4 background_palette_color){
	float f = dot(uv, vec2(0.7071067, 0.7071067)); // sqrt(2)/2
	if(mod(f, 0.01) < 0.005){
		return foreground_palette_color;
	}else{
		return background_palette_color;
	}
}

void fragment(){
	vec4 c = texture(base_texture, UV);
	vec4 c1 = texture(base_texture, UV + vec2(width, 0.0));
	vec4 c2 = texture(base_texture, UV + vec2(0.0, width));
	vec4 c3 = texture(base_texture, UV + vec2(-width, 0.0));
	vec4 c4 = texture(base_texture, UV + vec2(0.0, -width));
	if (c != c1 || c != c2 || c != c3 || c != c4){
		COLOR = vec4(0,0,0,1);
	}else{
		vec4 idx = texture(remap_texture, UV);
		vec2 idx2 = idx.rg * remap_k + remap_b;
		vec4 foreground_palette_color = texture(foreground_palette_texture, idx2);
		
		switch(mode){
			case 0:
				vec4 mode_p = texture(mode_palette_texture, idx2);
				
				if(mode_p.g == 0.0){
					COLOR = foreground_palette_color;
				}else{
					vec4 background_palette_color = texture(background_palette_texture, idx2);
					COLOR = band_color(UV, foreground_palette_color, background_palette_color);
				}
				
				if(mode_p.r == 1.0){
					vec3 hsl_COLOR = rgb2hsl(COLOR.rgb);
					COLOR.rgb = hsl2rgb(hsl_COLOR + vec3(0.00, 0.05, 0.3));
					COLOR = clamp(COLOR, 0.0, 1.0);
					COLOR = mix(COLOR, selecting_color, 0.05);
				}else if(mode_p.b >= 0.5){
					vec3 hsl_COLOR = rgb2hsl(COLOR.rgb);
					COLOR.rgb = hsl2rgb(hsl_COLOR + vec3(0.00, 0.05, 0.1));
					COLOR = mix(COLOR, vec4(0.0, 1.0, 0.0, 1.0), 0.05);
				}
				
				break;
				
			case 2: // region mode 2 (forced selecting color for debugging)
				vec4 mode_p = texture(mode_palette_texture, idx2);
				if(mode_p.r != 0.0){
					COLOR = selecting_color;
				}else if(mode_p.g == 0.0){
					// Show foreground color in the area
					COLOR = foreground_palette_color;
				}else{ // TODO: use systemantic int mapping
					// Show banded foreground & background color in the area
					vec4 background_palette_color = texture(background_palette_texture, idx2);
					COLOR = band_color(UV, foreground_palette_color, background_palette_color);
				}
				break;
			case 1: // terrain mode
				// Ignore foreground and background color
				vec4 terrain_color = texture(terrain_texture, UV);
				COLOR = pow(terrain_color, vec4(selecting_color.r));
				break;
		}
	}
}