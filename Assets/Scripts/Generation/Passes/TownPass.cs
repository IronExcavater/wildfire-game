/*
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using Generation.Objects;
using UnityEngine;
using Utilities;
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

            var noise = chunkWorldPos.AddScalar(offset);
            var spawnRoll = Mathf.PerlinNoise(noise.x * frequency, noise.y * frequency);

            if (spawnRoll < threshold)
                return;

            var jitter = GetNoiseJitter(noise.x, noise.y, chunkSize / resolution);
            var townWorldPos = chunkWorldPos + jitter;
            var height = await WorldGenerator.World.GetHeight(townWorldPos, job);

            var entity = new Property<Entity>(new Entity(typeof(TownObject), chunk));
            entity.Value.Position.Value = new Vector3(townWorldPos.x, height, townWorldPos.y);

            chunk.AddEntity(entity);
        }

        public List<Vector3> GetTownsAroundPosition(Vector2 position, int chunkRadius)
        {
            var result = new List<Vector3>();

            var originChunkPos = WorldGenerator.WorldToChunk(position);
            for (var dy = -chunkRadius; dy <= chunkRadius; dy++)
            for (var dx = -chunkRadius; dx <= chunkRadius; dx++)
            {
                if (!WorldGenerator.TryGetChunkReference(originChunkPos + new Vector2Int(dx, dy), out var chunk))
                    continue;

                if (chunk.TryGetEntityOfType<TownObject>(out var town))
                    result.Add(town.Value.Position.Value);
            }

            return result;
        }
    }
}
*/
