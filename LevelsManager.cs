using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Sokoban
{
    public class LevelManager
    {
        private readonly List<Level> allLevels = new();
        private int currentLevelIndex = -1;

        // Возвращает текущий уровень или null, если уровни не загружены.
        public Level CurrentLevel =>
            currentLevelIndex >= 0 && currentLevelIndex < allLevels.Count ? allLevels[currentLevelIndex] : null;

        public int TotalLevels => allLevels.Count;

        /// <summary>
        /// Загружает все уровни из текстового файла.
        /// </summary>
        /// <param name="content">Менеджер контента.</param>
        /// <param name="fileName">Путь к файлу уровней относительно папки Content.</param>
        public void LoadLevels(ContentManager content, string fileName)
        {
            allLevels.Clear();

            string[] lines;
            try
            {
                using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, fileName));
                using var reader = new StreamReader(stream);
                lines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            }
            catch (ContentLoadException)
            {
                Debug.WriteLine($"Ошибка: Файл уровней '{fileName}' не найден.");
                return;
            }

            Level currentLevel = null;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Пропускаем пустые строки и комментарии
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(';'))
                    continue;

                if (IsLevelHeader(trimmedLine))
                {
                    // Завершаем предыдущий уровень, если он был
                    if (currentLevel != null)
                    {
                        FinalizeLevel(currentLevel);
                    }

                    // Новый уровень
                    currentLevel = CreateLevelFromHeader(trimmedLine, allLevels.Count + 1);
                }
                currentLevel.Data.Add(trimmedLine);
            }

            // Сохраняем последний уровень
            if (currentLevel != null)
            {
                FinalizeLevel(currentLevel);
            }

            // Устанавливаем первый уровень как текущий
            currentLevelIndex = allLevels.Count > 0 ? 0 : -1;
        }

        private static bool IsLevelHeader(string line)
        {
            return line.StartsWith("Level");
        }

        private static Level CreateLevelFromHeader(string headerLine, int levelNumber)
        {
            return new Level
            {
                Name = headerLine,
                LevelNumber = levelNumber
            };
        }

        private void FinalizeLevel(Level level)
        {
            if (level.Data.Count > 0)
            {
                level.Height = level.Data.Count;
                level.Width = level.Data.Max(line => line.Length);
                allLevels.Add(level);
            }
        }

        public bool LoadNextLevel()
        {
            if (currentLevelIndex + 1 < allLevels.Count)
            {
                currentLevelIndex++;
                return true;
            }
            return false;
        }
    }
}