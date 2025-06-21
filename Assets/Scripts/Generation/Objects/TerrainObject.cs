using System;
using System.Collections.Generic;
using Generation.Data;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Objects
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainObject : DataObject<Entity>
    {
        private readonly Dictionary<int, Mesh> _meshes = new();
        private int _lod = -1;

        private int Lod
        {
            get => _lod;
            set
            {
                if (value == _lod) return;
                _lod = value;

                _meshFilter.sharedMesh = GenerateMesh(_lod);
            }
        }

        private Camera _camera;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private readonly Property<Vector3> _position = new();
        private readonly Property<float[,]> _heightmap = new();

        private const float SkirtDepth = 1;
        private const float SkirtOffset = 0.25f;

        protected override void Awake()
        {
            base.Awake();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _position.AddListener((_, change) => transform.position = change.NewValue);
            _heightmap.AddListener((_, _) =>
            {
                _meshes.Clear();
                _meshFilter.sharedMesh = GenerateMesh(Lod);
            });
        }

        protected override void OnDataChanged(PropertyBase<Entity, Entity, ValueChange<Entity>> property, ValueChange<Entity> change)
        {
            _position.BindBidirectional(change.NewValue.Position);

            if (change.NewValue.TryGetProperty("Heightmap", out Property<float[,]> heightmap))
                _heightmap.BindBidirectional(heightmap);
        }

        private Mesh GenerateMesh(int lod)
        {
            if (_meshes.TryGetValue(lod, out var mesh)) return mesh;

            var chunkSize = WorldGenerator.ChunkSize;
            var resolution = WorldGenerator.Resolution;
            var step = 1 << lod;

            var countX = (_heightmap.Value.GetLength(0) - 1) / step;
            var countY = (_heightmap.Value.GetLength(1) - 1) / step;

            var width = countX + 1;
            var height = countY + 1;

            var vertexCount = width * height;
            var skirtVertexCount = 4 * (width + height - 2);

            var vertices = new Vector3[vertexCount + skirtVertexCount];
            var uvs = new Vector2[vertices.Length];
            var normals = new Vector3[vertices.Length];
            var triangles = new int[countX * countY * 6 + skirtVertexCount * 3];

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var stepX = x * step;
                var stepY = y * step;

                var localX = stepX / (float)resolution;
                var localY = stepY / (float)resolution;

                var worldPos = new Vector2(localX + _position.Value.x, localY + _position.Value.z);

                var i = y * width + x;
                var h = WorldGenerator.World.GetHeight(worldPos);
                //var h = WorldGenerator.World.GetAverageHeight(new Vector2(localX + _position.Value.x, localY + _position.Value.z), step);
                vertices[i] = new Vector3(localX, h, localY);
                uvs[i] = worldPos;
                normals[i] = WorldGenerator.World.GetNormal(worldPos);
            }

            var tri = 0;

            for (var y = 0; y < countY; y++)
            for (var x = 0; x < countX; x++)
            {
                var i = y * width + x;

                triangles[tri++] = i;
                triangles[tri++] = i + width;
                triangles[tri++] = i + width + 1;

                triangles[tri++] = i;
                triangles[tri++] = i + width + 1;
                triangles[tri++] = i + 1;
            }

            var v = vertexCount;

            void AddSkirt(int i1, int i2)
            {
                var v1 = vertices[i1];
                var v2 = vertices[i2];

                var vertexCenter = new Vector2(chunkSize / 2f, chunkSize / 2f);
                var d1 = (new Vector2(v1.x, v1.z) - vertexCenter).normalized;
                var d2 = (new Vector2(v2.x, v2.z) - vertexCenter).normalized;

                vertices[v] = new Vector3(
                    v1.x + d1.x * SkirtOffset,
                    v1.y - SkirtDepth,
                    v1.z + d1.y * SkirtOffset
                );
                normals[v] = normals[i1];
                uvs[v] = uvs[i1];

                triangles[tri++] = i1;
                triangles[tri++] = i2;
                triangles[tri++] = v;
                v++;

                vertices[v] = new Vector3(
                    v2.x + d2.x * SkirtOffset,
                    v2.y - SkirtDepth,
                    v2.z + d2.y * SkirtOffset
                );
                normals[v] = normals[i2];
                uvs[v] = uvs[i2];

                triangles[tri++] = i2;
                triangles[tri++] = v;
                triangles[tri++] = v - 1;
                v++;
            }

            for (var x = 0; x < width - 1; x++)
                AddSkirt(x, x + 1);
            for (var y = 0; y < height - 1; y++)
                AddSkirt(y * width + (width - 1), (y + 1) * width + (width - 1));
            for (var x = width - 1; x > 0; x--)
                AddSkirt((height - 1) * width + x, (height - 1) * width + x - 1);
            for (var y = height - 1; y > 0; y--)
                AddSkirt(y * width, (y - 1) * width);

            mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                triangles = triangles,
                normals = normals
            };

            _meshes[lod] = mesh;
            return mesh;
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;

            Lod = GetLod(_camera);
        }

        private int GetLod(Camera camera)
        {
            var maxLod = WorldGenerator.MaxLodLevel;
            if (camera == null) return maxLod;

            var distance = Vector3.Distance(_camera.transform.position, transform.position);
            var chunkSize = WorldGenerator.ChunkSize;
            if (distance < chunkSize) return 0;

            var lod = Mathf.FloorToInt(Mathf.Log(distance / chunkSize, 2));
            return Math.Clamp(lod, 0, maxLod);
        }
    }
}
