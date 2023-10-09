namespace Pure.Particles;

/// <summary>
/// Represents a particle with position, movement angle and speed, age and color.
/// </summary>
public class Particle
{
    /// <summary>
    /// Gets or sets the position of the particle.
    /// </summary>
    public (float x, float y) Position
    {
        get;
        set;
    }
    /// <summary>
    /// Gets or sets the movement (angle and speed) of the particle.
    /// </summary>
    public (float angle, float speed) Movement
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the age of the particle, in seconds.
    /// </summary>
    public float Age
    {
        get;
        set;
    } = 1;
    /// <summary>
    /// Gets or sets the color of the particle.
    /// </summary>
    public uint Color
    {
        get;
        set;
    } = uint.MaxValue;

    /// <summary>
    /// Initializes a new particle instance with the specified position, 
    /// movement angle, movement speed, 
    /// age and color.
    /// </summary>
    /// <param name="position">The position of the particle.</param>
    /// <param name="movement">The movement (angle and speed) of the particle.</param>
    /// <param name="age">The age of the particle.</param>
    /// <param name="color">The color of the particle.</param>
    public Particle((float x, float y) position, (float angle, float speed) movement, float age = 1f, uint color = uint.MaxValue)
    {
        Position = position;
        Movement = movement;
        Age = age;
        Color = color;
    }

    /// <returns>
    /// A point bundle tuple containing the position and color of the particle.</returns>
    public (float x, float y, uint color) ToPointBundle()
    {
        return this;
    }
    /// <returns>
    /// A bundle tuple containing the position, color, movement (angle and speed) and age of the particle.</returns>
    public (float x, float y, uint color, float angle, float speed, float age) ToBundle()
    {
        return this;
    }

    /// <param name="particle">
    /// The particle to convert.</param>
    /// <returns>A point bundle tuple containing the position and color of the particle.</returns>
    public static implicit operator (float x, float y, uint color)(Particle particle)
    {
        return (particle.Position.x, particle.Position.y, particle.Color);
    }
    /// <param name="particle">
    /// The particle to convert.</param>
    /// <returns>A bundle tuple containing the position, color, movement (angle and speed) and age of the particle.</returns>
    public static implicit operator (float x, float y, uint color, float angle, float speed, float age)(Particle particle)
    {
        return (particle.Position.x, particle.Position.y, particle.Color, particle.Movement.angle, particle.Movement.speed, particle.Age);
    }
}