using Unity.Entities;
using Unity.Mathematics;

namespace Generation.Components
{
    public struct Chunk : IComponentData
    {
        public int2 Position;
    }
}
