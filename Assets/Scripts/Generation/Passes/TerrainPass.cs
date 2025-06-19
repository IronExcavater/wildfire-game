using System;
using Generation.Data;
using Generation.Objects;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Passes
{
    [Serializable]
    public class TerrainPass : GeneratorPass
    {
        [Range(1, 100)]
        public float amplitude = 5f;
        [Range(0.01f, 100)]
        public float frequency = 0.05f;

        public override void Apply(World world, Chunk chunk)
        {
            var chunkSize = WorldGenerator.ChunkSize;

            if (!chunk.TryGetEntityOfType(typeof(TerrainObject), out var terrain))
            {
                terrain = new Property<Entity>(new Entity(typeof(TerrainObject)));
                chunk.AddEntity(terrain);
            }

            var worldPos = new Vector3(chunk.Position.x * chunkSize, 0, chunk.Position.y * chunkSize);
            terrain.Value.Position.Value = worldPos;

            if (!terrain.Value.TryGetProperty("Heightmap", out Property<float[,]> heightmap) ||
                heightmap.Value.Length != (int)Math.Pow(chunkSize + 1, 2) || heightmap.Value.Rank != 2)
            {
                heightmap = new Property<float[,]>(new float[chunkSize + 1, chunkSize + 1]);
                terrain.Value.AddProperty("Heightmap", heightmap);
            }

            for (var y = 0; y < chunkSize + 1; y++)
            for (var x = 0; x < chunkSize + 1; x++)
            {
                var nx = chunk.Position.x * chunkSize + x;
                var ny = chunk.Position.y * chunkSize + y;
                var height = Mathf.PerlinNoise(nx * frequency, ny * frequency) * amplitude;
                heightmap.Value[x, y] += height;
            }
        }
    }
}
