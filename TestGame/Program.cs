using Pure.Tilemap;
using Pure.Utilities;
using Pure.Window;

namespace TestGame
{
    public class Program
    {
        // https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83
        // https://chillmindscapes.itch.io/
        // https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/
        // noise function
        // inputline double click selection and ctrl+z/y
        // tilemap editor collisions

        static void Main()
        {
            var t = new Tilemap("world.map");

            t.SetTile((47, 26), Tile.ARROW_DOWN, Color.Red);

            t.CameraSize = (32, 18);
            while (Window.IsExisting)
            {
                Window.Activate(true);

                Window.IsRetro = KeyboardKey.IsPressed(KeyboardKey.A) == false;

                var (x, y) = t.PositionFrom(MouseCursor.Position, Window.Size, false);
                t.CameraPosition = ((int)x - 30, (int)y);
                var cam = t.CameraUpdate();
                Window.DrawTilemap(cam, cam, (12, 12), (1, 1), "urizen.png");

                Window.Activate(false);
            }
        }
    }
}