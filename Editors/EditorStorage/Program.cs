namespace Pure.Editors.EditorStorage;

using Engine.Utilities;
using Engine.Storage;
using Tools.Tilemapper;
using EditorBase;
using Engine.Tilemap;
using Engine.UserInterface;
using Engine.Window;
using System.Diagnostics.CodeAnalysis;

public static class Program
{
    public static void Run()
    {
        editor.OnUpdateEditor = () =>
        {
            editor.MapsEditor.Flush();

            panels.Update();
            ui.Update();
        };
        editor.Run();
    }

#region Backend
    private enum Creating
    {
        Value, Tuple, List, Dictionary
    }

    private const string
        VALUE_FLAG = "Flag",
        VALUE_TEXT = "Text",
        VALUE_NUMBER = "Number",
        VALUE_SYMBOL = "Symbol";

    private static readonly Editor editor;
    private static Menu values, add;
    private static readonly Stepper promptTupleAmount;
    private static Storage storage = new();
    private static Creating creating;
    private static int currTupleAmount;
    private static readonly BlockPack ui = new(), panels = new();
    private static (int x, int y) rightClickPos;
    private static readonly InputBox promptText, promptKey, promptNumber;
    private static readonly Stepper promptSymbol;
    private static readonly List<string> selectedTypes = new();

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Storage Editor", mapSize: (mw, mh), viewSize: (mw, mh));
        CreateMenus();
        SubscribeToClicks();

        promptTupleAmount = new()
        {
            Text = "Value Amount",
            Range = (2, 9),
            Step = 1,
            Value = 2,
            Size = (18, 2)
        };
        promptTupleAmount.OnDisplay(()
            => editor.MapsUi.SetStepper(promptTupleAmount, (int)Editor.LayerMapsUi.PromptMiddle));

        promptText = new() { Size = (50, 30) };
        promptText.OnDisplay(() =>
            editor.MapsUi.SetInputBox(promptText, (int)Editor.LayerMapsUi.PromptBack));
        promptKey = new() { Size = (20, 1), IsSingleLine = true };
        promptKey.OnDisplay(() =>
            editor.MapsUi.SetInputBox(promptKey, (int)Editor.LayerMapsUi.PromptBack));
        promptNumber = new() { Size = (20, 1), SymbolGroup = SymbolGroup.Digits | SymbolGroup.Math };
        promptNumber.OnDisplay(() =>
            editor.MapsUi.SetInputBox(promptNumber, (int)Editor.LayerMapsUi.PromptBack));

        promptSymbol = new()
        {
            Range = (32, 126),
            Step = 1f,
            Size = (20, 2),
            Text = "ASCII Character",
            Value = 'A'
        };
        promptSymbol.OnDisplay(() =>
        {
            const int BACK = (int)Editor.LayerMapsUi.PromptBack;
            editor.MapsUi.SetStepper(promptSymbol, BACK);

            // edit the default tilemapper to display ascii characters
            var (x, y) = promptSymbol.Position;
            var value = (int)promptSymbol.Value;
            editor.MapsUi[BACK + 1].SetRectangle((x + 2, y + 1, 10, 1), Tile.EMPTY); // erase text
            editor.MapsUi[BACK + 1].SetTextLine((x + 2, y + 1), $"{value} '{Convert.ToChar(value)}'");
        });
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

