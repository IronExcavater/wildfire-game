using Generation.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utilities;
using Utilities.Attributes;
using Utilities.Observables;

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
    }

    public class ForestConfigBaker : Baker<ForestConfigAuthoring>
    {
        public override void Bake(ForestConfigAuthoring authoring)
        {
            AddComponent(new ForestConfig
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
            var world = SystemAPI.GetSingleton<WorldConfig>();
            var forest = SystemAPI.GetSingleton<ForestConfig>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var positions = _query.ToComponentDataArray<Chunk>(Allocator.Temp);
            var entities = _query.ToEntityArray(Allocator.Temp);

            for (var i = 0; i < positions.Length; i++)
                Apply(ref ecb, world, forest, positions[i], entities[i]);
        }

        private static void Apply(ref EntityCommandBuffer ecb, WorldConfig world, ForestConfig forest, Chunk chunk, Entity entity)
        {
            var tree = ecb.CreateEntity();
            ecb.AddComponent(tree, new LocalTransform
            {
            });

            var size = world.ChunkSize * world.Resolution;
            var chunkWorldPos = chunk.ToWorldPosition(world.ChunkSize);
            var offset =
            var step = forest.TreeSpacing * world.Resolution;

            for (float y = 0; y < size; y += step)
            for (float x = 0; x < size; x += step)
            {
                var worldPos = chunkWorldPos + new float2(x, y) / world.Resolution;
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

    public struct ForestGeneratedTag : IComponentData { }
    public struct TreeTag : IComponentData { }
}
