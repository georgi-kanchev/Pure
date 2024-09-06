using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Supremacy1257;

public class World
{
    public int Seed { get; }
    public (int width, int height) Size { get; }
    public Layer Layer { get; }
    public TilemapPack Tilemaps { get; }

    public World(int width, int height, int seed = 0)
    {
        Seed = seed;
        Size = (width, height);
        Layer = new(Size)
        {
            AtlasPath = "urizen.png",
            AtlasTileGap = (1, 1),
            AtlasTileSize = (12, 12)
        };
        Tilemaps = new(2, Size);

        GenerateTerrain();
    }

    public void Update()
    {
        var mousePos = Mouse.CursorPosition;
        HandleCamera(mousePos - prevMousePos);
        prevMousePos = mousePos;

        for (var i = 0; i < Tilemaps.Count; i++)
            Layer.DrawTilemap(Tilemaps[i]);

        // Layer.DrawLines(paths);
        // Layer.DrawPoints(points);
        Layer.Draw();
    }

#region Backend
    private Point prevMousePos;

    private void HandleCamera(Point mouseDelta)
    {
        if (Mouse.Button.Middle.IsPressed())
            Layer.Offset += mouseDelta * 1.02f / Layer.Zoom;

        if (Mouse.ScrollDelta != 0)
            Layer.Zoom *= Mouse.ScrollDelta < 0 ? 0.95f : 1.05f;
    }
    private void GenerateTerrain()
    {
        var (ground, terrain) = (Tilemaps[0], Tilemaps[1]);
        var tileCount = Layer.AtlasTileCount;
        var full = (0, 10).ToIndex1D(tileCount);
        var grass = new Tile(full, Color.Green.ToDark(0.45f));
        var grass1 = new Tile((2, 9).ToIndex1D(tileCount), Color.White.ToDark(0.3f));
        var water = (6, 22).ToIndex1D(tileCount);
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
        var mount1 = new Tile((2, 13).ToIndex1D(tileCount), Color.White);
        var mount2 = new Tile((2, 14).ToIndex1D(tileCount), Color.White);
        var mount3 = new Tile((2, 13).ToIndex1D(tileCount), Color.White, 0, true);
        ground.Fill(null, grass);

        for (var y = 0; y < Size.height; y++)
            for (var x = 0; x < Size.width; x++)
            {
                var height = new Point(x, y).ToNoise(NoiseType.ValueCubic, 20f, Seed);
                var mountShade = height.Map((0.3f, 0.0f), (0f, 0.6f));

                if (height > 0.55f)
                    terrain.SetTile((x, y), new(water, Color.White.ToDark((height - 0.55f) * 1.8f)));
                else if (height < 0.2f)
                {
                    ground.SetTile((x, y), new(mount, new Color(120).ToDark(mountShade)));
                    terrain.SetTile((x, y), new[] { mount2, mount2, mount2, mount2, mount1 }.ChooseOne());
                }
                else if (height < 0.3f)
                {
                    ground.SetTile((x, y), new(mount, new Color(120).ToDark(mountShade)));
                    terrain.SetTile((x, y), new[]
                    {
                        mount1, mount1, mount1, mount3, mount3, mount3, new()
                    }.ChooseOne());
                }
                else if (height < 0.35f)
                    terrain.SetTile((x, y), new[]
                    {
                        hill1, hill2, hill3, hill4, hill5, new(), new(), new()
                    }.ChooseOne());
            }

        terrain.SetAutoTile(terrain, new Tile[] { -1, 0, -1, -1, water, -1, -1, water, -1 }, waterT);
        terrain.SetAutoTile(terrain, new Tile[] { -1, -1, -1, water, water, 0, -1, -1, -1 }, waterR);
        terrain.SetAutoTile(terrain, new Tile[] { -1, water, -1, -1, water, -1, -1, 0, -1 }, waterB);
        terrain.SetAutoTile(terrain, new Tile[] { -1, -1, -1, 0, water, water, -1, -1, -1 }, waterL);
        terrain.Replace(terrain, 0, null, grass1, grass, grass, grass, grass);

        // var pts = GeneratePoints(w, h, 10, 6f);
        // ReduceWaterPoints(pts, terrain, water, mount1, mount2);

        // var points = pts.ToArray();
        // var paths = GeneratePaths(points);

        for (var y = 0; y < Size.height; y++)
            for (var x = 0; x < Size.width; x++)
            {
                var height = new Point(x, y).ToNoise(NoiseType.ValueCubic, 20f, Seed);
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
                }.ChooseOne());
            }

        for (var i = 0; i < Tilemaps.Count; i++)
        {
            var map = Tilemaps[i];
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