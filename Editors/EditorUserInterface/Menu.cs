namespace Pure.EditorUserInterface;

internal abstract class Menu : List
{
    protected Menu(params string[] options) : base((int.MaxValue, int.MaxValue), options.Length)
    {
        Size = (15, 15);
        ItemSize = (Size.width, 1);

        for (var i = 0; i < options.Length; i++)
            this[i].Text = options[i];

        OnDisplay(() =>
        {
            var scrollColor = Color.Gray;
            var middle = tilemaps[(int)Layer.EditMiddle];
            var front = tilemaps[(int)Layer.EditFront];

            SetClear(Layer.EditBack, this);
            SetClear(Layer.EditFront, this);

            middle.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));

            if (Scroll.IsHidden)
                return;

            front.SetTile(Scroll.Increase.Position,
                new(Tile.ARROW, GetColor(Scroll.Increase, scrollColor), 3));
            front.SetTile(Scroll.Slider.Handle.Position,
                new(Tile.SHAPE_CIRCLE, GetColor(Scroll.Slider, scrollColor)));
            front.SetTile(Scroll.Decrease.Position,
                new(Tile.ARROW, GetColor(Scroll.Decrease, scrollColor), 1));
        });
        OnItemDisplay(item =>
        {
            Disable(item, item.Text.EndsWith(" "));

            var color = item.IsDisabled ? Color.Gray : Color.Gray.ToBright();
            var front = tilemaps[(int)Layer.EditFront];

            front.SetTextLine(item.Position, item.Text, GetColor(item, color));
        });
    }

    public void Show((int x, int y) position)
    {
        Position = position;
        IsHidden = false;

        if (Position.x + Size.width > Input.TilemapSize.width)
            Position = (Input.TilemapSize.width - Size.width, Position.y);
        if (Position.y + Size.height > Input.TilemapSize.height)
            Position = (Position.x, Input.TilemapSize.height - Size.height);
    }

    public override void Update()
    {
        IsDisabled = IsHidden;
        base.Update();
    }
    protected override void OnInput()
    {
        base.OnInput();

        if (Mouse.ScrollDelta != 0 || Mouse.IsButtonPressed(Mouse.Button.Middle))
            IsHidden = true;

        var key = "on-lmb-deselect" + GetHashCode();
        var onLmbRelease = (Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once(key);
        if (onLmbRelease && IsHovered == false)
            IsHidden = true;
    }

#region Backend
    private static Color GetColor(Block block, Color baseColor)
    {
        if (block.IsDisabled) return baseColor;
        else if (block.IsPressedAndHeld) return baseColor.ToDark();
        else if (block.IsHovered) return baseColor.ToBright();

        return baseColor;
    }
    private static void SetClear(Layer layer, Block block)
    {
        tilemaps[(int)layer].SetBox(block.Position, block.Size, 0, 0, 0, 0);
    }
#endregion
}