namespace Pure.Editors.EditorBase;

using Tools.Tilemapper;

public class Menu : List
{
    public bool IsHidingOnClick { get; set; } = true;

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
            item.IsDisabled = item.Text.EndsWith(" ");
            editor.MapsUi.SetListItem(this, item, 3, false);
        });
        OnUpdate(() =>
        {
            IsDisabled = IsHidden;
            Fit();
        });

        Mouse.OnWheelScroll(() =>
        {
            if (IsHidingOnClick)
                IsHidden = true;
        });
        Mouse.Button.Middle.OnPress(() =>
        {
            if (IsHidingOnClick)
                IsHidden = true;
        });
        Mouse.Button.Left.OnRelease(() =>
        {
            if (IsHidingOnClick && IsHovered == false)
                IsHidden = true;
        });

        editor.Ui.Add(new Block[] { this });
    }

    public void Show((int x, int y) position)
    {
        Position = position;
        IsHidden = false;
    }
}