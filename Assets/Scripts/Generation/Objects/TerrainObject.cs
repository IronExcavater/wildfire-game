using Generation.Data;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Generation.Objects
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainObject : DataObject<Entity>
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private readonly ValueProperty<Vector3> _position = new();
        private readonly ValueProperty<float[,]> _heightmap = new();

        protected override void Awake()
        {
            base.Awake();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _position.AddListener((_, change) => transform.position = change.NewValue);
            _heightmap.AddListener((_, change) => GenerateTerrainMesh(change.NewValue));
        }

        protected override void OnDataChanged(PropertyBase<Entity, Entity, ValueChange<Entity>> property, ValueChange<Entity> change)
        {
            _position.BindBidirectional(change.NewValue.Position);

            if (change.NewValue.TryGetProperty("Heightmap", out ValueProperty<float[,]> heightmap))
                _heightmap.BindBidirectional(heightmap);
        }

        private void GenerateTerrainMesh(float[,] heightmap)
        {
            var width = heightmap.GetLength(0);
            var height = heightmap.GetLength(1);

            var vertices = new Vector3[width * height];
            var uvs = new Vector2[width * height];
            var triangles = new int[(width - 1) * (height - 1) * 6];

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var i = y * width + x;
                var h = heightmap[x, y];
                vertices[i] = new Vector3(x, h, y);
                uvs[i] = new Vector2(x / (float)(width - 1), y / (float)(height - 1));
            }

            var ti = 0;
            for (var y = 0; y < height - 1; y++)
            for (var x = 0; x < width - 1; x++)
            {
                var i = y * width + x;

                triangles[ti++] = i;
                triangles[ti++] = i + width;
                triangles[ti++] = i + width + 1;

                triangles[ti++] = i;
                triangles[ti++] = i + width + 1;
                triangles[ti++] = i + 1;
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                triangles = triangles
            };

            mesh.RecalculateNormals();
            _meshFilter.sharedMesh = mesh;
        }
    }
}
