using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sokoban;

public class Game1 : Core
{
    private const int tileSize = 64;

    private Sprite _wall, _floor, _crate, _goal;
    private Dictionary<MoveDirection, Sprite> _playerDirectionSprites;
    private SpriteFont _hudFont;
    private GameWorld _gameWorld;
    private LevelManager _levelManager;
    private KeyboardState _previousKeyboardState;

    public Game1() : base("Sokoban", 1280, 720, false) { }

    protected override void LoadContent() // вызывается 1 раз
    {
        try
        {
            var atlas = TextureAtlas.FromFile(Content, "images/atlas.xml");

            _floor = atlas.CreateSprite("floor");
            _wall = atlas.CreateSprite("wall");
            _crate = atlas.CreateSprite("crate");
            _goal = atlas.CreateSprite("goal");

            _playerDirectionSprites = new()
            {
                { MoveDirection.Down, atlas.CreateSprite("player_forward") },
                { MoveDirection.Up, atlas.CreateSprite("player_back") },
                { MoveDirection.Left, atlas.CreateSprite("player_left") },
                { MoveDirection.Right, atlas.CreateSprite("player_right") }
            };

            _hudFont = Content.Load<SpriteFont>("fonts/Font");

            _levelManager = new();
            _levelManager.LoadLevels(Content, "Levels/levels.txt");

            if (_levelManager.CurrentLevel != null)
                InitializeGameWorld(_levelManager.CurrentLevel, _playerDirectionSprites);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
        }
    }

    private void InitializeGameWorld(Level level, Dictionary<MoveDirection, Sprite> playerSprites)
    {
        var (width, height) = (level.Width, level.Height);
        var mapData = new TileType[width, height];
        Player player = null;
        var crates = new List<Crate>();

        var spriteLookup = new Dictionary<TileType, Sprite>
        {
            { TileType.Floor, _floor },
            { TileType.Wall, _wall },
            { TileType.Goal, _goal }
        };

        for (int y = 0; y < height; y++)
        {
            var row = level.Data[y];
            for (int x = 0; x < width; x++)
            {
                var symbol = x < row.Length ? row[x] : ' ';
                TileType tileType = symbol switch
                {
                    '#' => TileType.Wall,
                    '.' => TileType.Goal,
                    _ => TileType.Floor
                };

                mapData[x, y] = tileType;

                switch (symbol)
                {
                    case '@':
                        player ??= new(x, y, playerSprites); // если еще не создан
                        break;
                    case '$':
                        crates.Add(new(x, y, _crate));
                        break;
                }
            }
        }

        var map = new Map(mapData, spriteLookup, tileSize);
        _gameWorld = new(map, player, crates);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

        _gameWorld?.Update(gameTime);
        HandleInput();

        if (_gameWorld.IsLevelCompleted == true &&
            Keyboard.GetState().IsKeyDown(Keys.Space) &&
            _previousKeyboardState.IsKeyUp(Keys.Space) &&
            _levelManager.LoadNextLevel())
        {
            InitializeGameWorld(_levelManager.CurrentLevel, _playerDirectionSprites);
        }
        _previousKeyboardState = Keyboard.GetState();
    }

    private void HandleInput()
    {
        if (_gameWorld?.IsLevelCompleted == true) return;

        var current = Keyboard.GetState();
        var direction = current.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up) ? MoveDirection.Up :
                        current.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down) ? MoveDirection.Down :
                        current.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left) ? MoveDirection.Left :
                        current.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right) ? MoveDirection.Right :
                        MoveDirection.None;

        if (direction != MoveDirection.None)
            _gameWorld.Move(direction);

        if (current.IsKeyDown(Keys.R))
            ResetCurrentLevel();
    }

    public void ResetCurrentLevel()
    {
        if (_levelManager.CurrentLevel != null)
            InitializeGameWorld(_levelManager.CurrentLevel, _playerDirectionSprites);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _gameWorld?.Draw(SpriteBatch);

        if (_hudFont != null && _levelManager.CurrentLevel != null)
        {
            var level = _levelManager.CurrentLevel;
            var hud = new[]
            {
                $"Уровень: {level.LevelNumber} из {_levelManager.TotalLevels}",
                $"Ходов: {_gameWorld?.Steps ?? 0}", // ноль вместо null
                $"Время: {(int)(_gameWorld?.TimeElapsedSeconds ?? 0)}s"
            };

            var startPos = new Vector2(10, 10);
            var lineHeight = _hudFont.LineSpacing + 5;

            for (int i = 0; i < hud.Length; i++)
                SpriteBatch.DrawString(_hudFont, hud[i], startPos + new Vector2(0, i * lineHeight), Color.Black);

            if (_gameWorld?.IsLevelCompleted == true)
            {
                string winMsg = "Уровень пройден";
                string nextMsg = level.LevelNumber < _levelManager.TotalLevels
                    ? "Нажмите ПРОБЕЛ для следующего уровня"
                    : "Победа! Все уровни пройдены!";

                SpriteBatch.DrawString(_hudFont, winMsg, startPos + new Vector2(0, 4 * lineHeight), Color.Black);
                SpriteBatch.DrawString(_hudFont, nextMsg, startPos + new Vector2(0, 5 * lineHeight), Color.Black);
            }
        }

        SpriteBatch.End();
    }
}