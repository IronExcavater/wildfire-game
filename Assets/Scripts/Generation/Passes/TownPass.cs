using System;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using Generation.Objects;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Passes
{
    [Serializable]
    public class TownPass : GeneratorPass
    {
        [Range(0.001f, 0.05f)] public float frequency = 0.025f;
        [Range(0f, 1f)] public float threshold = 0.7f;

        public override async Task Apply(Chunk chunk, IJob job)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            float resolution = WorldGenerator.Resolution;
            var chunkWorldPos = chunk.WorldPosition;
            var offset = GetNoiseOffset();

            var noise = new Vector2(chunkWorldPos.x + offset, chunkWorldPos.z + offset);
            var spawnRoll = Mathf.PerlinNoise(noise.x * frequency, noise.y * frequency);

            if (spawnRoll < threshold)
                return;

            var jitter = GetNoiseJitter(noise.x, noise.y, chunkSize / resolution);
            var townWorldPos = new Vector2(
                chunkWorldPos.x + jitter.x,
                chunkWorldPos.z + jitter.y
            );
            var height = await WorldGenerator.World.GetHeight(townWorldPos, job);

            var entity = new Property<Entity>(new Entity(typeof(TownObject), chunk));
            entity.Value.Position.Value = new Vector3(townWorldPos.x, height, townWorldPos.y);

            chunk.AddEntity(entity);
        }
    }
}
