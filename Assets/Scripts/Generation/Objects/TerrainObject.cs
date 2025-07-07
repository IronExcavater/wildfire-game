using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
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
                _ = SetMesh(Lod);
            }
        }

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;

        private readonly Property<Chunk> _chunk = new();
        private readonly Property<Vector3> _position = new();
        private readonly Property<float[,]> _heightmap = new();

        private const float SkirtDepth = 1;
        private const float SkirtOffset = 0.25f;

        protected override void Awake()
        {
            base.Awake();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            Lod = GetLod();
            _position.AddListener((_, change) => transform.position = change.NewValue);
            _heightmap.AddListener((_, _) =>
            {
                _meshes.Clear();
                _ = SetMesh(Lod);
            });
        }

        private void Update()
        {
            Lod = GetLod();
        }

        protected override void OnDataChanged(PropertyBase<Entity, Entity, ValueChange<Entity>> property, ValueChange<Entity> change)
        {
            _chunk.BindBidirectional(change.NewValue.Chunk);
            _position.BindBidirectional(change.NewValue.Position);

            if (change.NewValue.TryGetProperty("Heightmap", out Property<float[,]> heightmap))
                _heightmap.BindBidirectional(heightmap);
        }

        public async Task SetMesh(int lod)
        {
            if (!_meshes.TryGetValue(lod, out var mesh))
            {
                mesh = await JobManager.Enqueue(new BuildTerrainJob(_chunk.Value.Position, lod));
                _meshes[lod] = mesh;
            }

            if (Lod == lod) _meshFilter.sharedMesh = mesh;
            else _ = SetMesh(Lod);
        }

        public async Task<(Vector3[] vertices, Vector2[] uvs, Vector3[] normals, int[] triangles)>
            GenerateMeshAsync(int lod, CancellationToken token)
        {
            return await Task.Run(async () =>
            {
                token.ThrowIfCancellationRequested();

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
                    token.ThrowIfCancellationRequested();
                    var stepX = x * step;
                    var stepY = y * step;

                    var localX = stepX / (float)resolution;
                    var localY = stepY / (float)resolution;

                    var worldPos = new Vector2(localX + _position.Value.x, localY + _position.Value.z);

                    var i = y * width + x;
                    var h = await WorldGenerator.World.GetHeight(worldPos);
                    //var h = WorldGenerator.World.GetAverageHeight(new Vector2(localX + _position.Value.x, localY + _position.Value.z), step);
                    vertices[i] = new Vector3(localX, h, localY);
                    uvs[i] = worldPos;
                    normals[i] = await WorldGenerator.World.GetNormal(worldPos);
                }

                var tri = 0;

                for (var y = 0; y < countY; y++)
                for (var x = 0; x < countX; x++)
                {
                    token.ThrowIfCancellationRequested();
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
                    token.ThrowIfCancellationRequested();
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
                        v2.x + d2.x * SkirtDepth * SkirtOffset,
                        v2.y - SkirtDepth,
                        v2.z + d2.y * SkirtDepth * SkirtOffset
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

                return (vertices, uvs, normals, triangles);
            }, token);
        }

        private int GetLod()
        {
            var maxLod = WorldGenerator.MaxLodLevel;
            var chunkSize = WorldGenerator.ChunkSize;

            var distance = Vector3.Distance(WorldLoader.CameraPosition(), transform.position);
            if (distance < chunkSize) return 0;

            var lod = Mathf.FloorToInt(Mathf.Log(distance / chunkSize, 2));
            return Math.Clamp(lod, 0, maxLod);
        }
    }
}
