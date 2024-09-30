namespace Pure;

using Examples.ExamplesSystems;
using Examples.ExamplesApplications;
using Examples.ExamplesUserInterface;

public static class Run
{
    public static void Main()
    {
        //Asteroids.Run();
        //FlappyBird.Run();
        //EightBallPool.Run();
        //Tetris.Run();
        //Minesweeper.Run();
        //Pong.Run();
        //Chat.Run();

        //Editors.EditorCollision.Program.Run();
        Editors.EditorMap.Program.Run();
        //Editors.EditorUserInterface.Program.Run();
        //Editors.EditorStorage.Program.Run();

        //RunExampleUserInterface();

        //DefaultGraphics.Run();
        //Commands.Run();
        //Storages.Run();
        //Collision.Run();
        //Pathfinding.Run();
        //LineOfSightAndLights.Run();
        //Audio.Run();
        //UtilityExtensions.Run();
        //Execution.Run();
    }

    private static void RunExampleUserInterface()
    {
        var (maps, ui) = Program.Initialize();
        //ui.AddRange(ButtonsAndCheckboxes.Create(maps));
        //ui.AddRange(SlidersAndScrolls.Create(maps));
        //ui.AddRange(FileViewers.Create(maps));
        //ui.AddRange(InputBoxes.Create(maps));
        //ui.AddRange(Panels.Create(maps));
        //ui.AddRange(Layouts.Create(maps));
        ui.Blocks.AddRange(Lists.Create(maps));
        //ui.AddRange(Steppers.Create(maps));
        //ui.AddRange(Pagination.Create(maps));
        //ui.AddRange(Prompts.Create(maps));
        //ui.AddRange(Palettes.Create(maps));
        Program.Run(maps, ui);
    }
}