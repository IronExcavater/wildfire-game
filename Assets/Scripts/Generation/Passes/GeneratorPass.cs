using System;
using Generation.Data;

namespace Generation.Passes
{
    [Serializable]
    public abstract class GeneratorPass
    {
        protected int GetNoiseOffset() => WorldGenerator.HashSeed(WorldGenerator.SeedString + GetType().FullName);

        public abstract void Apply(Chunk chunk);
    }
}
