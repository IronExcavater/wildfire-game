using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Utilities;

namespace Generation.Components
{
    public struct WorldConfig : IComponentData
    {
        public int ChunkSize;
        public int Resolution;
        public int MaxLodLevel;
        public int SeedInt;
        public FixedString64Bytes SeedString;
    }

    public class WorldConfigAuthoring : MonoBehaviour
    {
        [Range(1, 128)] public int chunkSize = 32;
        [Range(1, 10)] public int resolution = 2;
        [Range(1, 8)] public int maxLodLevel = 4;
        public string seed;

        private string _lastSeed;
        private static readonly MinMaxInt SeedLengthClamp = new(10, 15);

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(seed))
            {
                RandomizeSeed();
                Debug.LogWarning("Seed was empty. Generated random seed.");
            }
            else if (!IsSeedValid(seed))
            {
                seed = _lastSeed;
                if (!IsSeedValid(seed)) _lastSeed = seed = RandomizeSeed();
                Debug.LogWarning($"Seed must be between {SeedLengthClamp.min} and {SeedLengthClamp.max} characters.");
            }
            else
                _lastSeed = seed;
        }
        #endif
    }

    public class WorldConfigBaker : Baker<WorldConfigAuthoring>
    {
        public override void Bake(WorldConfigAuthoring authoring)
        {
            AddComponent(new WorldConfig
            {
                ChunkSize = authoring.chunkSize,
                Resolution = authoring.resolution,
                MaxLodLevel = authoring.maxLodLevel,
                SeedInt = WorldConfigAuthoring.HashSeed(authoring.seed),
                SeedString = authoring.seed
            });
        }
    }
}
