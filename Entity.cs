using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using System.Collections.Generic;

namespace Sokoban;

// Абстрактный базовый класс для всех игровых объектов (игрока, коробок и т.д.).
public abstract class Entity
{
    /// Позиция объекта в сетке 
    public Point GridPosition { get; set; }
    public Sprite CurrentSprite { get; set; }

    /// <summary>
    /// Конструктор с заданной позицией и спрайтом
    /// </summary>
    protected Entity(int x, int y, Sprite sprite) // может вызываться только из наследников
    {
        GridPosition = new Point(x, y);
        CurrentSprite = sprite;
    }

    /// <summary>
    /// Отрисовка объекта в заданной позиции.
    /// </summary>
    public virtual void Draw(SpriteBatch spriteBatch, int tileSize) // виртуальный метод
    {
        Vector2 pixelPosition = new Vector2(GridPosition.X * tileSize, GridPosition.Y * tileSize);
        CurrentSprite.Draw(spriteBatch, pixelPosition);
    }
}

public class Player : Entity
{
    // Глубина отрисовки игрока 
    private const float PlayerDepth = 0.6f;

    // Словарь спрайтов для каждого направления движения
    public Dictionary<MoveDirection, Sprite> DirectionSprites { get; private set; }

    /// <summary>
    /// Конструктор в заданной позиции.
    /// </summary>
    public Player(int x, int y, Dictionary<MoveDirection, Sprite> sprites)
        : base(x, y, GetInitialSprite(sprites))
    {
        DirectionSprites = sprites;
        SetDepthForAllSprites(PlayerDepth);
    }

    // Возвращает начальный спрайт (по умолчанию — направление "вниз").
    private static Sprite GetInitialSprite(Dictionary<MoveDirection, Sprite> sprites)
    {
        return sprites.TryGetValue(MoveDirection.Down, out var sprite) ? sprite : null;
    }

    // Устанавливает глубину отрисовки для всех спрайтов игрока.
    private void SetDepthForAllSprites(float depth)
    {
        foreach (var sprite in DirectionSprites.Values)
        {
            sprite.LayerDepth = depth;
        }

        CurrentSprite.LayerDepth = depth;
    }

    /// <summary>
    /// Обновляет спрайт игрока в зависимости от направления движения.
    /// </summary>
    public void UpdateSpriteDirection(MoveDirection direction)
    {
        if (DirectionSprites.TryGetValue(direction, out Sprite newSprite))
        {
            CurrentSprite = newSprite;   
        }
    }
}

public class Crate : Entity
{
    // Глубина отрисовки коробки
    private const float CrateDepth = 0.4f;

    /// <summary>
    /// Указывает, стоит ли коробка на целевом тайле.
    /// </summary>
    public bool IsOnGoal { get; set; } = false;

    /// <summary>
    /// Конструктор коробки в заданной позиции.
    /// </summary>
    public Crate(int x, int y, Sprite sprite)
        : base(x, y, sprite)
    {
        CurrentSprite.LayerDepth = CrateDepth;
    }
}