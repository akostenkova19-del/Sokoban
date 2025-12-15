using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using System.Collections.Generic;

namespace Sokoban;

public class Map
{
    private readonly TileType[,] mapData;
    private readonly Dictionary<TileType, Sprite> spriteToDraw;
    public int Size { get; private set; }
    public int WidthInTiles => mapData.GetLength(0);
    public int HeightInTiles => mapData.GetLength(1);

    public TileType[,] MapData => mapData;

    public Map(TileType[,] currMapData, Dictionary<TileType, Sprite> spriteDraw, int size)
    {
        mapData = currMapData;
        spriteToDraw = spriteDraw;
        Size = size;
    }

    public TileType GetTileType(Point pos)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= WidthInTiles || pos.Y >= HeightInTiles)
        {
            return TileType.Wall;
        }
        return mapData[pos.X, pos.Y];
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < WidthInTiles; x++)
        {
            for (int y = 0; y < HeightInTiles; y++)
            {
                TileType type = mapData[x, y];

                if (spriteToDraw.TryGetValue(type, out Sprite sprite))
                {
                    Vector2 position = new Vector2(x * Size, y * Size);
                    sprite.Draw(spriteBatch, position);
                }
            }
        }
    }
}
