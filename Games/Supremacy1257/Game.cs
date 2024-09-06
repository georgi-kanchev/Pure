using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Monitor = Pure.Engine.Window.Monitor;

namespace Supremacy1257;

public class Game
{
    public const int SEED = 0;

    private static void Main()
    {
        Window.Title = "Supremacy 1257";
        var aspect = Monitor.Current.AspectRatio;
        var ui = new Layer((aspect.width * 5, aspect.height * 5))
        {
            AtlasPath = "urizen.png",
            AtlasTileGap = (1, 1),
            AtlasTileSize = (12, 12)
        };
        var world = new World(100, 100);
        var navMap = new NavMap(world);
        navMap.ReducePointsOnTile(40f, world.Water);
        world.Layer.Fit();

        Keyboard.Key.Space.OnPress(() => world.Layer.Fit());

        Window.PixelScale = 1f;
        ui.Zoom = 2f;

        while (Window.KeepOpen())
        {
            world.Update();
            navMap.Update();

            ui.DrawCursor((69, 0).ToIndex1D(ui.AtlasTileCount));
            ui.Draw();
        }
    }
}