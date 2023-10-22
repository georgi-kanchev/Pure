namespace Pure.Engine.Window;

using SFML.Graphics;
using SFML.System;

internal static class Vertices
{
    public static string graphicsPath = "default";
    public static int tileIdFull = 10;
    public static (int w, int h) tileGap, tileSize = (8, 8);
    public static (float x, float y) offset;
    public static float zoom = 1f;

    public static Shader? retroScreen;
    public static readonly SortedDictionary<string, VertexArray> vertexQueue = new();

    public static void QueueRectangle((float x, float y) position, (float w, float h) size, uint tint)
    {
        Window.TryNoWindowException();

        TryInitQueue();

        var (w, h) = size;
        var (x, y) = (position.x * tileSize.w, position.y * tileSize.h);
        var c = new Color(tint);
        var verts = vertexQueue[graphicsPath];
        var (ttl, ttr, tbr, tbl) = GetTexCoords(tileIdFull, (1, 1));
        var tl = new Vector2f((int)x, (int)y);
        var br = new Vector2f((int)x + tileSize.w * w, (int)y + tileSize.h * h);
        var tr = new Vector2f(br.X, tl.Y);
        var bl = new Vector2f(tl.X, br.Y);

        verts.Append(new(tl, c, ttl));
        verts.Append(new(tr, c, ttr));
        verts.Append(new(br, c, tbr));
        verts.Append(new(bl, c, tbl));
    }
    public static void QueueLine((float, float) a, (float, float) b, uint tint)
    {
        TryInitQueue();

        var (tileW, tileH) = tileSize;
        var (x0, y0) = a;
        var (x1, y1) = b;
        var dx = MathF.Abs(x1 - x0);
        var dy = -MathF.Abs(y1 - y0);
        var (stepX, stepY) = (1f / tileW * 0.999f, 1f / tileH * 0.999f);
        var sx = x0 < x1 ? stepX : -stepY;
        var sy = y0 < y1 ? stepX : -stepY;
        var err = dx + dy;

        while (true)
        {
            QueuePoint((x0, y0), tint);

            if (IsWithin(x0, x1, stepX) && IsWithin(y0, y1, stepY))
                break;

            var e2 = 2f * err;

            if (e2 > dy)
            {
                err += dy;
                x0 += sx;
            }

            if (e2 < dx == false)
                continue;

            err += dx;
            y0 += sy;
        }
    }
    public static void QueueTile(
        (float, float) position,
        int tile,
        uint tint,
        sbyte angle,
        (int, int) size,
        (bool, bool) flips)
    {
        TryInitQueue();

        var (w, h) = size;
        w = w == 0 ? 1 : w;
        h = h == 0 ? 1 : h;

        var tiles = new int[Math.Abs(w), Math.Abs(h)];
        var gfxSize = Window.graphics[graphicsPath].Size;
        var (gapX, gapY) = tileGap;
        var (tileW, tileH) = tileSize;
        var tilesetTileCount = ((int)gfxSize.X / (tileW + gapX), (int)gfxSize.Y / (tileH + gapY));
        var (tileX, tileY) = IndexToCoords(tile, tilesetTileCount);
        var (x, y) = position;

        for (var j = 0; j < Math.Abs(h); j++)
            for (var i = 0; i < Math.Abs(w); i++)
                tiles[i, j] = CoordsToIndex(tileX + i, tileY + j, tilesetTileCount.Item1);

        if (w < 0)
            FlipVertically(tiles);
        if (h < 0)
            FlipHorizontally(tiles);
        if (flips.Item1)
            FlipVertically(tiles);
        if (flips.Item2)
            FlipHorizontally(tiles);

        for (var i = 0; i < tiles.GetLength(1); i++)
            for (var j = 0; j < tiles.GetLength(0); j++)
                QueueTile((x + j, y + i), tiles[j, i], tint, angle, (w, h));
    }
    public static void QueueTilemap(
        (int tile, uint tint, sbyte angle, bool isFlippedHorizontally, bool isFlippedVertically)[,]
            tilemap)
    {
        Window.TryNoWindowException();

        TryInitQueue();

        var (tilemapW, tilemapH) = (tilemap.GetLength(0), tilemap.GetLength(1));
        tilemapCenter = (tilemapW * tileSize.w / 2, tilemapH * tileSize.h / 2);

        for (var y = 0; y < tilemapH; y++)
            for (var x = 0; x < tilemapW; x++)
            {
                var id = tilemap[x, y].tile;
                if (id == 0)
                    continue;

                var tint = new Color(tilemap[x, y].tint);
                var tl = new Vector2f(x * tileSize.w, y * tileSize.h);
                var tr = new Vector2f((x + 1) * tileSize.w, y * tileSize.h);
                var br = new Vector2f((x + 1) * tileSize.w, (y + 1) * tileSize.h);
                var bl = new Vector2f(x * tileSize.w, (y + 1) * tileSize.h);
                var (ttl, ttr, tbr, tbl) = GetTexCoords(id, (1, 1));
                var rotated = GetRotatedPoints(tilemap[x, y].angle, tl, tr, br, bl);
                var (flipX, flipY) = (tilemap[x, y].isFlippedHorizontally,
                    tilemap[x, y].isFlippedVertically);

                if (flipX)
                {
                    (ttl, ttr) = (ttr, ttl);
                    (tbl, tbr) = (tbr, tbl);
                }

                if (flipY)
                {
                    (ttl, tbl) = (tbl, ttl);
                    (ttr, tbr) = (tbr, ttr);
                }

                tl = rotated[0];
                tr = rotated[1];
                br = rotated[2];
                bl = rotated[3];

                tl = new((int)tl.X, (int)tl.Y);
                tr = new((int)tr.X, (int)tr.Y);
                br = new((int)br.X, (int)br.Y);
                bl = new((int)bl.X, (int)bl.Y);

                vertexQueue[graphicsPath].Append(new(tl, tint, ttl));
                vertexQueue[graphicsPath].Append(new(tr, tint, ttr));
                vertexQueue[graphicsPath].Append(new(br, tint, tbr));
                vertexQueue[graphicsPath].Append(new(bl, tint, tbl));
            }
    }
    public static void QueuePoint((float x, float y) position, uint tint)
    {
        if (Window.window == null)
            return;

        var (tileW, tileH) = tileSize;
        QueueRectangle(position, (1f / tileW, 1f / tileH), tint);
    }

