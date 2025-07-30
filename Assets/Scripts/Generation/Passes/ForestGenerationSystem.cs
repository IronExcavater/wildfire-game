using Generation.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utilities;
using Utilities.Attributes;

namespace Generation.Passes
{
    public struct ForestConfig : IComponentData
    {
        public float ForestFrequency;
        public float2 SpawnChance;

        public float PlainsFrequency;
        public float PlainsThreshold;

        public float2 ElevationFactor;
        public float ValleyBoost;

        public int TreeSpacing;
        public float TreeJitter;
        public float2 TreeScale;
    }

    public class ForestConfigAuthoring : MonoBehaviour
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

        private class ForestConfigBaker : Baker<ForestConfigAuthoring>
        {
            public override void Bake(ForestConfigAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.None), new ForestConfig
                {
                    ForestFrequency = authoring.forestFrequency,
                    SpawnChance = authoring.spawnChance,
                    PlainsFrequency = authoring.plainsFrequency,
                    PlainsThreshold = authoring.plainsThreshold,
                    ElevationFactor = authoring.elevationFactor,
                    ValleyBoost = authoring.valleyBoost,
                    TreeSpacing = authoring.treeSpacing,
                    TreeJitter = authoring.treeJitter,
                    TreeScale = authoring.treeScale
                });
            }
        }
    }

    [UpdateAfter(typeof(HillGenerationSystem))]
    public partial struct ForestGenerationSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldConfig>();
            state.RequireForUpdate<ForestConfig>();

            _query = state.GetEntityQuery(
                ComponentType.ReadOnly<Chunk>(),
                ComponentType.Exclude<ForestGeneratedTag>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var worldConfig = SystemAPI.GetSingleton<WorldConfig>();
            var forestConfig = SystemAPI.GetSingleton<ForestConfig>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var chunks = _query.ToComponentDataArray<Chunk>(Allocator.Temp);
            var entities = _query.ToEntityArray(Allocator.Temp);

            for (var i = 0; i < chunks.Length; i++)
            {
                Apply(ref ecb, ref state, worldConfig, forestConfig, chunks[i], entities[i]);
                ecb.AddComponent<ForestGeneratedTag>(entities[i]);
            }
        }

        private void Apply(ref EntityCommandBuffer ecb, ref SystemState state, WorldConfig worldConfig,
            ForestConfig forestConfig, Chunk chunk, Entity chunkEntity)
        {
            var offset = WorldGenerator.HashSeed(worldConfig.SeedString + GetType().FullName);
            var chunkWorld = WorldGenerator.ChunkToWorld(chunk.Position, worldConfig.ChunkSize);
            var size = worldConfig.ChunkSize * worldConfig.Resolution;
            var step = forestConfig.TreeSpacing * worldConfig.Resolution;

            for (float y = 0; y < size; y += step)
            for (float x = 0; x < size; x += step)
            {
                var world = chunkWorld + new float2(x, y) / worldConfig.Resolution;
                var noise = world + offset;

                var jitter = Utils.JitterNoise(noise.x, noise.y, forestConfig.TreeJitter * step);

                world = chunkWorld + (new float2(x, y) + jitter / worldConfig.Resolution);
                noise = world + offset;

                var height = HeightmapUtils.GetHeight(noise, worldConfig, ref state);

                var forestMask = Mathf.PerlinNoise(noise.x * forestConfig.ForestFrequency, noise.y * forestConfig.ForestFrequency);
                var plainsMask = Mathf.PerlinNoise(noise.x * forestConfig.PlainsFrequency, noise.y * forestConfig.PlainsFrequency);

                // Skip tree if in a plains patch
                if (plainsMask > forestConfig.PlainsThreshold)
                    continue;

                // Boost tree density in lower altitudes
                var valleyFactor = math.saturate(Utils.InverseLerp(forestConfig.ElevationFactor.y, forestConfig.ElevationFactor.x, height));
                var boosted = math.pow(valleyFactor, 1.5f) * (1f + forestMask * forestConfig.ValleyBoost);
                var spawnProbability = math.lerp(forestConfig.SpawnChance.x, forestConfig.SpawnChance.y, math.saturate(boosted));

                var spawnRoll = Utils.StaticNoise(noise.x, noise.y);
                if (spawnRoll > spawnProbability)
                    continue;

                var rotationY = Utils.StaticNoise(noise.x, noise.y) * math.PI2;
                var scale = math.lerp(forestConfig.TreeScale.x, forestConfig.TreeScale.y, Utils.StaticNoise(noise.x, noise.y));

                var tree = ecb.CreateEntity();
                ecb.AddComponent(tree, new LocalTransform
                {
                    Position = new float3(world.x, height, world.y),
                    Rotation = quaternion.Euler(0f, rotationY, 0f),
                    Scale = scale
                });
                ecb.AddComponent<TreeTag>(tree);
            }
        }
    }

    public struct ForestGeneratedTag : IComponentData { }
    public struct TreeTag : IComponentData { }
}
