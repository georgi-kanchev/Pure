using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Pure;

using Examples.ExamplesUserInterface;

public static class Run
{
    public static void Main()
    {
        //Examples.ExamplesApplications.Pong.Run();
        //Examples.ExamplesApplications.FlappyBird.Run();

        //Editors.EditorMap.Program.Run();
        //Editors.EditorUserInterface.Program.Run();

        //RunExampleUserInterface();

        //Examples.ExamplesSystems.DefaultGraphics.Run();
        //Examples.ExamplesSystems.Collision.Run();
        //Examples.ExamplesSystems.Pathfinding.Run();
        Examples.ExamplesSystems.Chat.Run();
        //Examples.ExamplesSystems.UtilityExtensions.Run();
    }

    private static void RunExampleUserInterface()
    {
        var (maps, blocks) = Program.Initialize();
        //blocks.Add(ButtonsAndCheckboxes.Create(maps));
        //blocks.Add(SlidersAndScrolls.Create(maps));
        //blocks.Add(FileViewers.Create(maps));
        //blocks.Add(InputBoxes.Create(maps));
        //blocks.Add(Panels.Create(maps));
        //blocks.Add(Layouts.Create(maps));
        //blocks.Add(Lists.Create(maps));
        //blocks.Add(Steppers.Create(maps));
        //blocks.Add(Pagination.Create(maps));
        //blocks.Add(Prompts.Create(maps));
        Program.Run(maps, blocks);
    }
}