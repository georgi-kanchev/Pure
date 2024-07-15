namespace Pure.Engine.Window;

internal class EffectLayer : Effect
{
    public const int MAX = 32;

    protected override string Fragment
    {
        get =>
            $@"
uniform sampler2D texture;
uniform sampler2D data;

uniform float gamma;
uniform float saturation;
uniform float contrast;
uniform float brightness;
uniform float time;

uniform int lightCount;
uniform vec3 light[{MAX}]; // x, y, radius
uniform vec4 lightColor[{MAX}];
uniform int obstacleCount;
uniform vec4 obstacleArea[{MAX}];
uniform bool lightMask;
uniform bool lightFade;
uniform bool lightInvert;

uniform int replaceCount;
uniform vec4 replaceOld[{MAX}];
uniform vec4 replaceNew[{MAX}];

uniform int waveCount;
uniform vec4 waveTarget[{MAX}];
uniform vec4 waveArea[{MAX}];
uniform vec4 waveSpeedFreq[{MAX}];

uniform int blurCount;
uniform vec4 blurTarget[{MAX}];
uniform vec4 blurArea[{MAX}];
uniform vec2 blurStrength[{MAX}];

uniform int edgeCount;
uniform vec4 edgeTarget[{MAX}];
uniform vec4 edgeColor[{MAX}];
uniform vec4 edgeArea[{MAX}];
uniform int edgeType[{MAX}];

uniform vec4 tint;

uniform vec2 viewSize;
uniform vec2 tilemapSize;
uniform vec2 tileSize;

bool is_inside(vec2 coord, vec4 area, vec4 off)
{{
	return
		coord.x > area.x + off.x && coord.x < area.x + area.z + off.y &&
		coord.y > area.y - area.w + off.z && coord.y < area.y + off.w;
}}
bool is(vec3 a, vec3 b)
{{
	return distance(a, b) < 0.01;
}}

bool is_type(int value, int flag)
{{
    return (value & flag) != 0;
}}

int compute_out_code(vec2 p, vec2 rectMin, vec2 rectMax)
{{
	// left: 1, right: 2, bottom: 4, top: 8
    int code = 0;

    if (p.x < rectMin.x)
        code |= 1;
    else if (p.x > rectMax.x)
        code |= 2;

    if (p.y < rectMin.y)
        code |= 4;
    else if (p.y > rectMax.y)
        code |= 8;

    return code;
}}

bool is_shadow(vec2 p0, vec2 p1, vec2 rectMin, vec2 rectMax)
{{
	float flipY = rectMax.y - rectMin.y;
	rectMax.y -= flipY;
	rectMin.y -= flipY;

    int outcode0 = compute_out_code(p0, rectMin, rectMax);
    int outcode1 = compute_out_code(p1, rectMin, rectMax);

    bool accept = false;

    while (true)
	{{
        if ((outcode0 | outcode1) == 0)
		{{
            // Both points inside the rectangle, trivially accept
            accept = true;
            break;
        }}
		else if ((outcode0 & outcode1) != 0)
            break; // Both points share an outside zone, trivially reject
        else
		{{
            // Calculate the line segment to clip from an outside point to an intersection with the rectangle
            float x, y;
            int outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;

            if ((outcodeOut & 8) != 0)
			{{ // point is above the rectangle
                x = p0.x + (p1.x - p0.x) * (rectMax.y - p0.y) / (p1.y - p0.y);
                y = rectMax.y;
            }}
			else if ((outcodeOut & 4) != 0)
			{{ // point is below the rectangle
                x = p0.x + (p1.x - p0.x) * (rectMin.y - p0.y) / (p1.y - p0.y);
                y = rectMin.y;
            }}
			else if ((outcodeOut & 2) != 0)
			{{ // point is to the right of the rectangle
                y = p0.y + (p1.y - p0.y) * (rectMax.x - p0.x) / (p1.x - p0.x);
                x = rectMax.x;
            }}
			else if ((outcodeOut & 1) != 0)
			{{ // point is to the left of the rectangle
                y = p0.y + (p1.y - p0.y) * (rectMin.x - p0.x) / (p1.x - p0.x);
                x = rectMin.x;
            }}

            if (outcodeOut == outcode0)
			{{
                p0 = vec2(x, y);
                outcode0 = compute_out_code(p0, rectMin, rectMax);
            }}
			else
			{{
                p1 = vec2(x, y);
                outcode1 = compute_out_code(p1, rectMin, rectMax);
            }}
        }}
    }}
    return accept;
}}

void main(void)
{{
	vec2 pixel = 1.0 / viewSize;
	vec2 coord = gl_TexCoord[0].xy;

	vec2 tileSz = pixel * tileSize;
	vec2 tileCoord = floor(coord / tileSz) * tileSz;
	vec4 data = texture2D(data, tileCoord);
	if (data.a > 0)
		discard;

	// waves
	for(int i = 0; i < waveCount; i++)
	{{
		vec4 spFr = waveSpeedFreq[i];
		vec4 a = waveArea[i];
		vec4 off = vec4(-pixel.x, -pixel.x, pixel.y, pixel.y);
		vec4 t = waveTarget[i];		
		bool is_target_color = t == vec4(0.0) ||
								(t != vec4(0.0) && is(texture2D(texture, coord) * gl_Color, t));		

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
		bool is_target_color = t == vec4(0.0) ||
								(t != vec4(0.0) && is(texture2D(texture, coord) * gl_Color, t));		

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
			bool corners = is_type(type, 16) && (is_ul || is_dl || is_ur || is_dr);
			bool top = is_type(type, 1) && is_u;
			bool bot = is_type(type, 2) && is_d;
			bool left = is_type(type, 4) && is_l;
			bool right = is_type(type, 8) && is_r;

			if (corners || top || bot || left || right)
				color = c;
		}}
	}}

	// lights
	float maskOpacity = 0.0;	
	for(int i = 0; i < lightCount; i++)
	{{
		vec3 l = light[i];
		vec4 c = lightColor[i];
		float aspect = viewSize.x / viewSize.y;
		float dist = distance(vec2(coord.x, coord.y / aspect), vec2(l.x, l.y / aspect));	

		if (dist < l.z)
		{{
			bool shadow = false;
			for(int j = 0; j < obstacleCount; j++)
			{{
				vec4 area = obstacleArea[j];
				vec2 min = vec2(area.x, area.y);
				vec2 max = vec2(area.x + area.z, area.y + area.w);
				vec4 off = vec4(0.0);
				
				if (is_shadow(vec2(l.x, l.y), coord, min, max) && !is_inside(coord, area, off))
				{{
					shadow = true;
					break;
				}}
			}}

			float attenuation = lightFade ? 1.0 - (dist / l.z) : 1.0;

			if (lightMask)
				maskOpacity += shadow ? 0.0 : attenuation;
			else
			{{
				color.rgb += c.rgb * c.a * attenuation;
				color.rgb -= shadow ? mix(vec3(0.0), color.rgb, c.a * attenuation) : vec3(0.0, 0.0, 0.0);
			}}
		}}
	}}

	color.a = lightMask ? maskOpacity : color.a;
	color.a = lightMask && lightInvert ? 1.0 - color.a : color.a;	

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