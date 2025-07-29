using Generation.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Generation.Passes
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ChunkStreamingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldConfig>();
            state.RequireForUpdate<Camera>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var worldConfig = SystemAPI.GetSingleton<WorldConfig>();
            var chunkLookupSystem = state.World.GetExistingSystemManaged<ChunkLookupSystem>();
            var camera = SystemAPI.GetSingleton<Camera>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var sqrRadius = worldConfig.RenderDistance * worldConfig.RenderDistance;

            var cameraWorld = new float2(camera.Position.x, camera.Position.z);
            var cameraChunk = (int2)math.floor(cameraWorld) / worldConfig.ChunkSize;

            foreach (var kvp in chunkLookupSystem.ChunkMap)
            {
                var pos = kvp.Key;
                if (math.lengthsq(pos - cameraChunk) <= sqrRadius) continue;

                var chunk = kvp.Value;
                if (state.EntityManager.Exists(chunk)) ecb.DestroyEntity(chunk);
                chunkLookupSystem.RemoveChunk(pos);
            }

            for (var y = -worldConfig.RenderDistance; y <= worldConfig.RenderDistance; y++)
            for (var x = -worldConfig.RenderDistance; x <= worldConfig.RenderDistance; x++)
            {
                var pos = cameraChunk + new int2(x, y);
                if (math.lengthsq(pos - cameraChunk) > sqrRadius) continue;

                var chunk = ecb.CreateEntity();
                ecb.AddComponent(chunk, new Chunk { Position = pos });
                chunkLookupSystem.AddChunk(pos, chunk);
            }
        }
    }
}
