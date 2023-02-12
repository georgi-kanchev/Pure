namespace Pure.Particles;

public class ParticleManager
{
	public bool IsPaused { get; set; }
	public int Count => particles.Count;

	public void OnSpawn(Action<Particle> method)
		=> spawn = method;
	public void OnUpdate(Action<Particle> method)
		=> update = method;

	public void Spawn(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			var p = new Particle();
			particles.Add(p);
			spawn?.Invoke(p);
		}
	}
	public void Update(float deltaTime)
	{
		if (IsPaused)
			return;

		var toRemove = new List<Particle>();
		for (int i = 0; i < particles.Count; i++)
		{
			var p = particles[i];
			var rad = MathF.PI / 180 * p.MovementAngle;
			var (dirX, dirY) = (MathF.Cos(rad), MathF.Sin(rad));
			var (x, y) = p.Position;

			p.Age -= deltaTime;
			p.Position = (x + dirX * p.MovementSpeed, y + dirY * p.MovementSpeed);

			if (p.Age <= 0)
			{
				toRemove.Add(p);
				continue;
			}

			update?.Invoke(p);
		}
		for (int i = 0; i < toRemove.Count; i++)
			particles.Remove(toRemove[i]);
	}

	public static implicit operator ParticleManager(Particle[] particles)
	{
		var manager = new ParticleManager();
		for (int i = 0; i < particles.Length; i++)
			manager.particles.Add(particles[i]);
		return manager;
	}
	public static implicit operator Particle[](ParticleManager manager)
		=> manager.particles.ToArray();
	public static implicit operator (float, float)[](ParticleManager manager)
	{
		var positions = new (float, float)[manager.particles.Count];
		for (int i = 0; i < positions.Length; i++)
			positions[i] = manager.particles[i].Position;
		return positions;
	}

	#region Backend
	private Action<Particle>? spawn, update;

	private readonly List<Particle> particles = new();
	#endregion
}
