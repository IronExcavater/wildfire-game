using System;
using Generation.Data;
using UnityEngine;

namespace Generation.Passes
{
    [Serializable]
    public class RiverPass : GeneratorPass
    {
        [Range(0.001f, 0.05f)] public float frequency = 0.01f;
        [Range(0, 10f)] public float depth = 5f;
        [Range(0.01f, 1f)] public float width = 0.1f;

        public override void Apply(Chunk chunk)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var size = chunkSize * WorldGenerator.Resolution;
            var heightmap = chunk.GetHeightmap();
            var offset = GetNoiseOffset();

            for (var y = 0; y <= size; y++)
            for (var x = 0; x <= size; x++)
            {
                var nx = chunk.Position.x * size + x + offset;
                var ny = chunk.Position.y * size + y + offset;

                var flow = Mathf.PerlinNoise(nx * frequency, ny * frequency);
                var dist = Mathf.Abs(flow - 0.5f); // closer to center = river

                if (dist < width)
                {
                    var carve = (1 - (dist / width)) * depth;
                    heightmap.Value[x, y] -= carve;
                }
            }
        }
    }
}
