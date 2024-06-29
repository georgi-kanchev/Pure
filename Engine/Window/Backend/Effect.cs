namespace Pure.Engine.Window;

internal class Effect
{
    public Shader? Shader
    {
        get
        {
            if (shader != null)
                return shader;

            try
            {
                shader = Shader.FromString(Vertex, null, Fragment);
            }
            catch (Exception)
            {
                return shader;
            }

            return shader;
        }
    }

    protected virtual string Vertex
    {
        get =>
            @"
void main()
{
	gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
	gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	gl_FrontColor = gl_Color;
}";
    }
    protected virtual string Fragment
    {
        get =>
            @"
uniform sampler2D texture;

void main(void)
{
	gl_FragColor = texture2D(texture, gl_TexCoord[0].xy) * gl_Color;
}";
    }

#region Backend
    private Shader? shader;
#endregion
}