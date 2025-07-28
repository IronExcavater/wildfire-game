using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using Generation.Passes;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldGenerator : Singleton<WorldGenerator>
    {
        [SerializeField, Range(1, 128)] private int _chunkSize = 32;
        public static int ChunkSize => Instance._chunkSize;

        [SerializeField, Range(1, 10)] private int _resolution = 2;
        public static int Resolution => Instance._resolution;

        [SerializeField, Range(1, 8)] private int _maxLodLevel = 4;
        public static int MaxLodLevel => Instance._maxLodLevel;

        public static string SeedString { get; set; }
        public static int SeedInt => HashSeed(SeedString);

        private World _world = new();
        public static World World => Instance._world;

        private Dictionary<GenerationStage, List<GeneratorPass>> _passes = new();
        public static IReadOnlyDictionary<GenerationStage, List<GeneratorPass>> Passes => Instance._passes;

        [SerializeField] private GeneratorPasses _generatorPasses;

        public void AddPass(GenerationStage stage, GeneratorPass pass) => _passes[stage].Add(pass);

        protected override void Awake()
        {
            base.Awake();
            if (_generatorPasses != null) _passes = _generatorPasses.passes.Dictionary;
            Debug.Log(_passes[GenerationStage.Terrain]);
            SeedString = _generatorPasses?.seed ?? "default";
        }

        public static int2 WorldToChunk(float2 world, int chunkSize) => (int2)math.floor(world / chunkSize);
        public static float2 ChunkToWorld(int2 chunk, int chunkSize) => chunk * chunkSize;

        [SerializeField] private MinMaxInt _seedIntClamp = new(100_000, 100_000_000);
        public static MinMaxInt SeedIntClamp => Instance._seedIntClamp;
        [SerializeField] private MinMaxInt _seedStringClamp = new(10, 15);
        public static MinMaxInt SeedStringClamp => Instance._seedStringClamp;

        public static bool IsSeedValid(string seed)
        {
            var hash = HashSeed(seed, false);
            return hash == Instance._seedIntClamp.Clamp(hash);
        }

        public static string RandomizeSeed()
        {
            return Guid.NewGuid().ToString("N")
                .Substring(0, UnityEngine.Random.Range(Instance._seedStringClamp.min,
                    Instance._seedStringClamp.max));
        }

        public static int HashSeed(string seed, bool clamp = true)
        {
            unchecked
            {
                var hash = 23;
                var len = seed.Length;
                for (var i = 0; i < len; ++i)
                    hash = hash * 31 + seed[i];

                if (!clamp) return hash;
                return math.max(hash, Instance._seedIntClamp.min) % Instance._seedIntClamp.max;
            }
        }

        [ContextMenu("Estimate Seed Length Range")]
        private void EstimateSeedLengthClamp()
        {
            var validMinLength = int.MaxValue;
            var validMaxLength = int.MinValue;

            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            System.Random rng = new();

            for (var i = 0; i < 100_000; i++)
            {
                var length = rng.Next(4, 40);
                var seed = RandomString(length, chars, rng);

                var hash = HashSeed(seed, clamp: false);

                if (hash < _seedIntClamp.min || hash > _seedIntClamp.max)
                    continue;

                validMinLength = Mathf.Min(validMinLength, length);
                validMaxLength = Mathf.Max(validMaxLength, length);
            }

            Debug.Log($"Estimated valid seed lengths: min = {validMinLength}, max = {validMaxLength}");
        }

        private static string RandomString(int length, string chars, System.Random rng)
        {
            var result = new char[length];
            for (var i = 0; i < length; i++)
                result[i] = chars[rng.Next(chars.Length)];
            return new string(result);
        }
    }
}
