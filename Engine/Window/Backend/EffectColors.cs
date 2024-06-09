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

uniform vec4 replaceColorOld;
uniform vec4 replaceColorNew;

uniform vec4 tint;

void main(void)
{
    vec4 color = texture2D(texture, gl_TexCoord[0].xy) * gl_Color;

    if (distance(color, replaceColorOld) < 0.01)
        color = replaceColorNew;

    color.rgb = pow(color.rgb, vec3(1.0 / gamma));
    float luminance = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
    color.rgb = mix(vec3(luminance), color.rgb, saturation);
    color.rgb = mix(vec3(0.5), color.rgb, contrast);
    color.rgb *= brightness;

    color *= tint;

	gl_FragColor = color;
}";
    }
}