namespace Pure.Examples.UserInterface;

public static class Panels
{
    public static Block[] Create(TileMapPack maps)
    {
        Window.Title = "Pure - Panels Example";

        var text = $"- Useful for containing other elements\n\n" +
                   $"- Title\n\n" +
                   $"- Can be optionally moved and/or resized\n\n" +
                   $"- Cannot be resized or moved outside the window\n\n" +
                   $"- Minimum sizes";
        var panelText = new Panel { Text = "Cool Title", Size = (19, 19), SizeMinimum = (4, 2) };
        panelText.AlignInside((0.1f, 0.5f));
        panelText.OnDisplay += () =>
        {
            var (x, y) = panelText.Position;
            var (w, h) = panelText.Size;

            maps.SetPanel(panelText);
            text = text.Constrain((w - 2, h - 2));
            maps.TileMaps[1].SetText((x + 1, y + 1), text, Color.Green);
        };

        //============

        var button = new Button { Text = "CLICK ME!" };
        var panelButton = new Panel { Size = (15, 9), SizeMinimum = (5, 5) };
        panelButton.AlignInside((0.95f, 0.5f));
        panelButton.OnDisplay += () =>
        {
            var (x, y) = panelButton.Position;
            var (w, h) = panelButton.Size;

            maps.SetPanel(panelButton, 2);
            button.Position = (x + 1, y + 1);
            button.Size = (w - 2, h - 2);
            maps.SetButton(button, 2);
        };

        return [panelText, panelButton, button];
    }
}