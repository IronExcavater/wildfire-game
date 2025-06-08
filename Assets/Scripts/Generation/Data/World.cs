using System.Collections.Generic;
using UnityEngine;

namespace Generation.Data
{
    public sealed class World
    {
        public Dictionary<Vector2Int, Chunk> Chunks = new();

        public World(int width, int height, params IGeneratorPass[] passes)
        {
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    Chunks[new Vector2Int(x, y)] = new Chunk(new Vector2Int(x, y));

            foreach (var pass in passes)
                pass.Apply(this);
        }
    }
}
