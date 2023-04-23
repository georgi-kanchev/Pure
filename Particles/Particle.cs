namespace Pure.Particles;

/// <summary>
/// Represents a particle with position, movement angle and speed, age and color.
/// </summary>
public class Particle
{
	/// <summary>
	/// Gets or sets the position of the particle.
	/// </summary>
	public (float x, float y) Position { get; set; }
	/// <summary>
	/// Gets or sets the movement angle of the particle.
	/// </summary>
	public float MovementAngle { get; set; }
	/// <summary>
	/// Gets or sets the movement speed of the particle.
	/// </summary>
	public float MovementSpeed { get; set; }

	/// <summary>
	/// Gets or sets the age of the particle, in seconds.
	/// </summary>
	public float Age { get; set; } = 1;
	/// <summary>
	/// Gets or sets the color of the particle.
	/// </summary>
	public uint Color { get; set; } = uint.MaxValue;

	/// <summary>
	/// Initializes a new particle instance with the specified <paramref name="position"/>, 
	/// <paramref name="movementAngle"/>, <paramref name="movementSpeed"/>, 
	/// <paramref name="age"/> and <paramref name="color"/>.
	/// </summary>
	/// <param name="position">The position of the particle.</param>
	/// <param name="movementAngle">The movement angle of the particle.</param>
	/// <param name="movementSpeed">The movement speed of the particle.</param>
	/// <param name="age">The age of the particle.</param>
	/// <param name="color">The color of the particle.</param>
	public Particle((float x, float y) position, float movementAngle, float movementSpeed = 0.005f, float age = 1f, uint color = uint.MaxValue)
	{
		Position = position;
		MovementAngle = movementAngle;
		MovementSpeed = movementSpeed;
		Age = age;
		Color = color;
	}

	/// <returns>
	/// A bundle tuple containing the position and color of the particle.</returns>
	public ((float x, float y) position, uint color) ToBundle() => this;

	/// <summary>
	/// Implicitly converts a particle to a bundle tuple of its position and color.
	/// </summary>
	/// <param name="particle">The particle to convert.</param>
	/// <returns>A bundle tuple containing the position and color of the particle.</returns>
	public static implicit operator ((float x, float y) position, uint color)(Particle particle)
		=> (particle.Position, particle.Color);
}
