using Pure.Engine.Collision;

namespace Pure.Tools.Particles;

internal class ClusterData(float timeLeft)
{
    public (float x, float y) gravity, orbitPoint, shake, teleportPoint, teleportForce;
    public float friction, orbitRadius, size, timeLeft = timeLeft, bounceStrength = 0.5f, timeScale = 1f,
        varietyColor, varietyPushPullForce, varietyPushPullAngle, varietyTeleportAngle, varietyTeleportForce,
        teleportInterval;
    public int teleportIndex, teleportStep;

    public Action? teleportTick;
    public Action<int>? teleport;
    public Action<(int particleIndex, int triggerIndex)>? trigger, triggerEnter, triggerExit, collision;

    public readonly SolidPack obstacles = new();
    public readonly SolidPack triggers = new();
    public readonly Dictionary<int, int> triggering = []; // particleIndex, triggerIndex
    public readonly Dictionary<int, (float timeLeft, uint toColor, float duration, uint fromColor)> colorFades = [];

    public (float x, float y, float width, float height)? wrapArea = null;
}