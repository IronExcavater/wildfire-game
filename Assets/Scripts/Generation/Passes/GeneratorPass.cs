using System;
using Generation.Data;
using UnityEngine;

namespace Generation.Passes
{
    [Serializable]
    public abstract class GeneratorPass
    {
        protected int GetNoiseOffset() => WorldGenerator.HashSeed(WorldGenerator.SeedString + GetType().FullName);

        protected float StaticNoise(float x, float y)
        {
            var ix = (ulong)Mathf.FloorToInt(x);
            var iy = (ulong)Mathf.FloorToInt(y);

            var hash = Mix(ix, iy, (ulong)WorldGenerator.SeedInt);
            return (hash & 0xFFFFFFFF) / (float)uint.MaxValue;
        }

        private ulong Mix(ulong a, ulong b, ulong seed)
        {
            const ulong prime1 = 0xa0761d6478bd642f;
            const ulong prime2 = 0xe7037ed1a0b428db;
            var result = (a ^ prime1) * (b ^ prime2);
            result ^= (result >> 32);
            result *= (seed ^ prime1);
            result ^= (result >> 29);
            return result;
        }

        public abstract void Apply(Chunk chunk);
    }
}
