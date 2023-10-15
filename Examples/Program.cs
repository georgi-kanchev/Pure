namespace Pure.Examples;

using Systems.UserInterface;
using Tilemap;
using UserInterface;
using Window;

public abstract class Program
{
    private static void Main()
    {
        // systems examples
        //DefaultGraphics.Run();
        // Collision.Run();
        // Audio.Run();
        // ChatLAN.Run();
        // Commands.Run();
        // Storage.Run();
        // UtilityExtensions.Run();

        // user interface examples
        Window.Create(3);
        var (width, height) = Window.MonitorAspectRatio;
        var maps = new TilemapManager(7, (width * 3, height * 3));
        var ui = new UserInterface();
        Input.TilemapSize = maps.Size;
        //ui.Prompt = Prompts.Create(maps);
        //ui.Add(ButtonsAndCheckboxes.Create(maps));
        //ui.Add(SlidersAndScrolls.Create(maps));
        //ui.Add(FileViewers.Create(maps));
        //ui.Add(InputBoxes.Create(maps));
        //ui.Add(Panels.Create(maps));
        //ui.Add(Layouts.Create(maps));
        //ui.Add(Lists.Create(maps));
        //ui.Add(Steppers.Create(maps));
        //ui.Add(Pagination.Create(maps));
        ui.Add(Palettes.Create(maps));
        Utility.Run(ui, maps);

        // games examples
        // FlappyBird.Run();
        // Ludo.Run();
    }
}