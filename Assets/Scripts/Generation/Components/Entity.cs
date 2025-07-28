using Unity.Entities;
using Unity.Mathematics;

namespace Generation.Components
{
    public struct LocalTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
        public float Scale;
    }
}
