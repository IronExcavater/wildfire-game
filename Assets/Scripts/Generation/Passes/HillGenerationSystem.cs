using Generation.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Generation.Passes
{
    public struct HillConfig : IComponentData
    {
        public float Amplitude;
        public float Frequency;
        public int Octaves;
        public float Lacunarity;
        public float Persistence;
    }

    public class HillConfigAuthoring : MonoBehaviour
    {
        [Range(1, 100)] public float amplitude = 15f;
        [Range(0.001f, 0.5f)] public float frequency = 0.01f;
        [Range(1, 10)] public int octaves = 8;
        [Range(0.01f, 10)] public float lacunarity = 2f;
        [Range(0.01f, 1)] public float persistence = 0.5f;
    }

    public class HillConfigBaker : Baker<HillConfigAuthoring>
    {
        public override void Bake(HillConfigAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.None), new HillConfig
            {
                Amplitude = authoring.amplitude,
                Frequency = authoring.frequency,
                Octaves = authoring.octaves,
                Lacunarity = authoring.lacunarity,
                Persistence = authoring.persistence
            });
        }
    }

    public partial struct HillGenerationSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldConfig>();
            state.RequireForUpdate<HillConfig>();

            _query = state.GetEntityQuery(
                ComponentType.ReadOnly<Chunk>(),
                ComponentType.Exclude<HillGeneratedTag>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var worldConfig = SystemAPI.GetSingleton<WorldConfig>();
            var hillConfig = SystemAPI.GetSingleton<HillConfig>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var chunks = _query.ToComponentDataArray<Chunk>(Allocator.Temp);
            var entities = _query.ToEntityArray(Allocator.Temp);

            for (var i = 0; i < chunks.Length; i++)
            {
                Apply(ref ecb, ref state, worldConfig, hillConfig, chunks[i], entities[i]);
                ecb.AddComponent<ForestGeneratedTag>(entities[i]);
            }
        }

        private void Apply(ref EntityCommandBuffer ecb, ref SystemState state, WorldConfig worldConfig,
            HillConfig hillConfig, Chunk chunk, Entity chunkEntity)
        {
            var offset = WorldGenerator.HashSeed(worldConfig.SeedString + GetType().FullName);
            var chunkWorld = WorldGenerator.ChunkToWorld(chunk.Position, worldConfig.ChunkSize);
            var size = worldConfig.ChunkSize * worldConfig.Resolution;

            var terrain = ecb.CreateEntity();
            var heightmapBuffer = ecb.AddBuffer<TerrainHeightmap>(terrain);
            ecb.AddComponent(chunkEntity, new TerrainReference { TerrainEntity = terrain });

            for (var y = 0; y <= size; y++)
            for (var x = 0; x <= size; x++)
            {
                var amp = hillConfig.Amplitude;
                var freq = hillConfig.Frequency;

                var height = 0f;
                for (var i = 0; i < hillConfig.Octaves; i++)
                {
                    var world = chunkWorld + new float2(x, y) / worldConfig.Resolution;
                    var noise = world + offset;

                    height += Mathf.PerlinNoise(noise.x * freq, noise.y * freq) * amp;

                    amp *= hillConfig.Persistence;
                    freq *= hillConfig.Lacunarity;
                }

                heightmapBuffer.Add(new TerrainHeightmap { Value = height });
            }
        }
    }

    public struct HillGeneratedTag : IComponentData { }
}
