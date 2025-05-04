namespace Pure.Engine.Hardware;

[DoNotSave]
public class Monitor
{
	public string Name { get; set; } = string.Empty;
	public AreaI DesktopArea
	{
		get => area;
		set
		{
			area = value;
			AspectRatio = GetAspectRatio(value.width, value.height);
		}
	}
	public bool IsPrimary { get; set; }

	public SizeI AspectRatio { get; private set; }

	public override string ToString()
	{
		return Name;
	}

#region Backend
	private AreaI area;

	private static SizeI GetAspectRatio(int width, int height)
	{
		var gcd = height == 0 ? width : GetGreatestCommonDivisor(height, width % height);
		return (width / gcd, height / gcd);

		int GetGreatestCommonDivisor(int a, int b)
		{
			while (true)
			{
				if (b == 0)
					return a;
				var a1 = a;
				a = b;
				b = a1 % b;
			}
		}
	}
#endregion
}