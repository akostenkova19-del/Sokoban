using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sokoban
{
    public class GameWorld
    {
        private readonly Map mapGlobal;
        private readonly Player playerGlobal;
        private readonly List<Crate> cratesGlobal;

        // Метрики
        public int Steps { get; private set; } = 0;
        public float TimeElapsedSeconds { get; private set; } = 0f;

        // Состояние игры
        public bool IsLevelCompleted { get; private set; } = false;

        public GameWorld(Map map, Player player, List<Crate> crates)
        {
            mapGlobal = map;
            playerGlobal = player;
            cratesGlobal = crates;

        }

        public void Update(GameTime gameTime)
        {
            if (!IsLevelCompleted)
            {
                TimeElapsedSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Преобразует направление движения в смещение координат.
        /// </summary>
        private Point GetMoveOffset(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up: return new Point(0, -1);
                case MoveDirection.Down: return new Point(0, 1);
                case MoveDirection.Left: return new Point(-1, 0);
                case MoveDirection.Right: return new Point(1, 0);
                default: return Point.Zero;
            }
        }

        /// <summary>
        /// Движение игрока в заданном направлении
        /// </summary>
        public void Move(MoveDirection direction)
        {
            if (IsLevelCompleted) return;

            playerGlobal.UpdateSpriteDirection(direction);  
            Point offset = GetMoveOffset(direction);
            Point targetPlayerPos = playerGlobal.GridPosition + offset;

            if (mapGlobal.GetTileType(targetPlayerPos) == TileType.Wall)
            {
                return; // Столкновение со стеной, движение невозможно
            }

            Crate crateToPush = cratesGlobal.FirstOrDefault(c => c.GridPosition == targetPlayerPos); // первый элемент или null

            if (crateToPush != null)
            {
                Point targetCratePos = targetPlayerPos + offset; // толкаем ящик
                TileType nextTileType = mapGlobal.GetTileType(targetCratePos);

                if (nextTileType == TileType.Wall)
                {
                    return; // Ящик упирается в стену
                }

                Crate crateBeyond = cratesGlobal.FirstOrDefault(c => c.GridPosition == targetCratePos);
                if (crateBeyond != null)
                {
                    return; // Ящик упирается в другой ящик
                }

                crateToPush.GridPosition = targetCratePos; // Движение разрешено
                crateToPush.IsOnGoal = mapGlobal.GetTileType(targetCratePos) == TileType.Goal;

                playerGlobal.GridPosition = targetPlayerPos;
                CheckWinCondition(); // Проверяем, не завершен ли уровень
            }
            else
            {
                playerGlobal.GridPosition = targetPlayerPos;
            }
            Steps++;
        }

        /// <summary>
        /// Проверка, находятся ли все ящики на целевых позициях.
        /// </summary>
        private void CheckWinCondition()
        {
            bool allCratesOnGoals = cratesGlobal.All(crate => crate.IsOnGoal);
            IsLevelCompleted = allCratesOnGoals;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            mapGlobal.Draw(spriteBatch);

            foreach (var crate in cratesGlobal)
            {
                crate.Draw(spriteBatch, mapGlobal.Size);
            }
            playerGlobal.Draw(spriteBatch, mapGlobal.Size);
        }
    }
}