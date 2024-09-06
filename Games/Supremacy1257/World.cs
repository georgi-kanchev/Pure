using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Monitor = Pure.Engine.Window.Monitor;

namespace Supremacy1257;

public class World
{
    public int Water { get; }
    public int Mount1 { get; }
    public int Mount2 { get; }

    public (int width, int height) Size { get; }
    public Layer Layer { get; }

    public Tilemap Ground
    {
        get => tilemaps[0];
    }
    public Tilemap Terrain
    {
        get => tilemaps[1];
    }

    public World(int width, int height)
    {
        Size = (width, height);
        Layer = new(Size)
        {
            AtlasPath = "urizen.png",
            AtlasTileGap = (1, 1),
            AtlasTileSize = (12, 12)
        };
        tilemaps = new(2, Size);

        Water = (6, 22).ToIndex1D(Layer.AtlasTileCount);
        Mount1 = (2, 13).ToIndex1D(Layer.AtlasTileCount);
        Mount2 = (2, 14).ToIndex1D(Layer.AtlasTileCount);

        Ground.SeedOffset = (0, 0, Game.SEED);
        Terrain.SeedOffset = (0, 0, Game.SEED);

        GenerateTerrain();
    }

    public void Update()
    {
        var mousePos = Mouse.CursorPosition;
        HandleCamera(mousePos - prevMousePos);
        prevMousePos = mousePos;

        for (var i = 0; i < tilemaps.Count; i++)
            Layer.DrawTilemap(tilemaps[i]);

        Layer.Draw();
    }

#region Backend
    private Point prevMousePos;
    private readonly TilemapPack tilemaps;

    private void HandleCamera(Point mouseDelta)
    {
        var (aw, ah) = Monitor.Current.AspectRatio;
        var (ww, wh) = Window.Size;
        var delta = mouseDelta * ((float)aw / ww, (float)ah / wh) / Layer.Zoom * 122.5f;

        if (Mouse.IsAnyPressed())
            Layer.Offset += delta;

        if (Mouse.ScrollDelta != 0)
            Layer.Zoom *= Mouse.ScrollDelta < 0 ? 0.95f : 1.05f;
    }
    private void GenerateTerrain()
    {
        var (ground, terrain) = (tilemaps[0], tilemaps[1]);
        var tileCount = Layer.AtlasTileCount;
        var full = (0, 10).ToIndex1D(tileCount);
        var grass = new Tile(full, Color.Green.ToDark(0.45f));
        var grass1 = new Tile((2, 9).ToIndex1D(tileCount), Color.White.ToDark(0.3f));
        var waterT = (6, 19).ToIndex1D(tileCount);
        var waterR = new Tile(waterT, Color.White, 1);
        var waterB = new Tile(waterT, Color.White, 2);
        var waterL = new Tile(waterT, Color.White, 3);
        var trees1 = (2, 1).ToIndex1D(tileCount);
        var trees2 = (2, 0).ToIndex1D(tileCount);
        var hill1 = new Tile((2, 10).ToIndex1D(tileCount), Color.White.ToDark(0.3f));
        var hill2 = new Tile((2, 11).ToIndex1D(tileCount), Color.White.ToDark(0.3f));
        var hill3 = new Tile((2, 12).ToIndex1D(tileCount), Color.White.ToDark(0.3f));
        var hill4 = new Tile((2, 10).ToIndex1D(tileCount), Color.White.ToDark(0.3f), 0, true);
        var hill5 = new Tile((2, 12).ToIndex1D(tileCount), Color.White.ToDark(0.3f), 0, true);
        var mount = (0, 10).ToIndex1D(tileCount);
        var mount1 = new Tile(Mount1, Color.White);
        var mount2 = new Tile(Mount2, Color.White);
        var mount3 = new Tile(Mount1, Color.White, 0, true);
        ground.Fill(null, grass);
        terrain.Flush();

        for (var y = 0; y < Size.height; y++)
            for (var x = 0; x < Size.width; x++)
            {
                var height = new Point(x, y).ToNoise(NoiseType.ValueCubic, 20f, Game.SEED);
                var mountShade = height.Map((0.3f, 0.0f), (0f, 0.6f));

                if (height > 0.55f)
                {
                    var tile = new Tile(Water, Color.White.ToDark((height - 0.55f) * 1.8f));
                    ground.SetTile((x, y), tile);
                    terrain.SetTile((x, y), tile);
                }
                else if (height < 0.2f)
                {
                    ground.SetTile((x, y), new(mount, new Color(120).ToDark(mountShade)));
                    terrain.SetTile((x, y), new[]
                    {
                        mount2, mount2, mount2, mount2, mount1
                    }.ChooseOne(Game.SEED.ToSeed(x, y, 0)));
                }
                else if (height < 0.3f)
                {
                    ground.SetTile((x, y), new(mount, new Color(120).ToDark(mountShade)));
                    terrain.SetTile((x, y), new[]
                    {
                        mount1, mount1, mount1, mount3, mount3, mount3, new()
                    }.ChooseOne(Game.SEED.ToSeed(x, y, 1)));
                }
                else if (height < 0.35f)
                    terrain.SetTile((x, y), new[]
                    {
                        hill1, hill2, hill3, hill4, hill5, new(), new(), new()
                    }.ChooseOne(Game.SEED.ToSeed(x, y, 2)));
            }

        terrain.SetAutoTile(terrain, new Tile[] { -1, 0, -1, -1, Water, -1, -1, Water, -1 }, waterT);
        terrain.SetAutoTile(terrain, new Tile[] { -1, -1, -1, Water, Water, 0, -1, -1, -1 }, waterR);
        terrain.SetAutoTile(terrain, new Tile[] { -1, Water, -1, -1, Water, -1, -1, 0, -1 }, waterB);
        terrain.SetAutoTile(terrain, new Tile[] { -1, -1, -1, 0, Water, Water, -1, -1, -1 }, waterL);
        terrain.Replace(terrain, 0, null, grass1, grass, grass, grass, grass);

        for (var y = 0; y < Size.height; y++)
            for (var x = 0; x < Size.width; x++)
            {
                var height = new Point(x, y).ToNoise(NoiseType.ValueCubic, 20f, Game.SEED);
                var trees = new Point(x, y).ToNoise(NoiseType.ValueCubic);

                if (trees < 0.6f || height.IsBetween((0.3f, 0.55f)) == false)
                    continue;

                var colorF = Color.White.ToDark(trees.Map((0.6f, 1f), (0.1f, 0.65f)));
                var colorB = Color.Green.ToDark(trees.Map((0.6f, 1f), (0.5f, 0.75f)));

                ground.SetTile((x, y), new(full, colorB));
                terrain.SetTile((x, y), new Tile[]
                {
                    new(trees1, colorF), new(trees2, colorF),
                    new(trees1, colorF, 0, true), new(trees2, colorF, 0, true)
                }.ChooseOne(Game.SEED.ToSeed(x, y, 3)));
            }

        for (var i = 0; i < tilemaps.Count; i++)
        {
            var map = tilemaps[i];
            var (mapW, mapH) = (map.Size.width - 1, map.Size.height - 1);
            var color = Color.Brown.ToDark();

            map.SetLine((0, 0), (mapW, 0), null, new Tile(full, color));
            map.SetLine((mapW, 0), (mapW, mapH), null, new Tile(full, color));
            map.SetLine((mapW, mapH), (0, mapH), null, new Tile(full, color));
            map.SetLine((0, mapH), (0, 0), null, new Tile(full, color));
        }
    }
#endregion
}