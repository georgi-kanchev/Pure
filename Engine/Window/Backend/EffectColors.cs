namespace Pure.Engine.Window;

internal class EffectColors : Effect
{
    public override string Fragment
    {
        get =>
            @"
uniform sampler2D texture;

uniform float gamma = 1.0;
uniform float saturation = 1.0;
uniform float contrast = 1.0;
uniform float brightness = 1.0;

uniform int replaceColorsCount = 0;
uniform vec4 replaceColorsOld[99];
uniform vec4 replaceColorsNew[99];

uniform vec4 tint;
uniform vec4 overlay;

void main(void)
{
    vec4 color = texture2D(texture, gl_TexCoord[0].xy) * gl_Color;

    for(int i = 0; i < replaceColorsCount; i++)
	    if (distance(color, replaceColorsOld[i]) < 0.01)
		    color = replaceColorsNew[i];

    color.rgb = pow(color.rgb, vec3(1.0 / gamma));
    float luminance = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
    color.rgb = mix(vec3(luminance), color.rgb, saturation);
    color.rgb = mix(vec3(0.5), color.rgb, contrast);
    color.rgb *= brightness;

    color *= tint;
    color.rgb = mix(color.rgb, overlay.rgb, overlay.a);

	gl_FragColor = color;
}";
    }
}