    public static void TryInitQueue()
    {
        if (vertexQueue.ContainsKey(graphicsPath) == false)
            vertexQueue[graphicsPath] = new(PrimitiveType.Quads);
    }
    public static void DrawQueue()
    {
        Window.TryNoWindowException();

        Window.renderTexture.Clear();
        foreach (var kvp in vertexQueue)
        {
            var tex = Window.graphics[kvp.Key];
            var tr = Transform.Identity;
            var (centerX, centerY) = tilemapCenter;
            tr.Translate(offset.x, offset.y);
            tr.Scale(zoom, zoom, centerX, centerY);
            var r = new RenderStates(BlendMode.Alpha, tr, tex, null);
            Window.renderTexture.Draw(kvp.Value, r);
        }

        Window.renderTexture.Display();

        var sz = Window.window.GetView().Size;
        var (tw, th) = (Window.renderTexture.Size.X, Window.renderTexture.Size.Y);
        var shader = Window.IsRetro ? retroScreen : null;
        var rend = new RenderStates(
            BlendMode.Alpha, Transform.Identity, Window.renderTexture.Texture, shader);
        var verts = new Vertex[]
        {
            new(new(0, 0), Color.White, new(0, 0)),
            new(new(sz.X, 0), Color.White, new(tw, 0)),
            new(new(sz.X, sz.Y), Color.White, new(tw, th)),
            new(new(0, sz.Y), Color.White, new(0, th)),
        };

        if (Window.IsRetro)
        {
            var randVec = new Vector2f(retroRand.Next(0, 10) / 10f, retroRand.Next(0, 10) / 10f);
            shader?.SetUniform("time", retroScreenTimer.ElapsedTime.AsSeconds());
            shader?.SetUniform("randomVec", randVec);
            shader?.SetUniform("viewSize", new Vector2f(Window.window.Size.X, Window.window.Size.Y));

            if (Window.isClosing && retroTurnoffTime != null)
            {
                var timing = retroTurnoffTime.ElapsedTime.AsSeconds() / RETRO_TURNOFF_TIME;
                shader?.SetUniform("turnoffAnimation", timing);
            }
        }

        Window.window.Draw(verts, PrimitiveType.Quads, rend);
    }
    public static void ClearQueue()
    {
        foreach (var kvp in vertexQueue)
            kvp.Value.Clear();
    }

    public static void StartRetroAnimation()
    {
        retroTurnoffTime = new();
        retroTurnoff = new(RETRO_TURNOFF_TIME * 1000);
        retroTurnoff.Start();
        retroTurnoff.Elapsed += (_, _) => Window.window?.Close();
    }

#region Backend
    private static readonly Random retroRand = new();
    private static readonly Clock retroScreenTimer = new();
    private static System.Timers.Timer? retroTurnoff;
    private static Clock? retroTurnoffTime;
    private const float RETRO_TURNOFF_TIME = 0.5f;
    private static (int x, int y) tilemapCenter;

