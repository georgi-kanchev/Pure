﻿namespace Pure.Tools.Particles;

internal class ClusterData(float timeLeft)
{
    public (float x, float y) gravity, orbitPoint, shake;
    public float friction, orbitRadius, size, timeLeft = timeLeft, bounceStrength = 0.5f, timeScale = 1f,
        varietyColor, varietyPushPullForce, varietyPushPullAngle, varietySourceAngle, varietySourceForce;

    public Action? sourceTick;
    public int sourceIndex;

    public readonly List<(float x, float y, float w, float h)> bounceRects = [];

    public (float x, float y, float width, float height)? wrapArea = null;
}