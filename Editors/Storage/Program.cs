using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

using Pure.Engine.Utilities;
using Pure.Tools.Tilemap;
using Pure.Editors.Base;
using Pure.Engine.Tilemap;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;

using _Storage = Pure.Engine.Storage.Storage;

namespace Pure.Editors.Storage;

public static class Program
{
    public static void Run()
    {
        Window.SetIconFromTile(editor.LayerUi, (Tile.ICON_LOCK, Color.White), (Tile.FULL, Color.Brown));

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
    private enum DataType { Value, Tuple, List, Dictionary, TupleAdd }

    private const string VALUE_FLAG = "Flag", VALUE_TEXT = "Text", VALUE_NUMBER = "Number",
        VALUE_SYMBOL = "Symbol";

    private static readonly Editor editor;
    private static readonly Menu values, main;
    private static DataType creating;
    private static int currValueSelection;
    private static readonly BlockPack data = new(), panels = new(), moves = new(), removeKeys = new(),
        removes = new(), adds = new(), dictKeys = new();
    private static (int x, int y) rightClickPos;
    private static readonly InputBox promptText, promptNumber;
    private static readonly Stepper promptSymbol;
    private static readonly List<string> selectedTypes = new();
    private static int lastIndexAdd;
    private static readonly List<string> strings = new();
    private const string STR_PLACEHOLDER = "—";

    static Program()
    {
        editor = new("Pure - Storage Editor");

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
        values.OnItemInteraction(Interaction.Trigger, OnMenuValuesClick);

        main = new(editor,
            "Add… ",
            " Value",
            " Tuple",
            " List",
            " Dictionary",
            "Storage… ",
            " New",
            " Save",
            " Load",
            " Copy",
            " Paste")
        {
            Size = (11, 11),
            IsHidden = true
        };
        main.OnItemInteraction(Interaction.Trigger, OnMenuMainClick);

        SubscribeToClicks();

        promptText = new() { Size = (50, 30) };
        promptText.OnDisplay(() =>
            editor.MapsUi.SetInputBox(promptText, (int)Editor.LayerMapsUi.PromptBack));
        promptNumber = new() { Size = (20, 1), SymbolGroup = SymbolGroup.Decimals | SymbolGroup.Math };
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
            editor.MapsUi.Tilemaps[BACK + 1].SetArea((x + 2, y + 1, 10, 1), null, Tile.EMPTY); // erase text
            editor.MapsUi.Tilemaps[BACK + 1].SetText((x + 2, y + 1), $"{value} '{Convert.ToChar(value)}'");
        });
    }

    private static void SubscribeToClicks()
    {
        Mouse.Button.Left.OnPress(() => { });
        Mouse.Button.Left.OnRelease(() => { });
        Mouse.Button.Right.OnPress(() =>
        {
            main.IsHidden = false;
            var (x, y) = editor.MousePositionUi;
            main.Position = ((int)x + 1, (int)y + 1);

            var (wx, wy) = editor.MousePositionWorld;
            rightClickPos = ((int)wx, (int)wy);
        });
    }

    private static void AddPanel(bool emptyData = false)
    {
        values.IsHidden = true;

        var itemCount = creating == DataType.Tuple ? 2 : 1;
        var add = new Button { Size = (1, 1), Text = creating.ToString() };
        var removeKey = new Button { Size = (1, 1) };
        var allKeys = GetKeys();
        var panel = new Panel(values.Position)
        {
            SizeMinimum = (8, 4),
            Size = (13, itemCount + 2),
            Text = $"{creating}".EnsureUnique(allKeys),
            Position = rightClickPos
        };
        var list = new List((0, 0), itemCount)
        {
            Text = selectedTypes.ToString(",")
        };
        var remove = new List((0, 0), itemCount) { ItemSize = (1, 1) };
        var move = new List((0, 0), itemCount) { ItemSize = (1, 1) };
        var maps = editor.MapsEditor;
        var keys = new List((0, 0), 1)
        {
            IsHidden = true,
            IsDisabled = true,
            Text = "Text"
        };

        if (creating == DataType.Value)
        {
            panel.SizeMinimum = (5, 3);

            if (emptyData == false)
                list.Items[0].Text = GetDefaultValue(0);
        }
        else if (creating == DataType.Tuple)
        {
            if (emptyData == false)
                for (var i = 0; i < list.Items.Count; i++)
                    list.Items[i].Text = GetDefaultValue(i);
        }
        else if (creating is DataType.List or DataType.Dictionary)
            if (emptyData == false)
                list.Items[0].Text = GetDefaultValue(0);

        if (creating == DataType.Dictionary)
        {
            panel.Size = (19, 4);
            keys.Items[0].Text = GetUniqueText(keys, "Key");
        }

        keys.OnItemDisplay(item =>
        {
            maps.Tilemaps[1].SetText(item.Position, item.Text.Shorten(item.Size.width),
                item.GetInteractionColor(Color.Orange));
        });
        keys.OnItemInteraction(Interaction.Trigger, item => OnValueClick(keys, item, true));

        list.OnDisplay(() => maps.SetList(list));
        list.OnItemDisplay(item =>
        {
            var color = Color.Gray;
            var types = list.Text.Split(",");
            var type = types[list.Items.IndexOf(item)];

            if (type == VALUE_TEXT)
                color = Color.Orange;
            else if (type == VALUE_FLAG)
                color = item.IsSelected ? Color.Green : Color.Red;
            else if (type == VALUE_NUMBER)
                color = Color.Azure;
            else if (type == VALUE_SYMBOL)
                color = Color.Magenta;

            var interactionColor = item.GetInteractionColor(color);
            maps.Tilemaps[1].SetText(item.Position, item.Text.Shorten(item.Size.width), interactionColor);
        });
        list.OnItemInteraction(Interaction.Trigger, item => OnValueClick(list, item));

        panel.OnInteraction(Interaction.DoubleTrigger, () =>
        {
            Input.TilemapSize = editor.MapsUi.Size;
            editor.Prompt.Text = "Edit Key of " + add.Text;
            editor.PromptInput.Value = panel.Text;
            editor.PromptInput.SelectAll();
            editor.Prompt.Open(editor.PromptInput, onButtonTrigger: i =>
            {
                if (i != 0 || editor.PromptInput.Value == panel.Text)
                    return;

                var unique = editor.PromptInput.Value.EnsureUnique(allKeys);

                if (editor.PromptInput.Value != unique)
                    editor.PromptMessage(
                        $"The provided key '{editor.PromptInput.Value}' already exists.\n" +
                        $"It was changed to '{unique}'.");

                panel.Text = unique;
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

        panels.Blocks.Add(panel);
        data.Blocks.Add(list);
        removeKeys.Blocks.Add(removeKey);
        removes.Blocks.Add(remove);
        moves.Blocks.Add(move);
        adds.Blocks.Add(add);
        dictKeys.Blocks.Add(keys);
    }
    private static void OnPanelDisplay(Panel panel)
    {
        var i = panels.Blocks.IndexOf(panel);
        var list = (List)data.Blocks[i];
        var removeKey = (Button)removeKeys.Blocks[i];
        var add = (Button)adds.Blocks[i];
        var remove = (List)removes.Blocks[i];
        var move = (List)moves.Blocks[i];
        var keys = (List)dictKeys.Blocks[i];
        var (x, y) = panel.Position;
        var (w, h) = panel.Size;

        panel.SizeMaximum = (int.MaxValue, list.Items.Count + 2);

        if (add.Text == nameof(DataType.Value))
        {
            panel.SizeMaximum = (int.MaxValue, 3);
            remove.IsHidden = true;
            add.IsHidden = true;
            move.IsHidden = true;
        }
        else if (add.Text == nameof(DataType.Tuple))
        {
            remove.IsHidden = list.Items.Count < 3;
            add.IsHidden = list.Items.Count > 7;
            move.IsHidden = false;
        }
        else
        {
            remove.IsHidden = list.Items.Count < 2;
            add.IsHidden = false;
            move.IsHidden = list.Items.Count < 2;
        }

        var offX = move.IsHidden ? 1 : 3;
        var offW = -2;
        offW -= move.IsHidden ? 0 : 2;
        offW -= remove.IsHidden ? 0 : 2;

        keys.IsHidden = add.Text != nameof(DataType.Dictionary);

        remove.IsDisabled = remove.IsHidden;
        add.IsDisabled = add.IsHidden;
        move.IsDisabled = move.IsHidden;
        keys.IsDisabled = keys.IsHidden;

        if (add.Text == nameof(DataType.Dictionary))
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

    private static void OnMenuMainClick(Button btn)
    {
        main.IsHidden = true;
        var index = main.Items.IndexOf(btn);

        if (index < 5)
        {
            creating = (DataType)(main.Items.IndexOf(btn) - 1);
            selectedTypes.Clear();

            values.IsHidden = false;
            values.Position = main.Position;

            if (creating is DataType.Tuple)
            {
                currValueSelection = 1;
                values.Items[0].Text = $"Value {currValueSelection}/2… ";
            }
            else
                values.Items[0].Text = $"Value… ";

            return;
        }

        if (index == 6)
            editor.PromptConfirm(ResetAll);
        else if (index == 7)
            editor.PromptFileSave(Save());
        else if (index == 8)
            editor.PromptFileLoad(Load);
        else if (index == 9)
            Window.Clipboard = Convert.ToBase64String(Save());
        else if (index == 10)
            editor.PromptBase64(() => Load(Convert.FromBase64String(editor.PromptInput.Value)));
    }
    private static void OnMenuValuesClick(Button btn)
    {
        if (creating == DataType.Value)
        {
            selectedTypes.Add(btn.Text.Trim());
            AddPanel();
        }
        else if (creating == DataType.Tuple)
        {
            selectedTypes.Add(btn.Text.Trim());

            if (currValueSelection == 2)
            {
                AddPanel();
                return;
            }

            currValueSelection++;
            values.Items[0].Text = $"Value {currValueSelection}/2… ";
        }
        else if (creating is DataType.List or DataType.Dictionary)
        {
            selectedTypes.Add(btn.Text.Trim());
            AddPanel();
        }
        else if (creating is DataType.TupleAdd)
        {
            var type = btn.Text.Trim();
            selectedTypes.Add(type);

            var list = (List)data.Blocks[lastIndexAdd];
            var remove = (List)removes.Blocks[lastIndexAdd];
            var move = (List)moves.Blocks[lastIndexAdd];

            list.Items.Add(new() { Text = GetDefaultValue(0) });
            remove.Items.Add(new());
            move.Items.Add(new());

            list.Text += (list.Text == string.Empty ? string.Empty : ",") + type;
            values.IsHidden = true;
        }

        if (creating == DataType.Tuple)
            values.Items[0].Text = $"Value {currValueSelection}/2… ";
    }

    private static void OnValueClick(List list, Button item, bool isKey = false)
    {
        var types = list.Text.Split(",");
        var type = isKey ? types[0] : types[list.Items.IndexOf(item)];

        if (type == VALUE_FLAG)
        {
            item.Text = item.IsSelected ? "true" : "false";
            return;
        }

        list.Select(item, false);

        Input.TilemapSize = editor.MapsUi.Size;

        if (type == VALUE_TEXT)
        {
            promptText.Value = item.Text;
            promptText.SelectAll();
            editor.Prompt.Text = "Edit Text " + (isKey ? "Key" : "Value");
            editor.Prompt.Open(promptText, btnYes: -1, onButtonTrigger: i =>
            {
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
                        $"The provided key '{promptText.Value}' already exists.\n" +
                        $"It was changed to '{unique}'.");

                item.Text = unique;
            });
        }
        else if (type == VALUE_SYMBOL)
        {
            promptSymbol.Value = Convert.ToChar(item.Text);
            editor.Prompt.Text = "Edit Symbol Value";
            editor.Prompt.Open(promptSymbol, onButtonTrigger: i =>
            {
                if (i == 0)
                    item.Text = $"{Convert.ToChar((int)promptSymbol.Value)}";
            });
        }
        else if (type == VALUE_NUMBER)
        {
            promptNumber.Value = item.Text;
            promptNumber.SelectAll();
            editor.Prompt.Text = "Edit Number Value";
            editor.Prompt.Open(promptNumber, onButtonTrigger: i =>
            {
                if (i == 0)
                    item.Text = $"{promptNumber.Value.Calculate()}";
            });
        }

        Input.TilemapSize = editor.MapsEditor.Size;
    }
    private static void OnRemoveClick(List list, Button item)
    {
        var index = removes.Blocks.IndexOf(list);
        var itemIndex = list.Items.IndexOf(item);
        var valueList = (List)data.Blocks[index];
        var moveList = (List)moves.Blocks[index];
        var removeList = (List)removes.Blocks[index];
        var keysList = (List)dictKeys.Blocks[index];
        var types = valueList.Text.Split(",").ToList();

        types.RemoveAt(itemIndex);
        valueList.Items.Remove(valueList.Items[itemIndex]);
        moveList.Items.Remove(moveList.Items[itemIndex]);
        removeList.Items.Remove(removeList.Items[itemIndex]);

        if (keysList.Items.Count > 0)
            keysList.Items.Remove(keysList.Items[itemIndex]);

        valueList.Text = types.ToString(",");
    }
    private static void OnMoveClick(List list, Button item)
    {
        var index = moves.Blocks.IndexOf(list);
        var itemIndex = list.Items.IndexOf(item);
        var valueList = (List)data.Blocks[index];
        var keysList = (List)dictKeys.Blocks[index];
        var types = valueList.Text.Split(",").ToList();
        var add = (Button)adds.Blocks[index];

        types.Shift(-1, itemIndex);
        valueList.Items.Shift(-1, valueList.Items[itemIndex]);

        if (add.Text == nameof(DataType.Dictionary) && keysList.Items.Count > 0)
            keysList.Items.Shift(-1, keysList.Items[itemIndex]);

        valueList.Text = types.ToString(",");
    }
    private static void OnRemoveKeyClick(Button button)
    {
        var index = removeKeys.Blocks.IndexOf(button);

        editor.PromptYesNo($"Delete '{panels.Blocks[index].Text}'?", () =>
        {
            data.Blocks.Remove(data.Blocks[index]);
            panels.Blocks.Remove(panels.Blocks[index]);
            removes.Blocks.Remove(removes.Blocks[index]);
            moves.Blocks.Remove(moves.Blocks[index]);
            removeKeys.Blocks.Remove(removeKeys.Blocks[index]);
            adds.Blocks.Remove(adds.Blocks[index]);
            dictKeys.Blocks.Remove(dictKeys.Blocks[index]);
        });
    }
    private static void OnAddClick(Button button)
    {
        var index = adds.Blocks.IndexOf(button);
        var list = (List)data.Blocks[index];
        var type = adds.Blocks[index].Text;

        if (type == nameof(DataType.Tuple))
        {
            selectedTypes.Clear();
            values.Items[0].Text = $"Value {list.Items.Count + 1}/{list.Items.Count + 1}… ";
            creating = DataType.TupleAdd;
            lastIndexAdd = index;

            var (x, y) = editor.MousePositionUi;
            values.Position = ((int)x + 1, (int)y + 2);
            values.IsHidden = false;
        }
        else if (type is nameof(DataType.List) or nameof(DataType.Dictionary))
        {
            var valueType = list.Text.Split(",")[0];
            selectedTypes.Clear();
            selectedTypes.Add(valueType);

            var remove = (List)removes.Blocks[index];
            var move = (List)moves.Blocks[index];

            list.Items.Add(new() { Text = GetDefaultValue(0) });
            remove.Items.Add(new());
            move.Items.Add(new());

            list.Text += $",{valueType}";
        }

        list.Scroll.Slider.Progress = 1f;

        if (type != nameof(DataType.Dictionary))
            return;

        var keys = (List)dictKeys.Blocks[index];
        keys.Items.Add(new() { Text = GetUniqueText(keys, "Key") });
    }

    private static byte[] Save()
    {
        try
        {
            var result = new List<byte>();
            var storage = new _Storage();
            for (var i = 0; i < data.Blocks.Count; i++)
            {
                var list = (List)data.Blocks[i];
                var types = list.Text.Split(",");
                var dataType = adds.Blocks[i].Text;
                var key = panels.Blocks[i].Text;

                if (dataType == nameof(DataType.Value))
                    storage.Set(key, ObjectFromText(storage, types[0], list.Items[0].Text, out _));
                else if (dataType == nameof(DataType.Tuple))
                    storage.Set(key, CreateTuple(storage, types, list));
                else if (dataType == nameof(DataType.List))
                    storage.Set(key, CreateArray(storage, types[0], list));
                else if (dataType == nameof(DataType.Dictionary))
                    storage.Set(key, CreateDictionary(storage, types[0], (List)dictKeys.Blocks[i], list));
            }

            var bytes = Decompress(storage.ToBytes());
            result.AddRange(bytes);

            // hijack the end of the file to save some extra info
            // should be ignored by the engine but not by the editor

            for (var i = 0; i < data.Blocks.Count; i++)
            {
                var panel = panels.Blocks[i];
                result.AddRange(BitConverter.GetBytes(panel.Position.x));
                result.AddRange(BitConverter.GetBytes(panel.Position.y));
                result.AddRange(BitConverter.GetBytes(panel.Size.width));
                result.AddRange(BitConverter.GetBytes(panel.Size.height));
                result.AddRange(BitConverter.GetBytes(DataTypeToInt(adds.Blocks[i].Text)));
                PutString(result, data.Blocks[i].Text);
            }

            return Compress(result.ToArray());
        }
        catch (Exception)
        {
            editor.PromptMessage("Saving failed!");
            return [];
        }
    }
    private static void Load(byte[] bytes)
    {
        try
        {
            ResetAll();

            var storage = new _Storage(bytes);
            var decompressed = Decompress(bytes);
            var measure = Decompress(storage.ToBytes()).Length;
            var predict = decompressed.Length == measure;
            var hijackedBytes = decompressed[measure..];
            var offset = 0;
            var keys = storage.Keys;
            for (var i = 0; i < storage.Count; i++)
            {
                var panelX = predict ? 0 : GrabInt();
                var panelY = predict ? 0 : GrabInt();
                var panelW = predict ? 16 : GrabInt();
                var panelH = predict ? 8 : GrabInt();
                creating = predict ? default : (DataType)GrabInt();
                var types = predict ? string.Empty : GrabString(hijackedBytes, ref offset);
                selectedTypes.Clear();

                if (predict == false)
                    selectedTypes.AddRange(types.Split(","));

                AddPanel(true);

                var list = (List)data.Blocks[i];
                list.Text = types;
                panels.Blocks[i].Position = (panelX, panelY);
                panels.Blocks[i].Size = (panelW, panelH);
                panels.Blocks[i].Text = keys[i];

                LoadData(storage, creating, i, predict);

                OnPanelDisplay((Panel)panels.Blocks[i]);

                if (predict == false || i == 0)
                    continue;

                var prevPos = panels.Blocks[i - 1].Position;
                var prevSz = panels.Blocks[i - 1].Size;
                panels.Blocks[i].Position = (0, prevPos.y + prevSz.height);

                if (prevPos.y + prevSz.height > editor.MapsEditor.Size.height)
                    panels.Blocks[i].Position = (prevPos.x + prevSz.width, 0);
            }

            if (predict)
                editor.PromptMessage($"Loading storages that were saved by the\n" +
                                     $"engine rather than the editor is not\n" +
                                     $"recommended. This is because the types\n" +
                                     $"and values might get predicted\n" +
                                     $"incorrectly. Keep in mind that the\n" +
                                     $"editor supports fewer types than the\n" +
                                     $"engine. These types are:\n\n" +
                                     $"A. Primitives and Strings     \n" +
                                     $"B. Tuples of A                \n" +
                                     $"C. Arrays of A                \n" +
                                     $"D. Lists of A                 \n" +
                                     $"E. Dictionaries of <String, A>");

            int GrabInt()
            {
                return BitConverter.ToInt32(GetBytesFrom(hijackedBytes, 4, ref offset));
            }
        }
        catch (Exception)
        {
            editor.PromptMessage("Loading failed!");
        }
    }
    private static void ResetAll()
    {
        data.Blocks.Clear();
        panels.Blocks.Clear();
        moves.Blocks.Clear();
        removeKeys.Blocks.Clear();
        removes.Blocks.Clear();
        adds.Blocks.Clear();
        dictKeys.Blocks.Clear();
        strings.Clear();
    }

    private static string GetDefaultValue(int index)
    {
        var type = selectedTypes[index];
        var value = string.Empty;

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
        foreach (var block in panels.Blocks)
            if (block.IsHovered)
                return true;

        return false;
    }
    private static string GetUniqueText(List list, string text, Button? ignoreItem = null)
    {
        var texts = new List<string>();
        foreach (var item in list.Items)
            if (ignoreItem != item)
                texts.Add(item.Text);

        return texts.ToArray().EnsureUnique(text);
    }

    private static object? ObjectFromText(_Storage storage, string type, string text, out Type casted)
    {
        if (type == VALUE_FLAG)
        {
            casted = typeof(bool);
            return storage.ObjectFromText<bool>(text);
        }
        else if (type == VALUE_NUMBER)
        {
            casted = typeof(float);
            return storage.ObjectFromText<float>(text);
        }
        else if (type == VALUE_SYMBOL)
        {
            casted = typeof(char);
            return storage.ObjectFromText<char>(text);
        }

        casted = typeof(string);
        return storage.ObjectFromText<string>(text);
    }
    private static object? CreateTuple(_Storage storage, string[] strTypes, List list)
    {
        // lord, have mercy...

        var t = new Type[strTypes.Length];
        var objs = new object?[strTypes.Length];
        var resultType = default(Type);
        var tpl = new[]
        {
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>)
        };

        for (var i = 0; i < t.Length; i++)
        {
            objs[i] = Value(i, out var type);
            t[i] = type;
        }

        if (t.Length == 2)
            resultType = tpl[0].MakeGenericType(t[0], t[1]);
        else if (t.Length == 3)
            resultType = tpl[1].MakeGenericType(t[0], t[1], t[2]);
        else if (t.Length == 4)
            resultType = tpl[2].MakeGenericType(t[0], t[1], t[2], t[3]);
        else if (t.Length == 5)
            resultType = tpl[3].MakeGenericType(t[0], t[1], t[2], t[3], t[4]);
        else if (t.Length == 6)
            resultType = tpl[4].MakeGenericType(t[0], t[1], t[2], t[3], t[4], t[5]);
        else if (t.Length == 7)
            resultType = tpl[5].MakeGenericType(t[0], t[1], t[2], t[3], t[4], t[5], t[6]);
        else if (t.Length == 8)
            resultType = tpl[6].MakeGenericType(t[0], t[1], t[2], t[3], t[4], t[5], t[6], t[7]);

        if (resultType == null)
            return default;

        var tuple = Activator.CreateInstance(resultType);
        for (var i = 0; i < objs.Length; i++)
            resultType.GetField($"Item{i + 1}")?.SetValue(tuple, objs[i]);

        return tuple;

        object? Value(int i, out Type type)
        {
            return ObjectFromText(storage, strTypes[i], list.Items[i].Text, out type);
        }
    }
    private static Array CreateArray(_Storage storage, string type, List list)
    {
        var instance = Array.CreateInstance(GetType(type), list.Items.Count);
        for (var i = 0; i < list.Items.Count; i++)
            instance.SetValue(ObjectFromText(storage, type, list.Items[i].Text, out _), i);

        return instance;
    }
    private static object? CreateDictionary(_Storage storage, string type, List keys, List list)
    {
        var resultType = typeof(Dictionary<,>).MakeGenericType(typeof(string), GetType(type));
        var instance = Activator.CreateInstance(resultType);

        for (var i = 0; i < list.Items.Count; i++)
        {
            var value = ObjectFromText(storage, type, list.Items[i].Text, out _);
            resultType.GetMethod("Add")?.Invoke(instance, [keys.Items[i].Text, value]);
        }

        return instance;
    }

    private static void LoadData(_Storage storage, DataType type, int index, bool predict)
    {
        var list = (List)data.Blocks[index];
        var keys = (List)dictKeys.Blocks[index];
        var remove = (List)removes.Blocks[index];
        var move = (List)moves.Blocks[index];
        var types = list.Text.Split(",");
        var key = panels.Blocks[index].Text;
        var value = storage.GetText(key);

        if (value.IsSurroundedBy(storage.Dividers.text))
            value = value[1..^1];

        value = AddPlaceholders(value);
        var divider = string.Empty;

        if (predict)
        {
            type = PredictDataType(storage, value, out var separator);
            types = PredictValueTypes(storage, value.Split(separator));
            adds.Blocks[index].Text = $"{type}";

            if (type != DataType.Dictionary)
                list.Text = types.ToString(",");
            else
            {
                var valueTypes = new string[types.Length / 2];
                for (var i = 0; i < valueTypes.Length; i++)
                    valueTypes[i] = types[i * 2 + 1];

                types = valueTypes;
                list.Text = valueTypes.ToString(",");
            }
        }

        var isDict = type == DataType.Dictionary;
        if (type == DataType.Value)
        {
            value = FilterPlaceholders(storage, value).Replace(storage.Dividers.text, string.Empty);
            LoadValue(list.Items[0], types[0], list, value);
            return;
        }
        else if (type == DataType.Tuple)
            divider = storage.Dividers.tuple;
        else if (type == DataType.List)
            divider = storage.DividersCollection.oneD;
        else if (isDict)
            divider = storage.DividersCollection.dictionary;

        var multipleValues = value.Split(divider);
        for (var i = 0; i < multipleValues.Length; i++)
        {
            var v = FilterPlaceholders(storage, multipleValues[i]);
            v = v.Replace(storage.Dividers.text, string.Empty);

            if ((i == list.Items.Count && isDict == false) ||
                (i / 2 == list.Items.Count && isDict))
            {
                list.Items.Add(new());
                remove.Items.Add(new());
                move.Items.Add(new());

                if (isDict)
                    keys.Items.Add(new());
            }

            if (isDict)
            {
                if (i % 2 == 0)
                    LoadValue(keys.Items[i / 2], types[0], keys, v);
                else
                    LoadValue(list.Items[i / 2], types[0], list, v);
                continue;
            }

            LoadValue(list.Items[i], types[i], list, v);
        }
    }
    private static void LoadValue(Button btn, string type, List list, string value)
    {
        btn.Text = value;

        if (type != VALUE_FLAG)
            return;

        btn.Text = btn.Text.ToLower();
        list.Select(btn, btn.Text == "true");
    }

    private static Type GetType(string type)
    {
        if (type == VALUE_FLAG)
            return typeof(bool);
        else if (type == VALUE_NUMBER)
            return typeof(float);
        else if (type == VALUE_SYMBOL)
            return typeof(char);

        return typeof(string);
    }

    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
    private static void PutString(List<byte> intoBytes, string value)
    {
        var b = Encoding.UTF8.GetBytes(value);
        intoBytes.AddRange(BitConverter.GetBytes(b.Length));
        intoBytes.AddRange(b);
    }
    private static string GrabString(byte[] fromBytes, ref int offset)
    {
        var textBytesLength = BitConverter.ToInt32(GetBytesFrom(fromBytes, 4, ref offset));
        var bText = GetBytesFrom(fromBytes, textBytesLength, ref offset);
        return Encoding.UTF8.GetString(bText);
    }

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
            stream.CopyTo(output);

        return output.ToArray();
    }

    private static int DataTypeToInt(string dataType)
    {
        if (dataType == nameof(DataType.Tuple))
            return (int)DataType.Tuple;
        else if (dataType == nameof(DataType.List))
            return (int)DataType.List;
        else if (dataType == nameof(DataType.Dictionary))
            return (int)DataType.Dictionary;

        return (int)DataType.Value;
    }
    private static string[] GetKeys()
    {
        var result = new string[data.Blocks.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = panels.Blocks[i].Text;

        return result;
    }

    private static DataType PredictDataType(_Storage storage, string value, out string separator)
    {
        if (value.Contains(storage.Dividers.tuple))
        {
            separator = storage.Dividers.tuple;
            return DataType.Tuple;
        }
        else if (value.Contains(storage.DividersCollection.oneD))
        {
            separator = storage.DividersCollection.oneD;
            return DataType.List;
        }
        else if (value.Contains(storage.DividersCollection.dictionary))
        {
            separator = storage.DividersCollection.dictionary;
            return DataType.Dictionary;
        }

        separator = string.Empty;
        return DataType.Value;
    }
    private static string[] PredictValueTypes(_Storage storage, string[] strValues)
    {
        var result = new string[strValues.Length];

        for (var i = 0; i < strValues.Length; i++)
        {
            var v = strValues[i];
            if (v.StartsWith(STR_PLACEHOLDER))
            {
                v = FilterPlaceholders(storage, v).Replace(storage.Dividers.text, string.Empty);
                if (char.TryParse(v, out _))
                    result[i] = VALUE_SYMBOL;
                else
                    result[i] = VALUE_TEXT;
            }
            else if (v.ToLower() == "true" || v.ToLower() == "false")
                result[i] = VALUE_FLAG;
            else if (v.IsNumber())
                result[i] = VALUE_NUMBER;
            else
                result[i] = VALUE_TEXT;
        }

        return result;
    }

    private static string FilterPlaceholders(_Storage storage, string dataAsText)
    {
        return Regex.Replace(dataAsText, STR_PLACEHOLDER + "(\\d+)", match =>
        {
            var index = int.Parse(match.Groups[1].Value);
            return index >= 0 && index < strings.Count ?
                $"{storage.Dividers.text}{strings[index]}{storage.Dividers.text}" :
                match.Value;
        });
    }
    private static string AddPlaceholders(string dataAsText)
    {
        return Regex.Replace(dataAsText, "`([^`]+)`", match =>
        {
            var replacedValue = STR_PLACEHOLDER + strings.Count;
            strings.Add(match.Groups[1].Value);
            return replacedValue;
        });
    }
    #endregion
}