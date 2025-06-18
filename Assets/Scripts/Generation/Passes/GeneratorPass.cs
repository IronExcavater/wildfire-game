using System;
using Generation.Data;

namespace Generation.Passes
{
    [Serializable]
    public abstract class GeneratorPass
    {
        public abstract void Apply(World world);
    }
}