            var (wx, wy) = editor.MousePositionWorld;
            rightClickPos = ((int)wx, (int)wy);
        });
    }

    [MemberNotNull(nameof(add), nameof(values))]
    private static void CreateMenus()
    {
        values = new(editor,
            "Value… ",
            $" {VALUE_TEXT}",
            $" {VALUE_NUMBER}",
            $" {VALUE_FLAG}",
            $" {VALUE_SYMBOL}")
        {
            Size = (8, 5),
            IsHidden = true
        };
        values.OnItemInteraction(Interaction.Trigger, btn =>
        {
            var index = values.IndexOf(btn);

            if (creating == Creating.Value)
            {
                selectedTypes.Add(btn.Text.Trim());
                AddPanel();
            }
            else if (creating == Creating.Tuple)
            {
                selectedTypes.Add(btn.Text.Trim());

                if (currTupleAmount == (int)promptTupleAmount.Value)
                {
                    AddPanel();
                    return;
                }

                currTupleAmount++;
                values[0].Text = $"Value {currTupleAmount}/{promptTupleAmount.Value}… ";
            }
        });

        add = new(editor,
            "Add… ",
            " Value",
            " Tuple",
            " List",
            " Dictionary")
        {
            Size = (11, 5),
            IsHidden = true
        };
        add.OnItemInteraction(Interaction.Trigger, btn =>
        {
            add.IsHidden = true;
            creating = (Creating)(add.IndexOf(btn) - 1);
            selectedTypes.Clear();

            if (creating == Creating.Value)
            {
                values[0].Text = "Value… ";
                values.IsHidden = false;
                values.Position = add.Position;
            }
            else if (creating == Creating.Tuple)
            {
                editor.Prompt.Text = "Add Tuple";
                editor.Prompt.Open(promptTupleAmount, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    currTupleAmount = 1;
                    values[0].Text = $"Value {currTupleAmount}/{promptTupleAmount.Value}… ";
                    values.IsHidden = false;
                    values.Position = add.Position;
                });
            }
        });
    }

    private static void AddPanel()
    {
        values.IsHidden = true;
        var itemCounts = new Dictionary<Creating, int>
        {
            { Creating.Value, 1 },
            { Creating.Tuple, (int)promptTupleAmount.Value },
        };

        var panel = new Panel(values.Position)
        {
            SizeMinimum = (5, 4),
            Text = $"{creating}",
            Position = rightClickPos
        };
        var list = new List(itemCount: itemCounts[creating])
        {
            Text = selectedTypes.ToString(",")
        };
        list.OnItemInteraction(Interaction.Trigger, item => OnValueClick(list, item));

        if (creating == Creating.Value)
        {
            panel.SizeMinimum = (5, 3);
            panel.SizeMaximum = (int.MaxValue, 3);

            list[0].Text = GetDefaultValue(0);
        }
        else if (creating == Creating.Tuple)
        {
            for (var i = 0; i < selectedTypes.Count; i++)
                list[i].Text = GetDefaultValue(i);
        }

        panel.OnInteraction(Interaction.DoubleTrigger, () =>
        {
            Input.TilemapSize = editor.MapsUi.Size;
            editor.Prompt.Text = "Edit Key";
            promptKey.Value = panel.Text;
            editor.Prompt.Open(promptKey, i =>
            {
                editor.Prompt.Close();

                if (i != 0)
                    return;

                panel.Text = promptKey.Value;
            });
            Input.TilemapSize = editor.MapsEditor.Size;
        });
        panel.OnDisplay(() => OnPanelDisplay(panel));
        list.OnDisplay(() => editor.MapsEditor.SetList(list));
        list.OnItemDisplay(item => editor.MapsEditor.SetListItem(list, item));

        panels.Add(panel);
        ui.Add(list);
    }
    private static void OnValueClick(List list, Button item)
    {
        var types = list.Text.Split(",");
        var type = types[list.IndexOf(item)];

        if (type == VALUE_FLAG)
        {
            item.Text = item.IsSelected ? "true" : "false";
            return;
        }

        list.Select(item, false);

        Input.TilemapSize = editor.MapsUi.Size;

        if (type == VALUE_TEXT)
        {
            editor.IsPromptEnterDisabled = true;
            promptText.Value = item.Text;
            promptText.SelectAll();
            editor.Prompt.Text = "Edit Text Value";
            editor.Prompt.Open(promptText, i =>
            {
                editor.Prompt.Close();
                editor.IsPromptEnterDisabled = false;
                if (i == 0)
                    item.Text = promptText.Value;
            });
        }
        else if (type == VALUE_SYMBOL)
        {
            promptSymbol.Value = Convert.ToChar(item.Text);
            editor.Prompt.Text = "Edit Symbol Value";
            editor.Prompt.Open(promptSymbol, i =>
            {
                editor.Prompt.Close();
                if (i == 0)
                    item.Text = $"{Convert.ToChar((int)promptSymbol.Value)}";
            });
        }
        else if (type == VALUE_NUMBER)
        {
            promptNumber.Value = item.Text;
            editor.Prompt.Text = "Edit Number Value";
            editor.Prompt.Open(promptNumber, i =>
            {
                editor.Prompt.Close();
                if (i == 0)
                    item.Text = $"{promptNumber.Value.Calculate()}";
            });
        }

        Input.TilemapSize = editor.MapsEditor.Size;
    }

    private static void OnPanelDisplay(Panel panel)
    {
        var i = panels.IndexOf(panel);
        var e = (List)ui[i];
        e.Position = (panel.Position.x + 1, panel.Position.y + 1);
        e.Size = (panel.Size.width - 2, panel.Size.height - 2);
        e.ItemSize = (e.Size.width, 1);

        editor.MapsEditor.SetPanel(panel);
    }

    private static string GetDefaultValue(int index)
    {
        var type = selectedTypes[index];
        var value = "";

        if (type == VALUE_FLAG)
            value = "false";
        else if (type == VALUE_TEXT)
            value = VALUE_TEXT;
        else if (type == VALUE_NUMBER)
            value = "0";
        else if (type == VALUE_SYMBOL)
            value = "A";
        return value;
    }
#endregion
}