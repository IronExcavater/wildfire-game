using Generation.Passes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Utilities;

namespace Generation.Components
{
    public struct Heightmap : IBufferElementData
    {
        public float Value;
    }

    public struct TerrainReference : IComponentData
    {
        public Entity TerrainEntity;
    }

    public static class HeightmapUtils
    {
        public static float GetHeight(float2 worldPosition, WorldConfig worldConfig, ref SystemState state)
        {
            var sample = worldPosition * worldConfig.Resolution;

            var floor = (int2)math.floor(sample);
            var frac = sample - floor;

            var bottomLeft = GetHeight(floor, worldConfig, ref state);
            var bottomRight = GetHeight(floor + new int2(1, 0), worldConfig, ref state);
            var topLeft = GetHeight(floor + new int2(0, 1), worldConfig, ref state);
            var topRight = GetHeight(floor + new int2(1, 1), worldConfig, ref state);

            var bottomInterp = math.lerp(bottomLeft, bottomRight, frac.x);
            var topInterp = math.lerp(topLeft, topRight, frac.x);
            return math.lerp(bottomInterp, topInterp, frac.y);
        }

        public static float GetHeight(int2 worldPosition, WorldConfig worldConfig, ref SystemState state)
        {
            var mapSize = worldConfig.ChunkSize * worldConfig.Resolution;
            var chunk = (int2)math.floor((float2)worldPosition / mapSize);

            var local = Utils.EuclideanMod(worldPosition, mapSize);
            if (local.x < 0) { chunk.x--; local.x += mapSize; }
            if (local.y < 0) { chunk.y--; local.y += mapSize; }

            var chunkLookupSystem = state.World.GetExistingSystemManaged<ChunkLookupSystem>();
            if (!chunkLookupSystem.TryGetChunk(chunk, out var chunkEntity))
                return 0f;

            if (!state.EntityManager.HasComponent<TerrainReference>(chunkEntity))
                return 0f;

            var terrainEntity = state.EntityManager.GetComponentData<TerrainReference>(chunkEntity).TerrainEntity;
            if (!state.EntityManager.HasBuffer<Heightmap>(terrainEntity))
                return 0f;

            var heightmapBuffer = state.EntityManager.GetBuffer<Heightmap>(terrainEntity);

            var resolution = worldConfig.Resolution;
            var dim = worldConfig.ChunkSize * resolution + 1;
            var index = local.y * dim + local.x;

            if (index < 0 || index >= heightmapBuffer.Length)
                return 0f;

            return heightmapBuffer[index].Value;
        }
    }
}
