using System;
using Generation.Data;

namespace Generation
{
    [Serializable]
    public abstract class GeneratorPass
    {
        public abstract void Apply(World world);
    }
}
