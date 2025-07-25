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
        [Range(0.001f, 0.05f)] public float frequency = 0.015f;
        [Range(0f, 1f)] public float threshold = 0.9f;
        [Range(1, 16)] public int buildingSpacing = 4;
        [Range(0f, 1f)] public float buildingJitter = 0.5f;

        public override async Task Apply(Chunk chunk, IJob job)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var resolution = WorldGenerator.Resolution;
            var size = chunkSize * resolution;
            var chunkWorldPos = chunk.WorldPosition;
            var offset = GetNoiseOffset();
            var step = buildingSpacing * resolution;

            for (float y = 0; y < size; y += step)
            for (float x = 0; x < size; x += step)
            {
                var world = new Vector2(chunkWorldPos.x + x / resolution, chunkWorldPos.z + y / resolution);
                var noise = new Vector2(world.x + offset, world.y + offset);

                var jitter = GetNoiseJitter(noise.x, noise.y, buildingJitter * step);

                world = new Vector2(
                    chunkWorldPos.x + (x + jitter.x) / resolution,
                    chunkWorldPos.z + (y + jitter.y) / resolution);
                noise = new Vector2(world.x + offset, world.y + offset);

                var height = await chunk.World.GetHeight(world, job);

                var spawnRoll = Mathf.PerlinNoise(noise.x * frequency, noise.y * frequency);
                if (spawnRoll < threshold)
                    continue;

                var rotationY = StaticNoise(noise.x, noise.y) * 360f;

                var entity = new Property<Entity>(new Entity(typeof(TownObject), chunk));
                entity.Value.Position.Value = new Vector3(world.x, height, world.y);
                entity.Value.Rotation.Value = Quaternion.Euler(0f, rotationY, 0f);

                chunk.AddEntity(entity);
            }
        }
    }
}
