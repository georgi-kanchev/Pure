using Pure.Engine.Storage;
using Pure.Tools.Tilemapper;

namespace Pure.Editors.EditorStorage;

using EditorBase;
using Engine.Tilemap;
using Engine.UserInterface;
using Engine.Window;
using System.Diagnostics.CodeAnalysis;

public static class Program
{
    public static void Run()
    {
        editor.Run();
    }

#region Backend
    private static readonly Editor editor;
    private static Menu values, add;
    private static readonly Stepper tupleAmount;
    private static Storage storage = new();

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Collision Editor", mapSize: (mw, mh), viewSize: (mw, mh));
        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)), new Tilemap((mw, mh)));
        editor.MapsEditor.ViewSize = (mw, mh);
        CreateMenus();
        SubscribeToClicks();

        tupleAmount = new()
        {
            Text = "Value Amount",
            Range = (2, 10),
            Step = 1,
            Value = 2,
            Size = (18, 2)
        };
        tupleAmount.OnDisplay(()
            => editor.MapsUi.SetStepper(tupleAmount, (int)Editor.LayerMapsUi.PromptMiddle));
    }

    private static void SubscribeToClicks()
    {
        Mouse.Button.Left.OnPress(() =>
        {
        });
        Mouse.Button.Left.OnRelease(() =>
        {
        });
        Mouse.Button.Right.OnPress(() =>
        {
            add.IsHidden = false;
            var (x, y) = editor.MousePositionUi;
            add.Position = ((int)x + 1, (int)y + 1);
        });
    }

    [MemberNotNull(nameof(add), nameof(values))]
    private static void CreateMenus()
    {
        values = new(editor,
            "Value… ",
            " Text",
            " Decimal",
            " Integer",
            " Flag",
            " Character")
        {
            Size = (10, 6),
            IsHidden = true
        };
        values.OnItemInteraction(Interaction.Trigger, btn =>
        {
            var index = values.IndexOf(btn);
        });

        add = new(editor,
            "Add… ",
            " Value",
            " Tuple",
            " List",
            " Dictionary")
        {
            Size = (10, 5),
            IsHidden = true
        };
        add.OnItemInteraction(Interaction.Trigger, btn =>
        {
            add.IsHidden = true;
            var index = add.IndexOf(btn);

            if (index == 1) // value
            {
                values.IsHidden = false;
                values.Position = add.Position;
            }
            else if (index == 2) // tuple
            {
                editor.Prompt.Text = "Add Tuple";
                editor.Prompt.Open(tupleAmount, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;
                });
            }
        });
    }
#endregion
}