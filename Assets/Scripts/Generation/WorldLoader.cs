using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private Dictionary<Type, IObjectPool> _pools = new();
        public IReadOnlyDictionary<Type, IObjectPool> Pools => _pools;

        [SerializeField] private GeneratorPools _generatorPools;

        private readonly Dictionary<Vector2Int, List<DataObject<Entity>>> _instances = new();

        private Camera _camera;

        public void AddPool<T>(params T[] prefabs) where T : DataObject<Entity>
        {
            var pool = new ObjectPool<T>(10, transform, prefabs);
            _pools.Add(typeof(T), pool);
        }

        public ObjectPool<T> GetPool<T>() where T : MonoBehaviour
        {
            var type = typeof(T);
            if (GetPool(type) is ObjectPool<T> pool)
                return pool;

            throw new InvalidOperationException(
                $"Malformed ObjectPool: Key `{type.FullName}` exists but value is not ObjectPool<{type.Name}>. " +
                $"The registered pool type does not match the expected generic argument."
            );
        }

        public IObjectPool GetPool(Type type)
        {
            if (_pools.TryGetValue(type, out var pool))
                return pool;

            throw new InvalidOperationException($"ObjectPool<{type.Name}> not found");
        }

        private void LoadChunk(Vector2Int position)
        {
            if (_instances.ContainsKey(position)) return;

            var chunk = WorldGenerator.World.GetChunk(position);
            var instances = new List<DataObject<Entity>>();

            foreach (var entity in chunk.Entities)
            {
                var type = entity.Value.Type.Value;
                var pool = GetPool(type);
                var instance = (DataObject<Entity>)pool.Get();

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
            var chunks = WorldGenerator.World.Chunks;
            var sqrRadius = LoadRadius * LoadRadius;

            if (chunks == null) return;

            var cameraChunkPosition = new Vector2Int(
                Mathf.FloorToInt(cameraPosition.x / WorldGenerator.ChunkSize),
                Mathf.FloorToInt(cameraPosition.z / WorldGenerator.ChunkSize)
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
                var pos = cameraChunkPosition + new Vector2Int(x, y);
                if ((pos - cameraChunkPosition).sqrMagnitude <= sqrRadius)
                    LoadChunk(pos);
            }
        }

        private (Type, IObjectPool) TransformEntry(string key, List<GameObject> value)
        {
            var type = Type.GetType(key);
            if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type))
                throw new Exception($"{key} is not a valid MonoBehaviour type");

            var poolType = typeof(ObjectPool<>).MakeGenericType(type);
            var prefabArrayType = type.MakeArrayType();
            var paramTypes = new[] { typeof(int), typeof(Transform), prefabArrayType };

            var constructor = poolType.GetConstructor(paramTypes);
            if (constructor == null)
            {
                var paramNames = string.Join(", ", paramTypes.Select(t => t.FullName));
                throw new Exception($"No constructor found for type {type.Name} with parameters of ({paramNames})");
            }

            var convertedPrefabs = Array.CreateInstance(type, value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                var component = value[i].GetComponent(type);
                if (component == null)
                    throw new Exception($"GameObject `{value[i].name}` is missing component `{type.Name}`");
                convertedPrefabs.SetValue(component, i);
            }

            var poolInstance = constructor.Invoke(new object[] { 10, transform, convertedPrefabs });
            Debug.Log($"Created pool instance: {poolInstance}");
            return (type, (IObjectPool)poolInstance);
        }

        protected override void Awake()
        {
            base.Awake();
            if (_generatorPools)
            {
                foreach (var kvp in _generatorPools.pools.Dictionary)
                {
                    var (type, objectPool) = TransformEntry(kvp.Key, kvp.Value);
                    _pools[type] = objectPool;
                }
            }
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            UpdateVisibleChunks(_camera);
        }
    }
}
