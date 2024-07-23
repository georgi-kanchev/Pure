namespace Pure.Engine.Window;

internal class EffectLayer : Effect
{
    protected override string Fragment
    {
        get =>
            // general uniforms
            @"
uniform sampler2D texture;
uniform sampler2D data;
uniform float time;
uniform vec2 tileSize;
uniform vec2 tileCount;" +
            // light uniforms
            @"
uniform int lightCount;
uniform vec3 light[20]; // x, y, radius, type
uniform vec4 lightColor[20];
uniform int obstacleCount;
uniform vec4 obstacleArea[200];
uniform int lightFlags;" +

            // utility functions
            @"
bool is_inside(vec2 coord, vec4 area) {
	return coord.x > area.x && coord.x < area.x + area.z &&
		   coord.y > area.y - area.w && coord.y < area.y;
}
bool is(vec4 a, vec4 b) {
	return distance(a, b) < 0.005;
}
bool is(vec3 a, vec3 b) {
	return distance(a, b) < 0.005;
}
bool is(vec2 a, vec2 b) {
	return distance(a, b) < 0.005;
}
bool is(float a, float b) {
	return abs(a - b) < 0.005;
}
bool has_bit(int value, int flag)
{
	return (value & flag) != 0;
}
float map(float value, float min1, float max1, float min2, float max2) {
  return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}" +
            // data functions
            @"
vec2 get_tile_coord(){
	vec2 coord = gl_TexCoord[0].xy;
	vec2 texel = 1.0 / tileCount / tileSize;
	vec2 texelCenter = vec2(texel.x / 2.0, -texel.y / 2.0);
	return vec2(floor(coord.x * tileCount.x), ceil(coord.y * tileCount.y)) / tileCount + texelCenter;
}
vec4 get_data(uint offsetX, uint offsetY){
	vec2 tileCoord = get_tile_coord();
	vec2 texel = vec2(1.0) / tileCount / tileSize;
	tileCoord.x += texel.x * offsetX;
	tileCoord.y -= texel.y * offsetY;
	return texture2D(data, tileCoord);
}
vec4 get_area(uint offsetX, uint offsetY) {
	vec4 data = get_data(offsetX, offsetY);
	vec2 tileCoord = get_tile_coord();
	vec2 texel = 1.0 / tileCount / tileSize;
	vec2 tileSz = texel * tileSize;
	float x = data.r * tileSz.x + tileCoord.x - texel.x;
	float y = data.g * -tileSz.y + tileCoord.y + texel.y;
	float w = data.b * tileSz.x + texel.x / 2.0;
	float h = data.a * tileSz.y + texel.y / 2.0;
	return vec4(x, y, w, h);
}" +
            // light functions
            @"
int compute_out_code(vec2 p, vec2 rectMin, vec2 rectMax) {
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
}

bool is_shadow(vec2 p0, vec2 p1, vec2 rectMin, vec2 rectMax) {
	float flipY = rectMax.y - rectMin.y;
	rectMax.y -= flipY;
	rectMin.y -= flipY;

	int outcode0 = compute_out_code(p0, rectMin, rectMax);
	int outcode1 = compute_out_code(p1, rectMin, rectMax);

	bool accept = false;

	while (true) {
		if ((outcode0 | outcode1) == 0) {
			// Both points inside the rectangle, trivially accept
			accept = true;
			break;
		}
		else if ((outcode0 & outcode1) != 0)
			break; // Both points share an outside zone, trivially reject
		else {
			// Calculate the line segment to clip from an outside point to an intersection with the rectangle
			float x, y;
			int outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;

			if ((outcodeOut & 8) != 0) { // point is above the rectangle
				x = p0.x + (p1.x - p0.x) * (rectMax.y - p0.y) / (p1.y - p0.y);
				y = rectMax.y;
			}
			else if ((outcodeOut & 4) != 0) { // point is below the rectangle
				x = p0.x + (p1.x - p0.x) * (rectMin.y - p0.y) / (p1.y - p0.y);
				y = rectMin.y;
			}
			else if ((outcodeOut & 2) != 0) { // point is to the right of the rectangle
				y = p0.y + (p1.y - p0.y) * (rectMax.x - p0.x) / (p1.x - p0.x);
				x = rectMax.x;
			}
			else if ((outcodeOut & 1) != 0) { // point is to the left of the rectangle
				y = p0.y + (p1.y - p0.y) * (rectMin.x - p0.x) / (p1.x - p0.x);
				x = rectMin.x;
			}

			if (outcodeOut == outcode0) {
				p0 = vec2(x, y);
				outcode0 = compute_out_code(p0, rectMin, rectMax);
			}
			else {
				p1 = vec2(x, y);
				outcode1 = compute_out_code(p1, rectMin, rectMax);
			}
		}
	}
	return accept;
}
vec4 compute_lights(vec2 coord, vec4 color) {
	float maskOpacity = 0.0;
	vec2 viewSize = tileSize * tileCount;
	bool isFaded = !has_bit(lightFlags, 1);	
	bool isMask = has_bit(lightFlags, 2);
	bool isInverted = has_bit(lightFlags, 4);

	for(int i = 0; i < lightCount; i++)
	{
		vec3 l = light[i];
		vec4 c = lightColor[i];
		float aspect = viewSize.x / viewSize.y;
		float dist = distance(vec2(coord.x, coord.y / aspect), vec2(l.x, l.y / aspect));	

		if (dist >= l.z)
			continue;

		bool shadow = false;
		for(int j = 0; j < obstacleCount; j++)
		{
			vec4 area = obstacleArea[j];
			vec2 min = vec2(area.x, area.y);
			vec2 max = min + vec2(area.z, area.w);
			bool obstacleInShadow = is_inside(coord, area) && !has_bit(lightFlags, 8);						

			if (!obstacleInShadow && is_shadow(vec2(l.x, l.y), coord, min, max))
			{
				shadow = true;
				break;
			}
		}

		float attenuation = isFaded ? 1.0 - (dist / l.z) : 1.0;

		if (isMask)
			maskOpacity += shadow ? 0.0 : attenuation;
		else
		{
			color.rgb += c.rgb * c.a * attenuation;
			color.rgb -= shadow ? mix(vec3(0.0), color.rgb, c.a * attenuation) : vec3(0.0, 0.0, 0.0);
		}

	}
	color.a = isMask ? maskOpacity : color.a;
	color.a = isMask && isInverted ? 1.0 - color.a : color.a;

	return color;
}" +
            // wave functions
            @"
vec2 compute_waves(vec2 coord) {
	vec2 viewSize = tileSize * tileCount;
	vec4 multiplier = vec4(20.0, 20.0, 600.0, 600.0);
	vec2 texel = 1.0 / tileCount / tileSize;
	vec4 area = get_area(2, 1);
	vec4 spFr = get_data(3, 1);
	vec4 target = get_data(4, 1);
	bool is_target_color = target == vec4(0.0) ||
						  (target != vec4(0.0) && is(texture2D(texture, coord) * gl_Color, target));		
	
	spFr.x = spFr.x < 0.5 ? -spFr.x : spFr.x - 0.5;
	spFr.y = spFr.y < 0.5 ? -spFr.y : spFr.y - 0.5;
	spFr *= multiplier;

	if (is_inside(coord, area) && is_target_color) {
		coord.x += cos(coord.y * spFr.z + spFr.x * time) / viewSize.x / 1.5;
		coord.y += sin(coord.x * spFr.w + spFr.y * time) / viewSize.y / 1.5;
	}
	return coord;
}" +
// color adjust functions
            @"
vec4 compute_color_adjust(vec2 coord, vec4 color) {
	vec4 area = get_area(2, 0);

	if (!is_inside(coord, area))
		return color;
	
	vec4 data = get_data(3, 0);
	float gamma = data.x > 0.5 ? map(data.x, 0.5, 1.0, 2.0, 1.0) : map(data.x, 0.0, 0.5, 1.0, 0.0);
	
	color.rgb = pow(color.rgb, vec3(gamma));
	//float luminance = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
	//color.rgb = mix(vec3(luminance), color.rgb, data.y);
	//color.rgb = mix(vec3(0.5), color.rgb, data.z);
	//color.rgb *= data.w;
	//color *= tint;
	return color;
}" +

            // main
            @"
void main(void) {
	vec2 coord = compute_waves(gl_TexCoord[0].xy);
	vec4 color = texture2D(texture, coord) * gl_Color;
	
	color = compute_lights(coord, color);
	color = compute_color_adjust(coord, color);	

	gl_FragColor = color;
}";
    }
}

