using System;
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

        public override async Task Start()
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

        public override int CompareTo(IJob other)
        {
            var cmp = base.CompareTo(other);
            return other is BuildTerrainJob btj && cmp == 0
                ? Lod.CompareTo(btj.Lod)
                : cmp;
        }

        public override bool Equals(object obj) =>
            obj is BuildTerrainJob other &&
            base.Equals(other) &&
            Lod.Equals(other.Lod);
        public override int GetHashCode() => HashCode.Combine(Type, Position, Lod);

        public override string ToString() => $"{Type} {{ lod: {Lod} }} job at {Position}";
    }
}
