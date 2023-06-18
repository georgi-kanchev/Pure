namespace Pure.EditorUserInterface;

using UserInterface;

public class MenuAdd : Menu
{
    public MenuAdd(Tilemap.Tilemap background, Tilemap.Tilemap tilemap, RendererEdit editor)
        : base(background, tilemap,
            "Add… ",
            $"  {nameof(Button)}",
            $"  {nameof(InputBox)}",
            $"  {nameof(Pages)}",
            $"  {nameof(Panel)}",
            $"  {nameof(Palette)}",
            $"  {nameof(Slider)}",
            $"  {nameof(Pure.UserInterface.Scroll)}",
            $"  {nameof(List)}")
    {
        this.editor = editor;
        Size = (10, 9);
    }

    protected override void OnItemTrigger(Button item)
    {
        editor.CreateElement(IndexOf(item), Position);
    }

    #region Backend
    protected readonly RendererEdit editor;
    #endregion
}