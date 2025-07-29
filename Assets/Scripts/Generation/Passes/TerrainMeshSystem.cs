using Generation.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Generation.Passes
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HillGenerationSystem))]
    public partial struct TerrainMeshSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(
                ComponentType.ReadOnly<Chunk>(),
                ComponentType.ReadOnly<TerrainHeightmap>(),
                ComponentType.Exclude<TerrainMesh>()
            );

            state.RequireForUpdate<WorldConfig>();
            state.RequireForUpdate(_query);
        }

        public void OnUpdate(ref SystemState state)
        {
            var worldConfig = SystemAPI.GetSingleton<WorldConfig>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);



            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private Mesh GenerateMesh(NativeArray<TerrainHeightmap> heightmap, int dim)
        {
            var vertices = new Vector3[dim * dim];
            var indices = new int[(dim - 1) * (dim - 1) * 6];

            for (int y = 0; y < dim; y++)
            for (int x = 0; x < dim; x++)
            {
                int index = y * dim + x;
                float height = heightmap[index].Value;
                vertices[index] = new Vector3(x, height, y);
            }

            int tri = 0;
            for (int y = 0; y < dim - 1; y++)
            for (int x = 0; x < dim - 1; x++)
            {
                int i = y * dim + x;
                indices[tri++] = i;
                indices[tri++] = i + dim;
                indices[tri++] = i + 1;

                indices[tri++] = i + 1;
                indices[tri++] = i + dim;
                indices[tri++] = i + dim + 1;
            }

            var mesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
                triangles = indices,
                vertices = vertices,
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
