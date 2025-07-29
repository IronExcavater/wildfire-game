using System;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldGenerator : Singleton<WorldGenerator>
    {
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
