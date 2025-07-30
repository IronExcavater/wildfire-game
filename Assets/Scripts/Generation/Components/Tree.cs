using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Generation.Passes
{
    public class TreeMeshAuthoring : MonoBehaviour
    {
        public Mesh[] meshes;
        public Material[] materials;

        private class TreeMeshBaker : Baker<TreeMeshAuthoring>
        {
            public override void Bake(TreeMeshAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);

                var meshArray = new RenderMeshArray(authoring.materials, authoring.meshes);

                AddSharedComponentManaged(entity, meshArray);

                AddComponent(entity, new MeshInfo
                {
                    MaterialIndex = 0,
                    MeshIndex = 0
                });
            }
        }
    }

    public struct MeshInfo : IComponentData
    {
        public int MaterialIndex;
        public int MeshIndex;
    }
}

