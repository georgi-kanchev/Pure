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
            editor.IsDisabledViewZoom = IsHoveringPanel();

            editor.MapsEditor.Flush();

            panels.Update();
            data.Update();
            moves.Update();
            removes.Update();
            removeKeys.Update();
            adds.Update();
            dictKeys.Update();
        };
        editor.Run();
    }

#region Backend
    private enum Creating
    {
        Value, Tuple, List, Dictionary, TupleAdd
    }

    private const string
        VALUE_FLAG = "Flag",
        VALUE_TEXT = "Text",
        VALUE_NUMBER = "Number",
        VALUE_SYMBOL = "Symbol";

    private static readonly Editor editor;
    private static Menu values, addData;
    private static Creating creating;
    private static int currTupleAmount;
    private static readonly Storage storage = new();
    private static readonly BlockPack data = new(),
        panels = new(),
        moves = new(),
        removeKeys = new(),
        removes = new(),
        adds = new(),
        dictKeys = new();
    private static (int x, int y) rightClickPos;
    private static readonly InputBox promptText, promptKey, promptNumber;
    private static readonly Stepper promptSymbol;
    private static readonly List<string> selectedTypes = new();
    private static int lastIndexAdd;

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Storage Editor", mapSize: (mw, mh), viewSize: (mw, mh));
        CreateMenus();
        SubscribeToClicks();

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

    [MemberNotNull(nameof(addData), nameof(values))]
    private static void CreateMenus()
    {
        values = new(editor,
            "Value… ",
            $" {VALUE_TEXT}",
            $" {VALUE_NUMBER}",
            $" {VALUE_FLAG}",
            $" {VALUE_SYMBOL}")
        {
            Size = (10, 5),
            IsHidden = true
        };
        values.OnItemInteraction(Interaction.Trigger, btn =>
        {
            if (creating == Creating.Value)
            {
                selectedTypes.Add(btn.Text.Trim());
                AddPanel();
            }
            else if (creating == Creating.Tuple)
            {
                selectedTypes.Add(btn.Text.Trim());

                if (currTupleAmount == 2)
                {
                    AddPanel();
                    return;
                }

                currTupleAmount++;
                values[0].Text = $"Value {currTupleAmount}/2… ";
            }
            else if (creating is Creating.List)
            {
                selectedTypes.Add(btn.Text.Trim());
                AddPanel();
            }
            else if (creating is Creating.TupleAdd or Creating.Dictionary)
            {
                var type = btn.Text.Trim();
                selectedTypes.Add(type);

                var list = (List)data[lastIndexAdd];
                var remove = (List)removes[lastIndexAdd];
                var move = (List)moves[lastIndexAdd];

                list.Add(new Button { Text = GetDefaultValue(0) });
                remove.Add(new Button());
                move.Add(new Button());

                list.Text += (list.Text == "" ? "" : ",") + type;
                values.IsHidden = true;
            }

            if (creating != Creating.Dictionary)
                return;

            var keys = (List)dictKeys[lastIndexAdd];
            keys.Add(new Button { Text = GetUniqueText(keys, "Key", null) });
        });

        addData = new(editor,
            "Add… ",
            " Value",
            " Tuple",
            " List",
            " Dictionary")
        {
            Size = (11, 5),
            IsHidden = true
        };
        addData.OnItemInteraction(Interaction.Trigger, btn =>
        {
            addData.IsHidden = true;
            creating = (Creating)(addData.IndexOf(btn) - 1);
            selectedTypes.Clear();

            if (creating == Creating.Value)
            {
                values[0].Text = "Value… ";
                values.IsHidden = false;
                values.Position = addData.Position;
            }
            else if (creating == Creating.Tuple)
            {
                currTupleAmount = 1;
                values[0].Text = $"Value {currTupleAmount}/2… ";
                values.IsHidden = false;
                values.Position = addData.Position;
            }
            else if (creating == Creating.List)
            {
                values[0].Text = "List… ";
                values.IsHidden = false;
                values.Position = addData.Position;
            }
            else if (creating == Creating.Dictionary)
                AddPanel();
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
            addData.IsHidden = false;
            var (x, y) = editor.MousePositionUi;
            addData.Position = ((int)x + 1, (int)y + 1);

            var (wx, wy) = editor.MousePositionWorld;
            rightClickPos = ((int)wx, (int)wy);
        });
    }

    private static void AddPanel()
    {
        values.IsHidden = true;
        var itemCount = creating == Creating.Tuple ? 2 : 1;
        itemCount = creating == Creating.Dictionary ? 0 : itemCount;

        var add = new Button { Size = (1, 1), Text = creating.ToString() };
        var removeKey = new Button { Size = (1, 1) };
        var panel = new Panel(values.Position)
        {
            SizeMinimum = (8, 4),
            Size = (13, itemCount + 2),
            Text = $"{creating}",
            Position = rightClickPos
        };
        var list = new List(itemCount: itemCount)
        {
            Text = selectedTypes.ToString(",")
        };
        var remove = new List(itemCount: itemCount) { ItemSize = (1, 1) };
        var move = new List(itemCount: itemCount) { ItemSize = (1, 1) };
        var maps = editor.MapsEditor;
        var keys = new List(itemCount: 0)
        {
            IsHidden = true,
            IsDisabled = true,
            Text = "Text"
        };

        if (creating == Creating.Value)
        {
            panel.SizeMinimum = (5, 3);
            list[0].Text = GetDefaultValue(0);
        }
        else if (creating == Creating.Tuple)
        {
            for (var i = 0; i < selectedTypes.Count; i++)
                list[i].Text = GetDefaultValue(i);
        }
        else if (creating == Creating.List)
        {
            panel.Text = $"{selectedTypes[0]} {creating}";
            list[0].Text = GetDefaultValue(0);
        }
        else if (creating == Creating.Dictionary)
        {
            panel.Text = $"{creating}";
            panel.SizeMinimum = (11, 4);
            panel.Size = (18, panel.Size.height);
            keys.IsHidden = false;
            keys.IsDisabled = false;
        }

        keys.OnItemDisplay(item =>
        {
            maps[1].SetTextLine(item.Position, item.Text,
                item.GetInteractionColor(Color.Orange), item.Size.width);
        });
        keys.OnItemInteraction(Interaction.Trigger, item => OnValueClick(keys, item, true));

        list.OnDisplay(() => maps.SetList(list));
        list.OnItemDisplay(item =>
        {
            var color = Color.Gray;
            var types = list.Text.Split(",");
            var type = types[list.IndexOf(item)];

            if (type == VALUE_TEXT)
                color = Color.Orange;
            else if (type == VALUE_FLAG)
                color = item.IsSelected ? Color.Green : Color.Red;
            else if (type == VALUE_NUMBER)
                color = Color.Azure;
            else if (type == VALUE_SYMBOL)
                color = Color.Magenta;

            var interactionColor = item.GetInteractionColor(color);
            maps[1].SetTextLine(item.Position, item.Text, interactionColor, item.Size.width);
        });
        list.OnItemInteraction(Interaction.Trigger, item => OnValueClick(list, item));

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
            Input.TilemapSize = maps.Size;
        });
        panel.OnDisplay(() => OnPanelDisplay(panel));

        add.OnDisplay(() => maps.SetButtonIcon(add, new(Tile.MATH_PLUS, Color.Green), 1));
        add.OnInteraction(Interaction.Trigger, () => OnAddClick(add));
        removeKey.OnDisplay(() => maps.SetButtonIcon(removeKey, new(Tile.ICON_TRASH, Color.Red), 1));
        removeKey.OnInteraction(Interaction.Trigger, () => OnRemoveKeyClick(removeKey));

        remove.OnItemDisplay(item => maps.SetButtonIcon(item, new(Tile.ICON_X, Color.Red), 1));
        remove.OnItemInteraction(Interaction.Trigger, item => OnRemoveClick(remove, item));
        move.OnItemDisplay(item => maps.SetButtonIcon(item, new(Tile.ARROW_TAILLESS, Color.Gray, 3), 1));
        move.OnItemInteraction(Interaction.Trigger, item => OnMoveClick(move, item));

        panels.Add(panel);
        data.Add(list);
        removeKeys.Add(removeKey);
        removes.Add(remove);
        moves.Add(move);
        adds.Add(add);
        dictKeys.Add(keys);
    }

    private static void OnPanelDisplay(Panel panel)
    {
        var i = panels.IndexOf(panel);
        var list = (List)data[i];
        var removeKey = (Button)removeKeys[i];
        var add = (Button)adds[i];
        var remove = (List)removes[i];
        var move = (List)moves[i];
        var keys = (List)dictKeys[i];
        var (x, y) = panel.Position;
        var (w, h) = panel.Size;

        panel.SizeMaximum = (int.MaxValue, list.Count + 2);

        if (add.Text == $"{Creating.Value}")
        {
            panel.SizeMaximum = (int.MaxValue, 3);
            remove.IsHidden = true;
            add.IsHidden = true;
            move.IsHidden = true;
        }
        else if (add.Text == $"{Creating.Tuple}")
        {
            remove.IsHidden = list.Count < 3;
            add.IsHidden = list.Count > 8;
            move.IsHidden = false;
        }
        else if (add.Text == $"{Creating.Dictionary}")
        {
            remove.IsHidden = list.Count == 0;
            add.IsHidden = false;
            move.IsHidden = list.Count < 2;
        }
        else if (add.Text == $"{Creating.List}")
        {
            remove.IsHidden = list.Count < 2;
            add.IsHidden = false;
            move.IsHidden = list.Count < 2;
        }

        var offX = move.IsHidden ? 1 : 3;
        var offW = -2;
        offW -= move.IsHidden ? 0 : 2;
        offW -= remove.IsHidden ? 0 : 2;

        remove.IsDisabled = remove.IsHidden;
        add.IsDisabled = add.IsHidden;
        move.IsDisabled = move.IsHidden;

        if (add.Text == $"{Creating.Dictionary}")
        {
            var half = (w + offW) / 2;
            var oddW = w % 2 == 0 ? 0 : 1;
            list.Position = (x + offX + half + oddW, y + 1);
            list.Size = (half, h - 2);

            keys.Position = (x + offX, y + 1);
            keys.Size = (half - 1 + oddW, h - 2);
        }
        else
        {
            list.Position = (x + offX, y + 1);
            list.Size = (w + offW, h - 2);
        }

        list.ItemSize = (list.Size.width, 1);

        move.Scroll.Slider.Progress = list.Scroll.Slider.Progress;
        remove.Scroll.Slider.Progress = list.Scroll.Slider.Progress;
        keys.Scroll.Slider.Progress = list.Scroll.Slider.Progress;

        move.Position = (x + 1, y + 1);
        move.Size = (2, list.Size.height);
        remove.Position = (x + w - 3, y + 1);
        remove.Size = (2, list.Size.height);

        add.Position = (x + 1, y + h - 1);
        removeKey.Position = (x + w - 2, y + h - 1);

        editor.MapsEditor.SetPanel(panel);
    }

    private static void OnValueClick(List list, Button item, bool isKey = false)
    {
        var types = list.Text.Split(",");
        var type = isKey ? types[0] : types[list.IndexOf(item)];

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
            editor.Prompt.Text = "Edit Text " + (isKey ? "Key" : "Value");
            editor.Prompt.Open(promptText, i =>
            {
                editor.Prompt.Close();
                editor.IsPromptEnterDisabled = false;

                if (i != 0)
                    return;

                if (isKey == false)
                {
                    item.Text = promptText.Value;
                    return;
                }

                if (promptText.Value == item.Text)
                    return;

                var unique = GetUniqueText(list, promptText.Value, item);

                if (promptText.Value != unique)
                    editor.PromptMessage(
                        $"The provided key '{promptText.Value}' already exists.{Environment.NewLine}" +
                        $"It was changed to '{unique}'.");

                item.Text = unique;
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
            promptNumber.SelectAll();
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
    private static void OnRemoveClick(List list, Button item)
    {
        var index = removes.IndexOf(list);
        var itemIndex = list.IndexOf(item);
        var valueList = (List)data[index];
        var moveList = (List)moves[index];
        var removeList = (List)removes[index];
        var keysList = (List)dictKeys[index];
        var types = valueList.Text.Split(",").ToList();

        types.RemoveAt(itemIndex);
        valueList.Remove(valueList[itemIndex]);
        moveList.Remove(moveList[itemIndex]);
        removeList.Remove(removeList[itemIndex]);

        if (keysList.Count > 0)
            keysList.Remove(keysList[itemIndex]);

        valueList.Text = types.ToString(",");
    }
    private static void OnMoveClick(List list, Button item)
    {
        var index = moves.IndexOf(list);
        var itemIndex = list.IndexOf(item);
        var valueList = (List)data[index];
        var keysList = (List)dictKeys[index];
        var types = valueList.Text.Split(",").ToList();

        types.Shift(-1, itemIndex);
        valueList.Shift(-1, valueList[itemIndex]);

        if (keysList.Count > 0)
            keysList.Shift(-1, keysList[itemIndex]);

        valueList.Text = types.ToString(",");
    }
    private static void OnRemoveKeyClick(Button button)
    {
        var index = removeKeys.IndexOf(button);

        editor.PromptYesNo($"Delete '{panels[index].Text}'?", () =>
        {
            data.Remove(data[index]);
            panels.Remove(panels[index]);
            removes.Remove(removes[index]);
            moves.Remove(moves[index]);
            removeKeys.Remove(removeKeys[index]);
            adds.Remove(adds[index]);
            dictKeys.Remove(dictKeys[index]);
        });
    }
    private static void OnAddClick(Button button)
    {
        var index = adds.IndexOf(button);
        var list = (List)data[index];
        var type = adds[index].Text;

        if (type == $"{Creating.Tuple}")
        {
            values[0].Text = $"Value {list.Count + 1}/{list.Count + 1}… ";
            creating = Creating.TupleAdd;
            lastIndexAdd = index;
        }
        else if (type == $"{Creating.List}")
        {
            var valueType = list.Text.Split(",")[0];
            selectedTypes.Clear();
            selectedTypes.Add(valueType);

            var remove = (List)removes[index];
            var move = (List)moves[index];

            list.Add(new Button { Text = GetDefaultValue(0) });
            remove.Add(new Button());
            move.Add(new Button());

            list.Text += $",{valueType}";
        }
        else if (type == $"{Creating.Dictionary}")
        {
            selectedTypes.Clear();
            values[0].Text = "Value… ";
            lastIndexAdd = index;
        }

        if (type != $"{Creating.Tuple}" && type != $"{Creating.Dictionary}")
            return;

        var (x, y) = editor.MousePositionUi;
        values.Position = ((int)x + 1, (int)y + 2);
        values.IsHidden = false;
        list.Scroll.Slider.Progress = 1f;
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
    private static bool IsHoveringPanel()
    {
        for (var i = 0; i < panels.Count; i++)
            if (panels[i].IsHovered)
                return true;

        return false;
    }
    private static string GetUniqueText(List list, string text, Button? ignoreItem = null)
    {
        var texts = new List<string>();
        for (var i = 0; i < list.Count; i++)
            if (ignoreItem != list[i])
                texts.Add(list[i].Text);

        return texts.ToArray().EnsureUnique(text);
    }
#endregion
}