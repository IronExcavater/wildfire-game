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
        [Range(256f, 2048f)] public float cellSize = 512f;
        [Range(0.001f, 0.01f)] public float frequency = 0.005f;
        [Range(0f, 1f)] public float threshold = 0.6f;
        [Range(0f, 0.5f)] public float maxJitter = 0.2f;

        public override void Apply(Chunk chunk)
        {
            var resolution = WorldGenerator.Resolution;
            var chunkWorldPos = chunk.WorldPosition;
            var heightmap = chunk.GetHeightmap();
            var offset = GetNoiseOffset();
            var chunkSize = WorldGenerator.ChunkSize;
            var size = chunkSize * resolution;

            // Step based on cellSize
            var step = Mathf.RoundToInt(cellSize * resolution);

            for (var y = 0f; y < size; y += step)
            for (var x = 0f; x < size; x += step)
            {
                var worldX = chunkWorldPos.x + x / resolution + cellSize * 0.5f;
                var worldY = chunkWorldPos.z + y / resolution + cellSize * 0.5f;

                var nx = worldX + offset;
                var ny = worldY + offset;
                var noise = Mathf.PerlinNoise(nx * frequency, ny * frequency);

                if (noise < threshold)
                    continue;

                var jitterRadius = NormalizedHash(worldX, worldY, offset + 100) * maxJitter;
                var jitterAngle = NormalizedHash(worldX, worldY, offset + 101) * Mathf.PI * 2f;

                //var jitter = new Vector2(Mathf.Cos(jitterAngle), Mathf.Sin(jitterAngle)) * jitterRadius;
                var finalX = worldX;// + jitter.x;
                var finalY = worldY;// + jitter.y;

                var height = World.GetHeight(new Vector2(finalX, finalY), chunk.Position, heightmap.Value);

                var entity = new Property<Entity>(new Entity(typeof(TownObject), chunk));
                entity.Value.Position.Value = new Vector3(finalX, height, finalY);
                chunk.AddEntity(entity);
            }
        }
    }
}
