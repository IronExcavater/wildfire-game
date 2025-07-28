using System;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using Unity.Entities;
using UnityEngine;
using Utilities;

namespace Generation.Passes
{
    public partial struct HillGenerationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var config
        }
    }

    [Serializable]
    public class HillPass : GeneratorPass
    {
        [Range(1, 100)] public float amplitude = 15f;
        [Range(0.001f, 0.5f)] public float frequency = 0.01f;
        [Range(1, 10)] public int octaves = 8;
        [Range(0.01f, 10)] public float lacunarity = 2f;
        [Range(0.01f, 1)] public float persistence = 0.5f;

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
                var amp = amplitude;
                var freq = frequency;

                for (var i = 0; i < octaves; i++)
                {
                    var world = chunkWorldPos + new Vector2(x, y) / resolution;
                    var noise = world.AddScalar(offset);

                    var height = Mathf.PerlinNoise(noise.x * freq, noise.y * freq) * amp;
                    heightmap.Value[x, y] += height;

                    amp *= persistence;
                    freq *= lacunarity;
                }
            }

            return Task.CompletedTask;
        }
    }
}
