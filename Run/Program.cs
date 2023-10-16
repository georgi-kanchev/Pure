namespace Pure;

using Examples.ExamplesUserInterface;

public static class Run
{
    public static void Main()
    {
        RunExampleUserInterface();
    }

    private static void RunExampleUserInterface()
    {
        var (maps, blocks) = Program.Initialize();
        blocks.Prompt = Prompts.Create(maps);
        //blocks.Add(ButtonsAndCheckboxes.Create(maps));
        //blocks.Add(SlidersAndScrolls.Create(maps));
        blocks.Add(FileViewers.Create(maps));
        //blocks.Add(InputBoxes.Create(maps));
        //blocks.Add(Panels.Create(maps));
        //blocks.Add(Layouts.Create(maps));
        //blocks.Add(Lists.Create(maps));
        //blocks.Add(Steppers.Create(maps));
        //blocks.Add(Pagination.Create(maps));
        Program.Run(maps, blocks);
    }
}