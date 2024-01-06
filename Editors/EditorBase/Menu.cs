namespace Pure.Editors.EditorBase;

using Tools.Tilemapper;

public class Menu : List
{
    public Menu(Editor editor, params string[] options) : base((int.MaxValue, int.MaxValue),
        options.Length)
    {
        Size = (15, 15);
        ItemSize = (Size.width, 1);

        for (var i = 0; i < options.Length; i++)
            this[i].Text = options[i];

        OnDisplay(() => editor.MapsUi.SetList(this, 1));
        OnItemDisplay(item =>
        {
            Disable(item, item.Text.EndsWith(" "));
            editor.MapsUi.SetListItem(this, item, 3, false);
        });

        editor.Ui.Add(new Block[] { this });

        OnUpdate(() => IsDisabled = IsHidden);
    }

    public void Show((int x, int y) position)
    {
        Position = position;
        IsHidden = false;
    }

    protected override void OnInput()
    {
        base.OnInput();

        Fit();

        if (Mouse.ScrollDelta != 0 || Mouse.Button.Middle.IsPressed())
            IsHidden = true;

        var key = "on-lmb-deselect" + GetHashCode();
        var onLmbRelease = (Mouse.Button.Left.IsPressed() == false).Once(key);
        if (onLmbRelease && IsHovered == false)
            IsHidden = true;
    }
}