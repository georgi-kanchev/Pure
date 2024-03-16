namespace Pure.Examples.ExamplesUserInterface;

public static class Panels
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Panels Example";

        var alignment = 0f;
        var nl = Environment.NewLine;
        var text = $"- Useful for containing other elements{nl}{nl}" +
                   $"- Title{nl}{nl}" +
                   $"- Can be optionally moved and/or resized{nl}{nl}" +
                   $"- Cannot be resized or moved outside the window{nl}{nl}" +
                   $"- Minimum sizes";
        var panelText = new Panel { Text = "Cool Title", Size = (19, 19), SizeMinimum = (4, 2) };
        panelText.AlignInside((0.1f, 0.5f));
        panelText.OnDisplay(() =>
        {
            var (x, y) = panelText.Position;
            var (w, h) = panelText.Size;

            maps.SetPanel(panelText);
            maps[1].SetTextRectangle(
                position: (x + 1, y + 1),
                size: (w - 2, h - 2),
                text,
                tint: Color.Green);
        });

        //============

        var button = new Button { Text = "CLICK ME!" };
        var panelButton = new Panel { Size = (15, 9), SizeMinimum = (5, 5) };
        panelButton.AlignInside((0.95f, 0.5f));
        panelButton.OnDisplay(() =>
        {
            var (x, y) = panelButton.Position;
            var (w, h) = panelButton.Size;

            maps.SetPanel(panelButton, zOrder: 2);
            button.Position = (x + 1, y + 1);
            button.Size = (w - 2, h - 2);
            maps.SetButton(button, zOrder: 2);
        });

        return new Block[] { panelText, panelButton, button };
    }
}