//// blur
//for(int i = 0; i < blurCount; i++)
//{
//	vec4 a = blurArea[i];
//	vec4 off = vec4(-pixel.x, -pixel.x, pixel.y, pixel.y);		
//	vec4 t = blurTarget[i];	
//	vec2 s = blurStrength[i];	
//	bool is_target_color = t == vec4(0.0) ||
//							(t != vec4(0.0) && is(texture2D(texture, coord) * gl_Color, t));		
//
//	if (is_inside(coord, a, off) && is_target_color)
//	{
//		vec2 blur = pixel * s;
//		vec4 blurResult =
//			texture2D(texture, coord) * 4.0 +
//			texture2D(texture, coord - blur.x) * 2.0 +
//			texture2D(texture, coord + blur.x) * 2.0 +
//			texture2D(texture, coord - blur.y) * 2.0 +
//			texture2D(texture, coord + blur.y) * 2.0 +
//			texture2D(texture, coord - blur.x - blur.y) * 1.0 +
//			texture2D(texture, coord - blur.x + blur.y) * 1.0 +
//			texture2D(texture, coord + blur.x - blur.y) * 1.0 +
//			texture2D(texture, coord + blur.x + blur.y) * 1.0;
//		color = (blurResult / 16.0);
//	}
//}
//
//// replace colors
//for(int i = 0; i < replaceCount; i++)
//    if (distance(color, replaceOld[i]) < 0.01)
//	    color = replaceNew[i];
//
//// edges
//vec3 u = (texture2D(texture, vec2(coord.x, coord.y - pixel.y)) * gl_Color).rgb;
//vec3 d = (texture2D(texture, vec2(coord.x, coord.y + pixel.y)) * gl_Color).rgb;
//vec3 l = (texture2D(texture, vec2(coord.x + pixel.x, coord.y)) * gl_Color).rgb;
//vec3 r = (texture2D(texture, vec2(coord.x - pixel.x, coord.y)) * gl_Color).rgb;
//
//vec3 ul = (texture2D(texture, vec2(coord.x + pixel.x, coord.y - pixel.y)) * gl_Color).rgb;
//vec3 dl = (texture2D(texture, vec2(coord.x + pixel.x, coord.y + pixel.y)) * gl_Color).rgb;
//vec3 ur = (texture2D(texture, vec2(coord.x - pixel.x, coord.y - pixel.y)) * gl_Color).rgb;
//vec3 dr = (texture2D(texture, vec2(coord.x - pixel.x, coord.y + pixel.y)) * gl_Color).rgb;
//
//for(int i = 0; i < edgeCount; i++)
//{
//	vec4 t = edgeTarget[i];
//	vec4 c = edgeColor[i];
//	vec4 a = edgeArea[i];
//	int type = edgeType[i];		
//
//	bool is_u = is(u, t);
//	bool is_d = is(d, t);
//	bool is_l = is(l, t);
//	bool is_r = is(r, t);
//
//	bool is_ul = !is_u && !is_l && is(ul, t);		
//	bool is_ur = !is_u && !is_r && is(ur, t);		
//	bool is_dl = !is_d && !is_l && is(dl, t);		
//	bool is_dr = !is_d && !is_r && is(dr, t);		
//	
//	vec4 off = vec4(-pixel.x * 1.5, pixel.x, 0, pixel.y * 1.5);
//
//	if (!is(color, t) && is_inside(coord, a, off))
//	{
//		// types: Top = 1, Bottom = 2, Left = 4, Right = 8, Corners = 16
//		bool corners = has_bit(type, 16) && (is_ul || is_dl || is_ur || is_dr);
//		bool top = has_bit(type, 1) && is_u;
//		bool bot = has_bit(type, 2) && is_d;
//		bool left = has_bit(type, 4) && is_l;
//		bool right = has_bit(type, 8) && is_r;
//
//		if (corners || top || bot || left || right)
//			color = c;
//	}
//}
//

//// color adjustments
//color.rgb = pow(color.rgb, vec3(1.0 / gamma));
//float luminance = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
//color.rgb = mix(vec3(luminance), color.rgb, saturation);
//color.rgb = mix(vec3(0.5), color.rgb, contrast);
//color.rgb *= brightness;
//color *= tint;