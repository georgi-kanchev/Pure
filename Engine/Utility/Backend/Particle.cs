namespace Pure.Engine.Utility;

internal class Particle((float x, float y) position, (float time, Behavior behavior)[] behaviorChain)
{
    public (float x, float y) Position { get; set; } = position;
    public (float x, float y) Movement { get; set; }
    public (float time, Behavior behavior)[] BehaviorChain { get; } = behaviorChain;

    public float Age { get; set; }
    public uint Color { get; set; }
}