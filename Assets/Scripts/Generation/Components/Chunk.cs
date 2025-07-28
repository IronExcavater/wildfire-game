using Unity.Entities;
using Unity.Mathematics;

namespace Generation.Components
{
    public struct Chunk : IComponentData
    {
        public int2 Position;
    }

    public static class ChunkPositionExtensions
    {
        public static float2 ToWorldPosition(this Chunk chunk, int chunkSize)
        {
            return (float2)chunk.Position * chunkSize;
        }
    }
}
