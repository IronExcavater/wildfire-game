using System;
using Generation.Data;
using UnityEngine;

namespace Generation.Passes
{
    [Serializable]
    public class HillPass : GeneratorPass
    {
        [Range(1, 100)] public float amplitude = 10f;
        [Range(0.001f, 0.5f)] public float frequency = 0.01f;
        [Range(1, 10)] public int octaves = 4;
        [Range(0.01f, 10)] public float lacunarity = 2f;
        [Range(0.01f, 1)] public float persistence = 0.5f;

        public override void Apply(World world, Chunk chunk)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var size = chunkSize * WorldGenerator.Resolution;
            var heightmap = chunk.GetHeightmap();
            var arbitraryOffset = 100000; // Temporary solution to resolve noise seams with negative numbers

            for (var y = 0; y <= size; y++)
            for (var x = 0; x <= size; x++)
            {
                var amp = amplitude;
                var freq = frequency;

                for (var i = 0; i < octaves; i++)
                {
                    var nx = chunk.Position.x * size + x + arbitraryOffset;
                    var ny = chunk.Position.y * size + y + arbitraryOffset;

                    var height = Mathf.PerlinNoise(nx * freq, ny * freq) * amp;
                    heightmap.Value[x, y] += height;

                    amp *= persistence;
                    freq *= lacunarity;
                }
            }
        }
    }
}
