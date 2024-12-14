namespace Pure.Engine.Utility;

internal class ClusterData(float timeLeft)
{
    public float timeLeft = timeLeft;
    public float timeScale = 1f;
    public (float x, float y) gravity;
    public float friction;

    public (float x, float y) orbitPoint;
    public float orbitRadius;

    public float bounceStrength = 0.5f;
    public float size;
    public readonly List<(float x, float y, float w, float h)> bounceRects = [];
}