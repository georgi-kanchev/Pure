namespace Pure.Examples.UserInterface;

public static class Steppers
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Steppers Example";

        var stepper = new Stepper { Range = (-8, 10), Size = (12, 2) };
        stepper.AlignInside((0.05f, 0.5f));
        stepper.OnDisplay(() =>
        {
            var (x, y) = stepper.Position;
            maps.Tilemaps[0].SetText((x, y - 1), "Step: 1 / Range: (-8, 10)");
            maps.SetStepper(stepper);
        });

        //==============

        var stepperDecimal = new Stepper { Step = 0.1f, Size = (12, 2) };
        stepperDecimal.AlignInside((0.95f, 0.5f));
        stepperDecimal.OnDisplay(() =>
        {
            var (x, y) = stepperDecimal.Position;
            maps.Tilemaps[0].SetText((x, y - 1), "Step: 0.1");
            maps.SetStepper(stepperDecimal);
        });

        return [stepper, stepperDecimal];
    }
}