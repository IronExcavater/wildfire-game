using System.Collections.Generic;
using Generation.Passes;
using UnityEngine;

namespace Generation.Data
{
    public sealed class World
    {
        public readonly Vector2Int WorldSize;
        public readonly int ChunkSize;
        public Dictionary<Vector2Int, Chunk> Chunks = new();

        public World(Vector2Int worldSize, int chunkSize, params GeneratorPass[] passes)
        {
            WorldSize = worldSize;
            ChunkSize = chunkSize;

            for (var y = 0; y < WorldSize.y; y++)
            for (var x = 0; x < WorldSize.x; x++)
                Chunks[new Vector2Int(x, y)] = new Chunk(new Vector2Int(x, y));

            foreach (var pass in passes)
                pass.Apply(this);
        }
    }
}
