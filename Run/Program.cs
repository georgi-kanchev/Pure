namespace Pure;

using Examples.ExamplesSystems;
using Examples.ExamplesApplications;
using Examples.ExamplesUserInterface;

public static class Run
{
    public static void Main()
    {
        Asteroids.Run();
        //EightBallPool.Run();
        //FlappyBird.Run();
        //Tetris.Run();
        //Minesweeper.Run();
        //Pong.Run();
        //Chat.Run();

        //Editors.EditorCollision.Program.Run();
        //Editors.EditorMap.Program.Run();
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
    }

    private static void RunExampleUserInterface()
    {
        var (maps, blocks) = Program.Initialize();
        //blocks.Add(ButtonsAndCheckboxes.Create(maps));
        //blocks.Add(SlidersAndScrolls.Create(maps));
        //blocks.Add(FileViewers.Create(maps));
        blocks.Add(InputBoxes.Create(maps));
        //blocks.Add(Panels.Create(maps));
        //blocks.Add(Layouts.Create(maps));
        //blocks.Add(Lists.Create(maps));
        //blocks.Add(Steppers.Create(maps));
        //blocks.Add(Pagination.Create(maps));
        //blocks.Add(Prompts.Create(maps));
        //blocks.Add(Palettes.Create(maps));
        Program.Run(maps, blocks);
    }
}