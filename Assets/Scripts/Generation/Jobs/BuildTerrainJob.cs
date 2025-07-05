using System.Threading.Tasks;
using Generation.Objects;
using UnityEngine;

namespace Generation.Jobs
{
    public class BuildTerrainJob : JobBase<Mesh>
    {
        public int Lod { get; }

        public BuildTerrainJob(Vector2Int position, int lod) : base(ChunkJobType.BuildTerrain, position)
        {
            Lod = lod;
        }

        public override async Task ExecuteAsync()
        {
            var instance = (TerrainObject)WorldLoader.GetInstanceOfTypeAtPosition(Position, typeof(TerrainObject));

            var meshData = await instance.GenerateMeshAsync(Lod, CancelSource.Token);
            var mesh = new Mesh
            {
                vertices = meshData.vertices,
                uv = meshData.uvs,
                normals = meshData.normals,
                triangles = meshData.triangles
            };

            CompleteSource.SetResult(mesh);
        }
    }
}
