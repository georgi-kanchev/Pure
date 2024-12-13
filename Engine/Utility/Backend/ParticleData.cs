namespace Pure.Engine.Utility;

internal class ParticleData(float timeLeft)
{
    public float TimeLeft { get; set; } = timeLeft;
    public (float x, float y) Movement { get; set; }

    public (float x, float y) Gravity { get; set; }
    public float Friction { get; set; }
    public (float x, float y) OrbitPoint { get; set; }
    public float OrbitRadius { get; set; }
}