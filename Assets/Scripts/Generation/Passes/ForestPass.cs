using System;
using Generation.Data;
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
        [Range(0.001f, 0.05f)] public float plainsFrequency = 0.005f;
        [Range(0f, 1f)] public float plainsThreshold = 0.6f;

        [Header("Elevation Influence")]
        [MinMax(0f, 30f)] public MinMax elevationFactor = new(10, 20);
        [Range(0f, 1f)] public float valleyBoost = 0.5f;

        [Header("Tree Layout")]
        [Range(1, 16)] public int treeSpacing = 4;
        [Range(0f, 1f)] public float treeJitter = 0.2f;

        [Header("Tree Variation")]
        [MinMax(0.5f, 2f)] public MinMax treeScale = new(0.6f, 1.4f);

        public override void Apply(Chunk chunk)
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var resolution = WorldGenerator.Resolution;
            var size = chunkSize * resolution;
            var chunkWorldPos = chunk.WorldPosition;
            var heightmap = chunk.GetHeightmap();
            var offset = GetNoiseOffset();
            var step = treeSpacing * resolution;

            for (float y = 0; y < size; y += step)
            for (float x = 0; x < size; x += step)
            {
                var cellX = chunkWorldPos.x + x / resolution;
                var cellY = chunkWorldPos.z + y / resolution;

                var jitterAmount = NormalizedHash(cellX, cellY, offset + 100) * treeJitter * step;
                var jitterAngle = NormalizedHash(cellX, cellY, offset + 101) * Mathf.PI * 2f;
                var jitter = new Vector2(Mathf.Cos(jitterAngle), Mathf.Sin(jitterAngle)) * jitterAmount;

                var worldX = chunkWorldPos.x + (x + jitter.x) / resolution;
                var worldY = chunkWorldPos.z + (y + jitter.y) / resolution;

                var height = World.GetHeight(new Vector2(worldX, worldY), chunk.Position, heightmap.Value);

                // Add noise-based forest patterning (rough transitions)
                var nx = worldX + offset;
                var ny = worldY + offset;

                var forestMask = Mathf.PerlinNoise(nx * forestFrequency, ny * forestFrequency);
                var plainsMask = Mathf.PerlinNoise(nx * plainsFrequency, ny * plainsFrequency);

                // Skip tree if in a plains patch
                if (plainsMask > plainsThreshold)
                    continue;

                // Boost tree density in lower altitudes
                var valleyFactor = Mathf.InverseLerp(elevationFactor.max, elevationFactor.min, height);
                var boosted = Mathf.Pow(valleyFactor, 1.5f) * (1f + forestMask * valleyBoost);
                var spawnProbability = spawnChance.Lerp(Mathf.Clamp01(boosted));

                float spawnRoll = NormalizedHash(worldX, worldY, offset + 400);
                if (spawnRoll > spawnProbability)
                    continue;

                var rotationY = NormalizedHash(cellX, cellY, offset + 200) * 360f;
                var scale = treeScale.Lerp(NormalizedHash(cellX, cellY, offset + 300));

                var entity = new Property<Entity>(new Entity(typeof(TreeObject), chunk));
                entity.Value.Position.Value = new Vector3(worldX, height, worldY);
                entity.Value.Rotation.Value = Quaternion.Euler(0f, rotationY, 0f);
                entity.Value.Scale.Value = Vector3.one * scale;

                chunk.AddEntity(entity);
            }
        }
    }
}
