using System.Collections.Generic;
using UnityEngine;

namespace Generation.Data
{
    public sealed class World
    {
        public Dictionary<Vector2Int, Chunk> Chunks = new();

        public Chunk GetChunk(Vector2Int position)
        {
            if (!Chunks.TryGetValue(position, out var chunk))
            {
                chunk = new Chunk(position);
                Chunks[position] = chunk;

                foreach (var pass in WorldGenerator.Passes)
                    pass.Apply(this, chunk);
            }

            return chunk;
        }
    }
}
