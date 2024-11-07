using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

using Engine.Storage;

public static class Storages
{
    public enum SomeEnum
    {
        Value1, SecondValue, ValueNumber3
    }

    [DoNotSave]
    public class Test
    {
        public readonly string c = "test";
        public uint a = 123;
        public int? b = 3;
        public SomeEnum someEnum = SomeEnum.ValueNumber3;
    }

    public class B : Test
    {
        public int someInt = 32;
    }

    public static void Run()
    {
        var tilemap = new Tilemap((48, 27));
        var layer = new Layer(tilemap.Size);

        tilemap.SetEllipse((48 / 2, 27 / 2), (10, 10), true, null, 1, 2, 3, 4, 5, 6, 7, 8, 9);

        var test = new B().ToBytes();
        var huh = test.ToObject<B>();

        while (Window.KeepOpen())
        {
            layer.DrawTilemap(tilemap);
            layer.DrawCursor();
            layer.Draw();
        }
    }
}