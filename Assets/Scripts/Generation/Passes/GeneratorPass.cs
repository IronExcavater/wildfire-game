using System;
using Generation.Data;
using UnityEngine;

namespace Generation.Passes
{
    [Serializable]
    public abstract class GeneratorPass
    {
        protected int GetNoiseOffset() => WorldGenerator.HashSeed(WorldGenerator.SeedString + GetType().FullName);

        protected float NormalizedHash(float x, float y, int salt = 0)
        {
            // Convert float to fixed-point integers to ensure determinism
            var ix = Mathf.FloorToInt(x * 1000f); // 3 decimal precision
            var iy = Mathf.FloorToInt(y * 1000f);
            var hash = HashInts(ix, iy, salt);
            return (hash & 0x7FFFFFFF) / (float)int.MaxValue;
        }

        private int HashInts(int a, int b, int salt = 0)
        {
            unchecked
            {
                var hash = (uint)((a + salt) * 73856093) ^ (uint)((b + salt) * 19349663);
                return (int)(hash & 0x7FFFFFFF);
            }
        }

        public abstract void Apply(Chunk chunk);
    }
}
