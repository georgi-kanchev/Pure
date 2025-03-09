using Position = (float x, float y);
using DirectionF = (float x, float y);
using DirectionI = (int x, int y);
using PositionColored = (float x, float y, uint color);

namespace Pure.Engine.Utility;

public struct Angle
{
    /// <summary>
    /// Degrees = 270
    /// </summary>
    public static Angle Up
    {
        get => 270;
    }
    /// <summary>
    /// Degrees = 225
    /// </summary>
    public static Angle UpLeft
    {
        get => 225;
    }
    /// <summary>
    /// Degrees = 315
    /// </summary>
    public static Angle UpRight
    {
        get => 315;
    }
    /// <summary>
    /// Degrees = 90
    /// </summary>
    public static Angle Down
    {
        get => 90;
    }
    /// <summary>
    /// Degrees = 135
    /// </summary>
    public static Angle DownLeft
    {
        get => 135;
    }
    /// <summary>
    /// Degrees = 45
    /// </summary>
    public static Angle DownRight
    {
        get => 45;
    }
    /// <summary>
    /// Degrees = 180
    /// </summary>
    public static Angle Left
    {
        get => 180;
    }
    /// <summary>
    /// Degrees = 0
    /// </summary>
    public static Angle Right
    {
        get => 0;
    }

    public bool IsNaN
    {
        get => float.IsNaN(degrees);
    }

    public float Degrees
    {
        get => degrees;
        set => degrees = (value % 360 + 360) % 360;
    }
    public float Radians
    {
        get => MathF.PI / 180f * degrees;
        set => Degrees = value * (180f / MathF.PI);
    }
    public DirectionF Direction
    {
        get
        {
            var rad = Radians;
            return (MathF.Cos(rad), MathF.Sin(rad));
        }
        set => Degrees = MathF.Atan2(value.y, value.x) * (180f / MathF.PI);
    }

    public Angle(float degrees)
    {
        this.degrees = 0;
        Degrees = degrees;
    }
    public Angle(DirectionF direction)
    {
        degrees = 0;
        Direction = direction;
    }
    public Angle(float directionX, float directionY) : this((directionX, directionY))
    {
    }

    public Angle Rotate(float speed, float deltaTime = 1)
    {
        return this + speed * deltaTime;
    }
    public Angle RotateTo(Angle target, float speed, float deltaTime = 1)
    {
        speed = Math.Abs(speed);
        var angle = this;
        var difference = angle - target;

        // stops the rotation with an else when close enough
        // prevents the rotation from staying behind after the stop
        var checkedSpeed = speed * deltaTime;
        var result = angle.Rotate(speed * (IsBehind(target) ? 1 : -1), deltaTime);
        angle = Math.Abs(difference) < checkedSpeed ? target : result;

        // detects speed greater than possible
        // prevents jiggle when passing 0-360 & 360-0 | simple to fix yet took me half a day
        if (Math.Abs(difference) > 360 - checkedSpeed)
            angle = target;

        return angle;
    }
    public Angle PercentTo(float percent, Angle target)
    {
        percent /= 100f;

        var difference = target.degrees - degrees;

        if (difference <= -180)
            difference += 360;
        else if (difference > 180)
            difference -= 360;

        var interpolatedDifference = difference * percent;
        var interpolatedAngle = degrees + interpolatedDifference;

        return interpolatedAngle;
    }
    public Angle Limit((Angle lower, Angle upper) range)
    {
        if (IsWithin(range) == false)
            return Difference(range.lower) < Difference(range.upper) ? range.lower : range.upper;

        return this;
    }
    public bool IsWithin((Angle lower, Angle upper) range)
    {
        if (IsBehind(range.lower) || IsBehind(range.upper) == false)
            return false;

        return Difference(range.lower) < 180 && Difference(range.upper) < 180;
    }
    public bool IsBehind(Angle target)
    {
        var difference = degrees - target.degrees;
        if (difference is >= 0 and < 180) return false;
        if (difference is >= -180 and < 0) return true;
        if (difference is >= -360 and < -180) return false;
        if (difference is >= 180 and < 360) return true;

        return false;
    }
    public float Difference(Angle target)
    {
        var difference = target - degrees;
        if (difference < -180)
            difference += 360;
        else if (difference >= 180)
            difference -= 360;
        return Math.Abs(difference);
    }
    public float Dot(Angle target)
    {
        var dir = Direction;
        var targetDir = target.Direction;
        return dir.x * targetDir.x + dir.y * targetDir.y;
    }
    public Angle Reflect(Angle surfaceAngle)
    {
        return 2 * surfaceAngle - degrees + 180;
    }
    public Angle Reverse()
    {
        return degrees - 180;
    }

    public static Angle BetweenPoints(Position point, Position target)
    {
        var (x, y) = (target.x - point.x, target.y - point.y);
        var m = MathF.Sqrt(x * x + y * y);

        return (x / m, y / m);
    }
    public static Angle BetweenPoints(PositionColored point, PositionColored target)
    {
        return BetweenPoints((point.x, point.y), (target.x, target.y));
    }

    public override string ToString()
    {
        return $"{degrees}°";
    }
    public bool Equals(Angle other)
    {
        return degrees.Equals(other.degrees);
    }
    public override bool Equals(object? obj)
    {
        return obj is Angle other && Equals(other);
    }
    public override int GetHashCode()
    {
        return degrees.GetHashCode();
    }

    public static bool operator ==(Angle left, Angle right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Angle left, Angle right)
    {
        return left.Equals(right) == false;
    }

    public static implicit operator Angle(DirectionI direction)
    {
        var result = MathF.Atan2(direction.y, direction.x) * (180f / MathF.PI);
        return new() { Degrees = result };
    }
    public static implicit operator DirectionI(Angle angle)
    {
        var rad = MathF.PI / 180 * angle;
        return ((int)MathF.Round(MathF.Cos(rad)), (int)MathF.Round(MathF.Sin(rad)));
    }
    public static implicit operator Angle(DirectionF direction)
    {
        var result = MathF.Atan2(direction.y, direction.x) * (180f / MathF.PI);
        return new() { Degrees = result };
    }
    public static implicit operator DirectionF(Angle angle)
    {
        var rad = MathF.PI / 180 * angle;
        return (MathF.Cos(rad), MathF.Sin(rad));
    }
    public static implicit operator Angle(int degrees)
    {
        return new() { Degrees = degrees };
    }
    public static implicit operator int(Angle angle)
    {
        return (int)MathF.Round(angle.degrees);
    }
    public static implicit operator Angle(float degrees)
    {
        return new() { Degrees = degrees };
    }
    public static implicit operator float(Angle angle)
    {
        return angle.degrees;
    }

#region Backend
    private float degrees;
#endregion
}