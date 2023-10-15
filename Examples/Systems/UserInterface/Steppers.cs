namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using static Utility;

public static class Steppers
{
    public static Element[] Create(TilemapManager maps)
    {
        var stepper = new Stepper { Range = (-8, 10), Size = (12, 2) };
        stepper.Align((0.05f, 0.5f));
        stepper.OnDisplay(() =>
        {
            var (x, y) = stepper.Position;
            maps[0].SetTextLine((x, y - 1), "Step: 1 / Range: (-8, 10)");
            DisplayStepper(maps, stepper, zOrder: 0);
        });

        //==============

        var stepperDecimal = new Stepper { Step = 0.1f, Size = (12, 2) };
        stepperDecimal.Align((0.95f, 0.5f));
        stepperDecimal.OnDisplay(() =>
        {
            var (x, y) = stepperDecimal.Position;
            maps[0].SetTextLine((x, y - 1), "Step: 0.1");
            DisplayStepper(maps, stepperDecimal, zOrder: 0);
        });

        return new Element[] { stepper, stepperDecimal };
    }
    public static void DisplayStepper(TilemapManager maps, Stepper stepper, int zOrder)
    {
        var e = stepper;
        var text = MathF.Round(e.Step, 2).Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";
        var color = Color.Gray.ToBright();

        SetBackground(maps[zOrder], stepper);

        maps[zOrder + 1].SetTile(
            position: e.Decrease.Position,
            tile: new(Tile.ARROW, GetColor(e.Decrease, color), angle: 1));
        maps[zOrder + 1].SetTile(
            e.Increase.Position,
            tile: new(Tile.ARROW, GetColor(e.Increase, color), angle: 3));
        maps[zOrder + 1].SetTextLine(
            position: (e.Position.x + 2, e.Position.y),
            e.Text);
        maps[zOrder + 1].SetTextLine(
            position: (e.Position.x + 2, e.Position.y + 1),
            text);

        maps[zOrder + 1].SetTile(
            position: e.Minimum.Position,
            tile: new(Tile.MATH_MUCH_LESS, GetColor(e.Minimum, color)));
        maps[zOrder + 1].SetTile(
            position: e.Middle.Position,
            tile: new(Tile.PUNCTUATION_PIPE, GetColor(e.Middle, color)));
        maps[zOrder + 1].SetTile(
            position: e.Maximum.Position,
            tile: new(Tile.MATH_MUCH_GREATER, GetColor(e.Maximum, color)));
    }
}