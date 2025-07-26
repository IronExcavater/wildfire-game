using System;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using Generation.Objects;
using UnityEngine;
using Utilities;
using Utilities.Attributes;
using Utilities.Observables;

namespace Generation.Passes
{
    [Serializable]
    public class ForestPass : GeneratorPass
    {
        [Header("Forest Density")]
        [Range(0.001f, 0.05f)] public float forestFrequency = 0.01f;
        [MinMax(0f, 1f, true)] public MinMax spawnChance = new(0.2f, 0.9f);

        [Header("Plains Exclusion")]
        [Range(0.001f, 0.05f)] public float plainsFrequency = 0.03f;
        [Range(0f, 1f)] public float plainsThreshold = 0.7f;

        [Header("Elevation Influence")]
        [MinMax(0f, 30f)] public MinMax elevationFactor = new(10, 20);
        [Range(0f, 1f)] public float valleyBoost = 0.5f;

        [Header("Tree Layout")]
        [Range(1, 16)] public int treeSpacing = 3;
        [Range(0f, 1f)] public float treeJitter = 0.5f;

        [Header("Tree Variation")]
        [MinMax(0.5f, 2f)] public MinMax treeScale = new(0.6f, 1.4f);

        public override async Task Apply(Chunk chunk, IJob job)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var resolution = WorldGenerator.Resolution;
            var size = chunkSize * resolution;
            var chunkWorldPos = chunk.WorldPosition;
            var offset = GetNoiseOffset();
            var step = treeSpacing * resolution;

            for (float y = 0; y < size; y += step)
            for (float x = 0; x < size; x += step)
            {
                var world = chunkWorldPos + new Vector2(x, y) / resolution;
                var noise = world.AddScalar(offset);

                var jitter = GetNoiseJitter(noise.x, noise.y, treeJitter * step);

                world = chunkWorldPos + (new Vector2(x, y) + jitter / resolution);
                noise = new Vector2(world.x + offset, world.y + offset);

                var height = await WorldGenerator.World.GetHeight(world, job);

                var forestMask = Mathf.PerlinNoise(noise.x * forestFrequency, noise.y * forestFrequency);
                var plainsMask = Mathf.PerlinNoise(noise.x * plainsFrequency, noise.y * plainsFrequency);

                // Skip tree if in a plains patch
                if (plainsMask > plainsThreshold)
                    continue;

                // Boost tree density in lower altitudes
                var valleyFactor = Mathf.InverseLerp(elevationFactor.max, elevationFactor.min, height);
                var boosted = Mathf.Pow(valleyFactor, 1.5f) * (1f + forestMask * valleyBoost);
                var spawnProbability = spawnChance.Lerp(Mathf.Clamp01(boosted));

                var spawnRoll = StaticNoise(noise.x, noise.y);
                if (spawnRoll > spawnProbability)
                    continue;

                var rotationY = StaticNoise(noise.x, noise.y) * 360f;
                var scale = treeScale.Lerp(StaticNoise(noise.x, noise.y));

                var entity = new Property<Entity>(new Entity(typeof(TreeObject), chunk));
                entity.Value.Position.Value = new Vector3(world.x, height, world.y);
                entity.Value.Rotation.Value = Quaternion.Euler(0f, rotationY, 0f);
                entity.Value.Scale.Value = Vector3.one * scale;

                chunk.AddEntity(entity);
            }
        }
    }
}
