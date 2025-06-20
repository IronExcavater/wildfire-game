using System;
using System.Collections.Generic;
using Generation.Data;
using UnityEngine;
using Utilities;
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

                _meshFilter.sharedMesh = GenerateMesh(_heightmap.Value, _lod);
            }
        }

        private Camera _camera;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private readonly Property<Vector3> _position = new();
        private readonly Property<float[,]> _heightmap = new();

        protected override void Awake()
        {
            base.Awake();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _position.AddListener((_, change) => transform.position = change.NewValue);
            _heightmap.AddListener((_, _) =>
            {
                _meshes.Clear();
                _meshFilter.sharedMesh = GenerateMesh(_heightmap.Value, Lod);
            });
        }

        protected override void OnDataChanged(PropertyBase<Entity, Entity, ValueChange<Entity>> property, ValueChange<Entity> change)
        {
            _position.BindBidirectional(change.NewValue.Position);

            if (change.NewValue.TryGetProperty("Heightmap", out Property<float[,]> heightmap))
                _heightmap.BindBidirectional(heightmap);
        }

        private Mesh GenerateMesh(float[,] heightmap, int lod)
        {
            if (_meshes.TryGetValue(lod, out var mesh)) return mesh;

            var resolution = WorldGenerator.Resolution;
            var step = 1 << lod;

            var countX = (heightmap.GetLength(0) - 1) / step;
            var countY = (heightmap.GetLength(1) - 1) / step;

            var width = countX + 1;
            var height = countY + 1;

            var vertices = new Vector3[width * height];
            var uvs = new Vector2[width * height];
            var triangles = new int[(width - 1) * (height - 1) * 6];

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var stepX = x * step;
                var stepY = y * step;

                var worldX = stepX / (float)resolution;
                var worldY = stepY / (float)resolution;

                var i = y * width + x;
                var h = AverageHeight(heightmap, stepX, stepY, step);
                vertices[i] = new Vector3(worldX, h, worldY);
                uvs[i] = new Vector2(x / (float)(width - 1), y / (float)(height - 1));
            }

            var ti = 0;
            for (var y = 0; y < countY; y++)
            for (var x = 0; x < countX; x++)
            {
                var i = y * width + x;

                triangles[ti++] = i;
                triangles[ti++] = i + width;
                triangles[ti++] = i + width + 1;

                triangles[ti++] = i;
                triangles[ti++] = i + width + 1;
                triangles[ti++] = i + 1;
            }

            mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                triangles = triangles
            };
            mesh.RecalculateNormals();

            _meshes[lod] = mesh;
            return mesh;
        }

        private float AverageHeight(float[,] heightmap, int x, int y, int radius)
        {
            var average = 0d;
            var count = 0;

            for (var dy = -radius; dy <= radius; dy++)
            for (var dx = -radius; dx <= radius; dx++)
            {
                var px = Math.Clamp(x + dx, 0, heightmap.GetLength(0) - 1);
                var py = Math.Clamp(y + dy, 0, heightmap.GetLength(1) - 1);
                Utils.AddValueToAverage(ref average, ref count, heightmap[px, py]);
            }

            return (float)average;
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
