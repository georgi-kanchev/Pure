namespace Pure.Engine.Window;

internal class EffectLayer : Effect
{
    public const int MAX_REPLACE = 64, MAX_WAVE = 48, MAX_BLUR = 48, MAX_EDGE = 32;

    protected override string Fragment
    {
        get =>
            $@"
uniform sampler2D texture;

uniform float gamma;
uniform float saturation;
uniform float contrast;
uniform float brightness;
uniform float time;

uniform int replaceCount;
uniform vec4 replaceOld[{MAX_REPLACE}];
uniform vec4 replaceNew[{MAX_REPLACE}];

uniform int waveCount;
uniform vec4 waveTarget[{MAX_WAVE}];
uniform vec4 waveArea[{MAX_WAVE}];
uniform vec4 waveSpeedFreq[{MAX_WAVE}];

uniform int blurCount;
uniform vec4 blurTarget[{MAX_BLUR}];
uniform vec4 blurArea[{MAX_BLUR}];
uniform vec2 blurStrength[{MAX_BLUR}];

uniform int edgeCount;
uniform vec4 edgeTarget[{MAX_EDGE}];
uniform vec4 edgeColor[{MAX_EDGE}];
uniform vec4 edgeArea[{MAX_EDGE}];
uniform int edgeType[{MAX_EDGE}];

uniform vec4 tint;

uniform vec2 viewSize;

bool is_inside(vec2 coord, vec4 area, vec4 off)
{{
	vec2 pixel = 1.0 / viewSize;
	return
		coord.x > area.x + off.x && coord.x < area.x + area.z + off.y &&
		coord.y > area.y - area.w + off.z && coord.y < area.y + off.w;
}}
bool is(vec3 a, vec3 b)
{{
	return distance(a, b) < 0.01;
}}

bool isType(int value, int flag)
{{
    return (value & flag) != 0;
}}

void main(void)
{{
	vec2 pixel = 1.0 / viewSize;
	vec2 coord = gl_TexCoord[0].xy;

	// waves
	for(int i = 0; i < waveCount; i++)
	{{
		vec4 spFr = waveSpeedFreq[i];
		vec4 a = waveArea[i];
		vec4 off = vec4(-pixel.x, -pixel.x, pixel.y, pixel.y);		
		vec4 t = waveTarget[i];		
		bool is_target_color = t == vec4(0, 0, 0, 0) ||
								(t != vec4(0, 0, 0, 0) && is(texture2D(texture, coord) * gl_Color, t));		

		if (is_inside(coord, a, off) && is_target_color)
		{{
			coord.x += cos(coord.y * spFr.z + spFr.x * time) / viewSize.x / 1.5;
			coord.y += sin(coord.x * spFr.w + spFr.y * time) / viewSize.y / 1.5;
		}}
	}}

    vec4 color = texture2D(texture, coord) * gl_Color;
	
	// blur
	for(int i = 0; i < blurCount; i++)
	{{
		vec4 a = blurArea[i];
		vec4 off = vec4(-pixel.x, -pixel.x, pixel.y, pixel.y);		
		vec4 t = blurTarget[i];	
		vec2 s = blurStrength[i];	
		bool is_target_color = t == vec4(0, 0, 0, 0) ||
								(t != vec4(0, 0, 0, 0) && is(texture2D(texture, coord) * gl_Color, t));		

		if (is_inside(coord, a, off) && is_target_color)
		{{
			vec2 blur = pixel * s;
			vec4 blurResult =
				texture2D(texture, coord) * 4.0 +
				texture2D(texture, coord - blur.x) * 2.0 +
				texture2D(texture, coord + blur.x) * 2.0 +
				texture2D(texture, coord - blur.y) * 2.0 +
				texture2D(texture, coord + blur.y) * 2.0 +
				texture2D(texture, coord - blur.x - blur.y) * 1.0 +
				texture2D(texture, coord - blur.x + blur.y) * 1.0 +
				texture2D(texture, coord + blur.x - blur.y) * 1.0 +
				texture2D(texture, coord + blur.x + blur.y) * 1.0;
			color = (blurResult / 16.0);
		}}
	}}

	// replace colors
    for(int i = 0; i < replaceCount; i++)
	    if (distance(color, replaceOld[i]) < 0.01)
		    color = replaceNew[i];

	// edges
	vec3 u = (texture2D(texture, vec2(coord.x, coord.y - pixel.y)) * gl_Color).rgb;
	vec3 d = (texture2D(texture, vec2(coord.x, coord.y + pixel.y)) * gl_Color).rgb;
	vec3 l = (texture2D(texture, vec2(coord.x + pixel.x, coord.y)) * gl_Color).rgb;
	vec3 r = (texture2D(texture, vec2(coord.x - pixel.x, coord.y)) * gl_Color).rgb;

	vec3 ul = (texture2D(texture, vec2(coord.x + pixel.x, coord.y - pixel.y)) * gl_Color).rgb;
	vec3 dl = (texture2D(texture, vec2(coord.x + pixel.x, coord.y + pixel.y)) * gl_Color).rgb;
	vec3 ur = (texture2D(texture, vec2(coord.x - pixel.x, coord.y - pixel.y)) * gl_Color).rgb;
	vec3 dr = (texture2D(texture, vec2(coord.x - pixel.x, coord.y + pixel.y)) * gl_Color).rgb;
	
	for(int i = 0; i < edgeCount; i++)
	{{
		vec4 t = edgeTarget[i];
		vec4 c = edgeColor[i];
		vec4 a = edgeArea[i];
		int type = edgeType[i];		

		bool is_u = is(u, t);
		bool is_d = is(d, t);
		bool is_l = is(l, t);
		bool is_r = is(r, t);

		bool is_ul = !is_u && !is_l && is(ul, t);		
		bool is_ur = !is_u && !is_r && is(ur, t);		
		bool is_dl = !is_d && !is_l && is(dl, t);		
		bool is_dr = !is_d && !is_r && is(dr, t);		
		
		vec4 off = vec4(-pixel.x * 1.5, pixel.x, 0, pixel.y * 1.5);

		if (!is(color, t) && is_inside(coord, a, off))
		{{
			// types: Top = 1, Bottom = 2, Left = 4, Right = 8, Corners = 16
			bool corners = isType(type, 16) && (is_ul || is_dl || is_ur || is_dr);
			bool top = isType(type, 1) && is_u;
			bool bot = isType(type, 2) && is_d;
			bool left = isType(type, 4) && is_l;
			bool right = isType(type, 8) && is_r;

			if (corners || top || bot || left || right)
				color = c;
		}}
	}}

	// color adjustments
    color.rgb = pow(color.rgb, vec3(1.0 / gamma));
    float luminance = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
    color.rgb = mix(vec3(luminance), color.rgb, saturation);
    color.rgb = mix(vec3(0.5), color.rgb, contrast);
    color.rgb *= brightness;

    color *= tint;

	gl_FragColor = color;
}}";
    }
}