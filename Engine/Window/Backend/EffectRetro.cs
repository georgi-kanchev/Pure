namespace Pure.Engine.Window;

internal class EffectRetro : Effect
{
    public override string Fragment
    {
        get =>
            @"
uniform sampler2D texture;
uniform vec2 viewSize;
uniform vec2 offScreen;
uniform vec2 randomVec;
uniform float time;
uniform float turnoffAnimation;

uniform vec2 curvature = vec2(2.5, 2.5);
uniform vec2 scanLineOpacity = vec2(0.6, 1.0);
uniform float vignetteOpacity = 1.0;
uniform float brightness = 3.0;
uniform float vignetteRoundness = 10.0;
uniform float whiteNoiseAmount = 1.0;
uniform float scanLineSize = 1.0;
uniform float scanLineSpeed = 5.0;

vec2 hash(vec2 p)
{
	p = vec2(dot(p,vec2(127.1,311.7)), dot(p,vec2(269.5,183.3)));
	return -1.0 + 2.0*fract(sin(p)*43758.5453123);
}
float noise(in vec2 p)
{
	const float K1 = 0.366025404; // (sqrt(3)-1)/2;
	const float K2 = 0.211324865; // (3-sqrt(3))/6;

	vec2  i = floor(p + (p.x+p.y)*K1);
	vec2  a = p - i + (i.x+i.y)*K2;
	float m = step(a.y,a.x); 
	vec2  o = vec2(m,1.0-m);
	vec2  b = a - o + K2;
	vec2  c = a - 1.0 + 2.0*K2;
	vec3  h = max(0.5-vec3(dot(a,a), dot(b,b), dot(c,c)), 0.0);
	vec3  n = h*h*h*h*vec3(dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
	return dot(n, vec3(70.0));
}
vec2 curveRemapUV(vec2 uv)
{
	uv = uv * 2.0-1.0;
	vec2 offset = abs(uv.yx) / vec2(curvature.x, curvature.y);
	uv = uv + uv * offset * offset;
	uv = uv * 0.5 + 0.5;
	return uv;
}
vec4 scanLineIntensity(float uv, float resolution, float opacity)
{
	float intensity = sin(uv * resolution * 2.0);
	intensity = ((0.5 * intensity) + 0.5) * 0.9 + 0.1;
	return vec4(vec3(pow(intensity, opacity)), 1.0);
}
vec4 vignetteIntensity(vec2 uv, vec2 resolution, float opacity, float roundness)
{
	float intensity = uv.x * uv.y * (1.0 - uv.x) * (1.0 - uv.y);
	return vec4(vec3(clamp(pow((resolution.x / roundness) * intensity, opacity), 0.0, 1.0)), 1.0);
}
void main(void)
{
	vec2 coord = gl_FragCoord / viewSize - offScreen / viewSize;
	vec2 remappedUV = curveRemapUV(coord);
	vec4 baseColor = texture2D(texture, gl_TexCoord[0].xy) * gl_Color;
	float scanDarkMultiplier = (1.0 + sin((-time * scanLineSpeed) + coord.y * (10.0 / scanLineSize)) / 2.0);
	float f = noise(64.0 * (coord + randomVec));
	float o = 1.0 - turnoffAnimation * 1.5;
	f = 0.5 + 0.5*f;
	
	//baseColor.rgba += f / 3;
	baseColor *= vignetteIntensity(remappedUV, viewSize, vignetteOpacity, vignetteRoundness);
	baseColor *= scanLineIntensity(remappedUV.x, viewSize.y, scanLineOpacity.x);
	baseColor *= scanLineIntensity(remappedUV.y, viewSize.x, scanLineOpacity.y);
	baseColor *= vec4(vec3(brightness + f * 2), 1.0);
	baseColor.rgb *= scanDarkMultiplier + 0.9;
	baseColor.rgba += f * 10 * turnoffAnimation;
	
	if (remappedUV.x < 0.0 || remappedUV.y < 0.0 || remappedUV.x > 1.0 || remappedUV.y > 1.0 ||
		coord.y < 0.48 - o || coord.y > 0.52 + o)
		baseColor = vec4(0.0, 0.0, 0.0, 1.0);
	
	if (turnoffAnimation > 0.5)
	{
		float dist = distance(vec2(coord.x, coord.y), vec2(0.5)) / 4.0 + 0.5;
		vec4 turnoff = vec4(1.0) - turnoffAnimation / 2.0 - dist;
		vec4 target = max(vec4(0.5), turnoff * 100.0 * turnoffAnimation);
		baseColor = mix(baseColor, target, turnoffAnimation / 5.0);
	}
	
	gl_FragColor = baseColor;
}";
    }
}