    private static bool IsWithin(float number, float targetNumber, float range)
    {
        return IsBetween(number, targetNumber - range, targetNumber + range);
    }
    private static bool IsBetween(float number, float rangeA, float rangeB)
    {
        if (rangeA > rangeB)
            (rangeA, rangeB) = (rangeB, rangeA);

        var l = rangeA <= number;
        var u = rangeB >= number;
        return l && u;
    }
    private static (int, int) IndexToCoords(int index, (int, int) fieldSize)
    {
        index = index < 0 ? 0 : index;
        index = index > fieldSize.Item1 * fieldSize.Item2 - 1 ?
            (fieldSize.Item1 * fieldSize.Item2 - 1) :
            index;

        return (index % fieldSize.Item1, index / fieldSize.Item1);
    }
    private static int CoordsToIndex(int x, int y, int width)
    {
        return y * width + x;
    }
    private static int Wrap(int number, int targetNumber)
    {
        return ((number % targetNumber) + targetNumber) % targetNumber;
    }
    private static void Shift<T>(IList<T> collection, int offset)
    {
        if (offset == default)
            return;

        if (offset < 0)
        {
            offset = Math.Abs(offset);
            for (var j = 0; j < offset; j++)
            {
                var temp = new T[collection.Count];
                for (var i = 0; i < collection.Count - 1; i++)
                    temp[i] = collection[i + 1];
                temp[^1] = collection[0];

                for (var i = 0; i < temp.Length; i++)
                    collection[i] = temp[i];
            }

            return;
        }

        for (var j = 0; j < offset; j++)
        {
            var tmp = new T[collection.Count];
            for (var i = 1; i < collection.Count; i++)
                tmp[i] = collection[i - 1];
            tmp[0] = collection[tmp.Length - 1];

            for (var i = 0; i < tmp.Length; i++)
                collection[i] = tmp[i];
        }
    }
    private static Vector2f[] GetRotatedPoints(sbyte angle, params Vector2f[] points)
    {
        Shift(points, Wrap(-angle, 4));
        return points;
    }
    private static void FlipHorizontally<T>(T[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols / 2; j++)
            {
                (matrix[i, j], matrix[i, cols - j - 1]) = (matrix[i, cols - j - 1], matrix[i, j]);
            }
    }
    private static void FlipVertically<T>(T[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        for (var i = 0; i < rows / 2; i++)
            for (var j = 0; j < cols; j++)
            {
                (matrix[i, j], matrix[rows - i - 1, j]) = (matrix[rows - i - 1, j], matrix[i, j]);
            }
    }
    private static void QueueTile(
        (float x, float y) position,
        int id,
        uint tint,
        sbyte angle,
        (int w, int h) size)
    {
        if (Window.window == null || id == 0)
            return;

        var verts = vertexQueue[graphicsPath];
        var (w, h) = size;
        w = Math.Abs(w);
        h = Math.Abs(h);

        var (ttl, ttr, tbr, tbl) = GetTexCoords(id, size);
        var (x, y) = (position.x * tileSize.w, position.y * tileSize.h);
        var c = new Color(tint);
        var tl = new Vector2f((int)x, (int)y);
        var br = new Vector2f((int)(x + tileSize.w * w), (int)(y + tileSize.h * h));

        if (angle is 1 or 3)
            br = new((int)(x + tileSize.h * h), (int)(y + tileSize.w * w));

        var tr = new Vector2f(br.X, tl.Y);
        var bl = new Vector2f(tl.X, br.Y);
        var rotated = GetRotatedPoints((sbyte)-angle, ttl, ttr, tbr, tbl);
        ttl = rotated[0];
        ttr = rotated[1];
        tbr = rotated[2];
        tbl = rotated[3];

        if (size.w < 0)
        {
            (ttl, ttr) = (ttr, ttl);
            (tbl, tbr) = (tbr, tbl);
        }

        if (size.h < 0)
        {
            (ttl, tbl) = (tbl, ttl);
            (ttr, tbr) = (tbr, ttr);
        }

        verts.Append(new(tl, c, ttl));
        verts.Append(new(tr, c, ttr));
        verts.Append(new(br, c, tbr));
        verts.Append(new(bl, c, tbl));
    }
    private static (Vector2f tl, Vector2f tr, Vector2f br, Vector2f bl) GetTexCoords(
        int id,
        (int, int) size)
    {
        var (w, h) = size;
        var texture = Window.graphics[graphicsPath];
        var (tileW, tileH) = tileSize;
        var texSz = texture.Size;
        var (tileGapW, tileGapH) = tileGap;
        var tileCount = ((int)texSz.X / (tileW + tileGapW), (int)texSz.Y / (tileH + tileGapH));
        var (texX, texY) = IndexToCoords(id, tileCount);
        var tl = new Vector2f(
            (texX) * (tileW + tileGapW),
            (texY) * (tileH + tileGapH));
        var tr = tl + new Vector2f(tileW * w, 0);
        var br = tl + new Vector2f(tileW * w, tileH * h);
        var bl = tl + new Vector2f(0, tileH * h);
        return (tl, tr, br, bl);
    }
#endregion
}