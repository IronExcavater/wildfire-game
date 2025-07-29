using Generation.Components;
using Unity.Entities;

namespace Generation.Passes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct TerrainRendererSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(
                ComponentType.ReadOnly<HillGeneratedTag>(),
                ComponentType.ReadOnly<TerrainMesh>()
            );
        }

        public void OnUpdate(ref SystemState state)
        {

        }
    }
}
