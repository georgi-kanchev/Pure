namespace Pure.Particles;

public class Particle
{
	public (float x, float y) Position { get; set; }
	public float MovementAngle { get; set; }
	public float MovementSpeed { get; set; } = 0.005f;

	public float Age { get; set; } = 1;
}
