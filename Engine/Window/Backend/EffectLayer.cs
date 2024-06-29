namespace Pure.Engine.Window;

internal class EffectLayer : Effect
{
    public const int MAX_REPLACE = 64;
    public const int MAX_EDGE = 64;

    protected override string Fragment
    {
        get =>
            $@"
uniform sampler2D texture;

uniform float gamma;
uniform float saturation;
uniform float contrast;
uniform float brightness;

uniform int replaceCount;
uniform vec4 replaceOld[{MAX_REPLACE}];
uniform vec4 replaceNew[{MAX_REPLACE}];

uniform int edgeCount;
uniform vec4 edgeTarget[{MAX_EDGE}];
uniform vec4 edgeColor[{MAX_EDGE}];
uniform vec4 edgeArea[{MAX_EDGE}];
uniform int edgeType[{MAX_EDGE}];

uniform vec4 tint;
uniform vec4 overlay;

uniform vec2 viewSize;

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
	vec2 coord = gl_TexCoord[0].xy;
    vec4 color = texture2D(texture, coord) * gl_Color;

    for(int i = 0; i < replaceCount; i++)
	    if (distance(color, replaceOld[i]) < 0.01)
		    color = replaceNew[i];

	vec2 pixel = 1.0 / viewSize;
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
		
		bool is_inside = coord.x > a.x - pixel.x * 1.5 && coord.x < a.x + a.z &&
						coord.y > a.y - a.w && coord.y < a.y + pixel.y * 1.5;
		
		if (!is(color, t) && is_inside)
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

    color.rgb = pow(color.rgb, vec3(1.0 / gamma));
    float luminance = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
    color.rgb = mix(vec3(luminance), color.rgb, saturation);
    color.rgb = mix(vec3(0.5), color.rgb, contrast);
    color.rgb *= brightness;

    color *= tint;
    color.rgb = mix(color.rgb, overlay.rgb, overlay.a);

	gl_FragColor = color;
}}";
    }
}