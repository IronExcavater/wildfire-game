using System;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using UnityEngine;

namespace Generation.Passes
{
    [Serializable]
    public abstract class GeneratorPass
    {
        protected int GetNoiseOffset()
        {
            var raw = WorldGenerator.HashSeed(WorldGenerator.SeedString + GetType().FullName);
            return raw % 100_000_000;
        }

        protected static float StaticNoise(float x, float y)
        {
            var ix = (ulong)Mathf.FloorToInt(x);
            var iy = (ulong)Mathf.FloorToInt(y);

            var hash = Mix(ix, iy, (ulong)WorldGenerator.SeedInt);
            return (hash & 0xFFFFFFFF) / (float)uint.MaxValue;
        }

        private static ulong Mix(ulong a, ulong b, ulong seed)
        {
            const ulong prime1 = 0xa0761d6478bd642f;
            const ulong prime2 = 0xe7037ed1a0b428db;
            var result = (a ^ prime1) * (b ^ prime2);
            result ^= (result >> 32);
            result *= (seed ^ prime1);
            result ^= (result >> 29);
            return result;
        }

        protected static Vector2 GetNoiseJitter(float x, float y, float amount)
        {
            var jitterAmount = StaticNoise(x, y) * amount;
            var jitterAngle = StaticNoise(x, y) * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(jitterAngle), Mathf.Sin(jitterAngle)) * jitterAmount;
        }

        public abstract Task Apply(Chunk chunk, IJob job);
    }
}
