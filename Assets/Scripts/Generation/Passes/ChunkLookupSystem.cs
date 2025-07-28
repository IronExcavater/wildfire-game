using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Generation.Passes
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class ChunkLookupSystem : SystemBase
    {
        public NativeParallelHashMap<int2, Entity> ChunkMap;

        protected override void OnCreate()
        {
            ChunkMap = new NativeParallelHashMap<int2, Entity>(1024, Allocator.Persistent);
        }

        protected override void OnUpdate() { }

        protected override void OnDestroy()
        {
            if (ChunkMap.IsCreated) ChunkMap.Dispose();
        }

        public bool TryGetChunk(int2 pos, out Entity entity) => ChunkMap.TryGetValue(pos, out entity);
        public void AddChunk(int2 pos, Entity entity) => ChunkMap[pos] = entity;
        public void RemoveChunk(int2 pos) => ChunkMap.Remove(pos);
    }
}
