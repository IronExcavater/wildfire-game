using System;
using Generation.Data;
using Generation.Objects;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Passes
{
    [Serializable]
    public class TownPass : GeneratorPass
    {
        [Range(0.001f, 0.01f)] public float frequency = 0.005f;
        [Range(0f, 1f)] public float threshold = 0.6f;
        [Range(1, 16)] public int buildingSpacing = 4;
        [Range(0f, 1f)] public float buildingJitter = 0.5f;

        public override void Apply(Chunk chunk)
        {
            var resolution = WorldGenerator.Resolution;
            var chunkWorldPos = chunk.WorldPosition;
            var heightmap = chunk.GetHeightmap();
            var offset = GetNoiseOffset();
            var chunkSize = WorldGenerator.ChunkSize;
            var size = chunkSize * resolution;

            var step = buildingSpacing * resolution;

            for (var y = 0f; y < size; y += step)
            for (var x = 0f; x < size; x += step)
            {
                var world = new Vector2(chunkWorldPos.x + x / resolution, chunkWorldPos.z + y / resolution);
                var noise = new Vector2(world.x + offset, world.y + offset);

                var noiseRoll = Mathf.PerlinNoise(noise.x * frequency, noise.y * frequency);

                if (noiseRoll < threshold)
                    continue;

                var jitterRadius = StaticNoise(noise.x, noise.y) * buildingJitter;
                var jitterAngle = StaticNoise(noise.x, noise.y) * Mathf.PI * 2f;
                var jitter = new Vector2(Mathf.Cos(jitterAngle), Mathf.Sin(jitterAngle)) * jitterRadius;

                world = new Vector2(
                    chunkWorldPos.x + (x + jitter.x) / resolution,
                    chunkWorldPos.z + (y + jitter.y) / resolution);

                var height = World.GetHeight(new Vector2(world.x, world.y), chunk.Position, heightmap.Value);

                var entity = new Property<Entity>(new Entity(typeof(TownObject), chunk));
                entity.Value.Position.Value = new Vector3(world.x, height, world.y);
                chunk.AddEntity(entity);
            }
        }
    }
}
