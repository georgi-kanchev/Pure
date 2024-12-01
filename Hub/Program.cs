using Pure.Engine.UserInterface;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Pure.Examples.Systems;
using Pure.Examples.Games;
using Pure.Examples.UserInterface;
using Pure.Tools.Tilemap;

namespace Pure.Hub;

public static class Program
{
    public static void Main()
    {
        Storages.Run();

        var (maps, ui) = Examples.UserInterface.Program.Initialize();
        var editors = new List((0, 0), 3) { Size = (14, 3), ItemSize = (14, 1), Text = "Editors:" };
        var apps = new List((0, 0), 9) { Size = (15, 9), ItemSize = (15, 1), Text = "Example Games:" };
        var uis = new List((0, 0), 11) { Size = (20, 11), ItemSize = (20, 1), Text = "Example UI:" };
        var systems = new List((0, 0), 7) { Size = (22, 7), ItemSize = (22, 1), Text = "Example Systems:" };

        editors.Edit(["Collision", "Map", "User Interface"]);
        editors.AlignInside((0.75f, 0.25f));
        OnTrigger(editors, Editors.Collision.Program.Run, Editors.Map.Program.Run,
            Editors.UserInterface.Program.Run);
        OnDisplay(editors, () =>
        {
            maps.Tilemaps[0].SetText((0, 0),
                "(click on a Hub Project to start it)".Constrain(maps.Size, alignment: Alignment.Top));
        });

        apps.Edit([
            "Asteroids", "Chat", "Eight Ball Pool", "Flappy Bird", "Minesweeper", "Pong", "Tetris",
            "Number Guess"
        ]);
        apps.AlignInside((0.18f, 0.25f));
        OnTrigger(apps, Asteroids.Run, Chat.Run, EightBallPool.Run, FlappyBird.Run, Minesweeper.Run,
            Pong.Run, Tetris.Run, NumberGuess.Run);
        OnDisplay(apps);

        uis.Edit([
            "Buttons & Checkboxes", "Sliders & Scrolls", "Input Boxes", "Steppers", "Panels",
            "Pagination", "Lists", "Prompts", "Layouts", "File Viewers", "Palettes"
        ]);
        uis.AlignInside((0.95f, 0.8f));
        OnTrigger(uis,
            () => RunUI(ButtonsAndCheckboxes.Create(maps)), () => RunUI(SlidersAndScrolls.Create(maps)),
            () => RunUI(InputBoxes.Create(maps)), () => RunUI(Steppers.Create(maps)),
            () => RunUI(Panels.Create(maps)), () => RunUI(Pagination.Create(maps)),
            () => RunUI(Lists.Create(maps)), () => RunUI(Prompts.Create(maps)),
            () => RunUI(Layouts.Create(maps)), () => RunUI(FileViewers.Create(maps)),
            () => RunUI(Palettes.Create(maps)));
        OnDisplay(uis);

        systems.Edit([
            "Default Graphics", "Collision", "Line of Sight & Lights", "Pathfinding", "Audio",
            "Terrain Generation", "Immediate GUI"
        ]);
        systems.AlignInside((0.05f, 0.77f));
        OnTrigger(systems, DefaultGraphics.Run, Collision.Run, LineOfSightAndLights.Run,
            Pathfinding.Run, Audio.Run, TerrainGeneration.Run, ImmediateGui.Run);
        OnDisplay(systems);

        ui.Blocks.AddRange([editors, apps, uis, systems]);
        Window.Title = "Pure - Hub";
        Examples.UserInterface.Program.Run(maps, ui);

        void RunUI(Block[] blocks)
        {
            var (_, newUI) = Examples.UserInterface.Program.Initialize();
            newUI.Blocks.AddRange(blocks);
            Examples.UserInterface.Program.Run(maps, newUI);
        }

        void OnTrigger(List list, params Action[] actions)
        {
            for (var i = 0; i < actions.Length; i++)
            {
                var index = i;
                list.Items[i].OnInteraction(Interaction.Trigger, () =>
                {
                    Mouse.CursorCurrent = Mouse.Cursor.Arrow;
                    actions[index].Invoke();
                });
            }
        }

        void OnDisplay(List list, Action? extraCode = null)
        {
            list.OnDisplay += () =>
            {
                extraCode?.Invoke();
                maps.Tilemaps[0].SetText((list.Position.x, list.Position.y - 1), list.Text);
                maps.SetList(list);
            };
            list.OnItemDisplay += item => maps.SetListItem(list, item);
        }
    }
}