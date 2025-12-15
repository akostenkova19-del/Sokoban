using System.Collections.Generic;

namespace Sokoban
{
    public class Level
    {
        public string Name { get; set; }
        public int LevelNumber { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public List<string> Data { get; set; } = new List<string>();
    }
}