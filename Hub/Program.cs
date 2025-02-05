using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Examples.Systems;
using Pure.Examples.Games;
using Pure.Examples.UserInterface;
using Pure.Tools.Tiles;

namespace Pure.Hub;

public static class Program
{
    public static void Main()
    {
        Storages.Run();

        var (maps, ui) = Examples.UserInterface.Program.Initialize();
        var editors = new List((0, 0), 2) { Size = (14, 2), ItemSize = (14, 1), Text = "Editors:" };
        var apps = new List((0, 0), 8) { Size = (15, 8), ItemSize = (15, 1), Text = "Example Games:" };
        var uis = new List((0, 0), 11) { Size = (20, 11), ItemSize = (20, 1), Text = "Example UI:" };
        var systems = new List((0, 0), 10) { Size = (22, 10), ItemSize = (22, 1), Text = "Example Systems:" };

        editors.Edit(["Collision", "Map"]);
        editors.AlignInside((0.75f, 0.25f));
        OnTrigger(editors, Editors.Collision.Program.Run, Editors.Map.Program.Run);
        OnDisplay(editors, () =>
        {
            maps[0].SetText((0, 0), "(click on a Hub Project to start it)"
                .Constrain(maps[0].Size, alignment: Alignment.Top));
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
            "Terrain Generation", "Immediate GUI", "Animations", "Particles", "Auto Tiling"
        ]);
        systems.AlignInside((0.1f, 0.9f));
        OnTrigger(systems, DefaultGraphics.Run, Collision.Run, LineOfSightAndLights.Run, Pathfinding.Run,
            Audio.Run, TerrainGeneration.Run, ImmediateGui.Run, Animations.Run, ParticleSystems.Run,
            AutoTiling.Run);
        OnDisplay(systems);

        ui.AddRange([editors, apps, uis, systems]);
        Window.Title = "Pure - Hub";
        Examples.UserInterface.Program.Run(maps, ui);

        void RunUI(Block[] blocks)
        {
            var (_, newUI) = Examples.UserInterface.Program.Initialize();
            newUI.AddRange(blocks);
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
                maps[0].SetText((list.Position.x, list.Position.y - 1), list.Text);
                maps.SetList(list);
            };
            list.OnItemDisplay += item => maps.SetListItem(list, item);
        }
    }
}