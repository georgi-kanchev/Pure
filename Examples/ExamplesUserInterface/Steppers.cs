namespace Pure.Examples.ExamplesUserInterface;

public static class Steppers
{
    public static Block[] Create(TilemapPack maps)
    {
        var stepper = new Stepper { Range = (-8, 10), Size = (12, 2) };
        stepper.Align((0.05f, 0.5f));
        stepper.OnDisplay(() =>
        {
            var (x, y) = stepper.Position;
            maps[0].SetTextLine((x, y - 1), "Step: 1 / Range: (-8, 10)");
            maps.SetStepper(stepper);
        });

        //==============

        var stepperDecimal = new Stepper { Step = 0.1f, Size = (12, 2) };
        stepperDecimal.Align((0.95f, 0.5f));
        stepperDecimal.OnDisplay(() =>
        {
            var (x, y) = stepperDecimal.Position;
            maps[0].SetTextLine((x, y - 1), "Step: 0.1");
            maps.SetStepper(stepperDecimal);
        });

        return new Block[] { stepper, stepperDecimal };
    }
}