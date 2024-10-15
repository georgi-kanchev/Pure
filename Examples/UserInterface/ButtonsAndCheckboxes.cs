namespace Pure.Examples.UserInterface;

public static class ButtonsAndCheckboxes
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Buttons & Checkboxes Example";

        var counter = 0;
        var button = new Button { Text = "Cool Button" };
        button.Size = (button.Text.Length + 2, 3);
        button.AlignInside((0.5f, 0.2f));
        button.OnInteraction(Interaction.Trigger, () => counter++);
        button.OnDisplay(() =>
        {
            maps.SetButton(button);
            maps.Tilemaps[1].SetText((0, 0), $"The {button.Text} was pressed {counter} times.");
        });

        // ==============

        var checkbox = new Button { Text = "Checkbox" };
        checkbox.Size = (checkbox.Text.Length + 2, 1);
        checkbox.AlignInside((0.5f, 0.5f));
        checkbox.OnDisplay(() => maps.SetCheckbox(checkbox));

        // ==============

        var buttonDisabled = new Button { IsDisabled = true, Text = "Disabled Button" };
        buttonDisabled.Size = (buttonDisabled.Text.Length, 1);
        buttonDisabled.AlignInside((0.5f, 0.8f));
        buttonDisabled.OnDisplay(() =>
        {
            maps.Tilemaps[0].SetText(buttonDisabled.Position, buttonDisabled.Text,
                Color.Gray.ToDark(0.7f));
        });

        return [button, checkbox, buttonDisabled];
    }
}