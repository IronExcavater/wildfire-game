using System;
using Generation.Data;
using UnityEngine;

namespace Generation.Passes
{
    [Serializable]
    public class MountainPass : GeneratorPass
    {
        [Range(0, 1)] public float weight = 0.8f;
        [Range(0.001f, 0.05f)] public float frequency = 0.005f;
        [Range(1, 4)] public float sharpness = 2f;

        public override void Apply(World world, Chunk chunk)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var size = chunkSize * WorldGenerator.Resolution;

            var heightmap = chunk.GetHeightmap();

            var arbitraryOffset = 200000; // Temporary solution to resolve noise seams with negative numbers

            for (var y = 0; y <= size; y++)
            for (var x = 0; x <= size; x++)
            {
                var nx = chunk.Position.x * size + x + arbitraryOffset;
                var ny = chunk.Position.y * size + y + arbitraryOffset;


                var ridge = Mathf.PerlinNoise(nx * frequency, ny * frequency);
                ridge = 1 - Mathf.Abs(2 * ridge - 1);
                ridge = Mathf.Pow(ridge, sharpness) * weight;

                heightmap.Value[x, y] += ridge;
            }
        }
    }
}
