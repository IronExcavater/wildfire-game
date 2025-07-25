using System;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using UnityEngine;

namespace Generation.Passes
{
    [Serializable]
    public class MountainPass : GeneratorPass
    {
        [Range(0, 1)] public float weight = 0.8f;
        [Range(0.001f, 0.05f)] public float frequency = 0.005f;
        [Range(1, 4)] public float sharpness = 2f;

        public override Task Apply(Chunk chunk, IJob job)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            float resolution = WorldGenerator.Resolution;
            var size = chunkSize * resolution;
            var chunkWorldPos = chunk.WorldPosition;
            var heightmap = chunk.GetHeightmap();
            var offset = GetNoiseOffset();

            for (var y = 0; y <= size; y++)
            for (var x = 0; x <= size; x++)
            {
                var world = new Vector2(chunkWorldPos.x + x / resolution, chunkWorldPos.z + y / resolution);
                var noise = new Vector2(world.x + offset, world.y + offset);

                var ridge = Mathf.PerlinNoise(noise.x * frequency, noise.y * frequency);
                ridge = 1 - Mathf.Abs(2 * ridge - 1);
                ridge = Mathf.Pow(ridge, sharpness) * weight;

                heightmap.Value[x, y] += ridge;
            }

            return Task.CompletedTask;
        }
    }
}
