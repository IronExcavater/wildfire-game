using System.Collections.Generic;
using Generation.Data;
using Generation.Objects;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldLoader : Singleton<WorldLoader>
    {
        private float _loadRadius = 10;
        public static float LoadRadius => Instance._loadRadius;

        private float _checkInterval = 0.5f;
        public static float CheckInterval => Instance._checkInterval;

        private Dictionary<System.Type, IObjectPool> _pools = new();
        private readonly Dictionary<Vector2Int, List<DataObject<Entity>>> _instances = new();

        private Camera _camera;

        public void AddPool<T>(params T[] prefabs) where T : DataObject<Entity>
        {
            var pool = new ObjectPool<T>(10, transform, prefabs);
            _pools.Add(typeof(T), pool);
        }

        public ObjectPool<DataObject<Entity>> GetPool(System.Type type)
        {
            if (_pools.TryGetValue(type, out var pool))
                return pool as ObjectPool<DataObject<Entity>>;

            throw new System.InvalidOperationException($"No ObjectPool found for type {type.Name} in Generator.");
        }

        private void LoadChunk(Chunk chunk)
        {
            if (_instances.ContainsKey(chunk.Position)) return;

            var instances = new List<DataObject<Entity>>();

            foreach (var entity in chunk.Entities)
            {
                var type = entity.Value.Type.Value;
                var instance = GetPool(type).Get();

                instance.Data.Bind(entity);
                instances.Add(instance);
            }

            _instances.Add(chunk.Position, instances);
        }

        private void UnloadChunk(Chunk chunk)
        {
            if (!_instances.TryGetValue(chunk.Position, out var instances)) return;

            foreach (var instance in instances)
            {
                var type = instance.GetType();

                instance.Data.Unbind();
                GetPool(type).Release(instance);
            }

            _instances.Remove(chunk.Position);
        }

        public void UpdateVisibleChunks(Camera camera)
        {
            if (camera == null) return;

            var cameraPosition = camera.transform.position;
            var worldSize = WorldGenerator.WorldSize;
            var chunks = WorldGenerator.World.Chunks;
            var sqrRadius = LoadRadius * LoadRadius;

            if (chunks == null) return;

            var cameraChunkPosition = new Vector2Int(
                Mathf.FloorToInt(cameraPosition.x / worldSize.x),
                Mathf.FloorToInt(cameraPosition.z / worldSize.y)
            );

            foreach (var kvp in _instances)
            {
                if ((kvp.Key - cameraChunkPosition).sqrMagnitude > sqrRadius &&
                    chunks.TryGetValue(kvp.Key, out var chunk))
                    UnloadChunk(chunk);
            }

            for (var y = -Mathf.FloorToInt(LoadRadius); y <= Mathf.CeilToInt(LoadRadius); y++)
            for (var x = -Mathf.FloorToInt(LoadRadius); x <= Mathf.CeilToInt(LoadRadius); x++)
            {
                if (chunks.TryGetValue(new Vector2Int(x, y), out var chunk) &&
                    (chunk.Position - cameraChunkPosition).sqrMagnitude <= sqrRadius)
                    LoadChunk(chunk);
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            UpdateVisibleChunks(_camera);
        }
    }
}
