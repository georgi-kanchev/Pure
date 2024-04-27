namespace Pure.Engine.Utilities;

public struct Angle
{
    public static Angle Up
    {
        get => 270;
    }
    public static Angle UpLeft
    {
        get => 225;
    }
    public static Angle UpRight
    {
        get => 315;
    }
    public static Angle Down
    {
        get => 90;
    }
    public static Angle DownLeft
    {
        get => 135;
    }
    public static Angle DownRight
    {
        get => 45;
    }
    public static Angle Left
    {
        get => 180;
    }
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
    public (float x, float y) Direction
    {
        get
        {
            var rad = Radians;
            return (MathF.Cos(rad), MathF.Sin(rad));
        }
        set => Degrees = MathF.Atan2(value.x, value.y) * (180f / MathF.PI);
    }

    public Angle(float degrees)
    {
        this.degrees = 0;
        Degrees = degrees;
    }
    public Angle((float x, float y) direction)
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
        else if (difference is >= -180 and < 0) return true;
        else if (difference is >= -360 and < -180) return false;
        else if (difference is >= 180 and < 360) return true;

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

    public static Angle BetweenPoints((float x, float y) point, (float x, float y) target)
    {
        var (x, y) = (target.x - point.x, target.y - point.y);
        var m = MathF.Sqrt(x * x + y * y);

        return (x / m, y / m);
    }
    public static Angle BetweenPoints(
        (float x, float y, uint color) point,
        (float x, float y, uint color) target)
    {
        return BetweenPoints((point.x, point.y), (target.x, target.y));
    }

    public override string ToString()
    {
        return $"{degrees}°";
    }

    public static implicit operator Angle((int x, int y) direction)
    {
        var result = MathF.Atan2(direction.y, direction.x) * (180f / MathF.PI);
        return new() { Degrees = result };
    }
    public static implicit operator (int x, int y)(Angle angle)
    {
        var rad = MathF.PI / 180 * angle;
        return ((int)MathF.Round(MathF.Cos(rad)), (int)MathF.Round(MathF.Sin(rad)));
    }
    public static implicit operator Angle((float x, float y) direction)
    {
        var result = MathF.Atan2(direction.y, direction.x) * (180f / MathF.PI);
        return new() { Degrees = result };
    }
    public static implicit operator (float x, float y)(Angle angle)
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