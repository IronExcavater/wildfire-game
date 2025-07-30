using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Generation.Components
{
    public struct WorldConfig : IComponentData
    {
        public int ChunkSize;
        public int Resolution;
        public int MaxLodLevel;
        public int SeedInt;
        public FixedString64Bytes SeedString;

        public int RenderDistance;
        public int SimulationDistance;
    }

    public class WorldConfigAuthoring : MonoBehaviour
    {
        [Range(1, 128)] public int chunkSize = 32;
        [Range(1, 10)] public int resolution = 2;
        [Range(1, 8)] public int maxLodLevel = 4;
        public string seed;

        [Range(1, 16)] public int renderDistance = 4;
        [Range(1, 32)] public int simulationDistance = 12;

        private string _lastSeed;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(seed))
            {
                WorldGenerator.RandomizeSeed();
                Debug.LogWarning("Seed was empty. Generated random seed.");
            }
            else if (!WorldGenerator.IsSeedValid(seed))
            {
                seed = _lastSeed;
                if (!WorldGenerator.IsSeedValid(seed)) _lastSeed = seed = WorldGenerator.RandomizeSeed();
                Debug.LogWarning($"Seed must be between {WorldGenerator.SeedStringClamp.min} and {WorldGenerator.SeedStringClamp.max} characters.");
            }
            else
                _lastSeed = seed;
        }
        #endif

        private class WorldConfigBaker : Baker<WorldConfigAuthoring>
        {
            public override void Bake(WorldConfigAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.None), new WorldConfig
                {
                    ChunkSize = authoring.chunkSize,
                    Resolution = authoring.resolution,
                    MaxLodLevel = authoring.maxLodLevel,
                    SeedInt = WorldGenerator.HashSeed(authoring.seed),
                    SeedString = authoring.seed,
                    RenderDistance = authoring.renderDistance,
                    SimulationDistance = authoring.simulationDistance
                });
            }
        }
    }
}
