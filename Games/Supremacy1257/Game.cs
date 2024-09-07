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
        //navMap.ReducePointsOnTile(40f, world.Water);
        world.Layer.Fit();
        ui.Fit();

        var territories = new List<Territory>();

        Keyboard.Key.Space.OnPress(() => world.Layer.Fit());
        Mouse.Button.Left.OnPress(() =>
        {
            var mousePos = (Point)world.Layer.PixelToPosition(Mouse.CursorPosition);
            for (var i = 0; i < navMap.Points.Count; i++)
                if (navMap.Points[i].Position.Distance(mousePos) < 1f)
                {
                    var color = new[]
                    {
                        Color.Red.ToDark(), Color.Brown, Color.Azure, Color.Gray, Color.Purple, Color.Violet
                    }.ChooseOne();
                    var territory = new Territory(navMap.Points[i], world, new(world.Full, color));
                    territories.Add(territory);
                }
        });

        Window.PixelScale = 1f;

        while (Window.KeepOpen())
        {
            world.Update();
            navMap.Update();

            foreach (var territory in territories)
                territory.Update();

            world.Layer.Draw();

            ui.DrawCursor((69, 0).ToIndex1D(ui.AtlasTileCount));
            ui.Draw();
        }
    }
}