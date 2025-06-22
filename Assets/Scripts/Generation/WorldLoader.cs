using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Objects;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldLoader : Singleton<WorldLoader>
    {
        [SerializeField, Range(1, 1000)] private float _loadRadius = 10;
        public static float LoadRadius => Instance._loadRadius;

        private float _checkInterval = 0.5f;
        public static float CheckInterval => Instance._checkInterval;

        private Dictionary<Type, IObjectPool> _pools = new();
        public IReadOnlyDictionary<Type, IObjectPool> Pools => _pools;

        [SerializeField] private GeneratorPools _generatorPools;

        private readonly Dictionary<Vector2Int, TaskCompletionSource<List<DataObject<Entity>>>> _loadTasks = new();
        private readonly Queue<Vector2Int> _loadQueue = new();

        private readonly Dictionary<Vector2Int, TaskCompletionSource<bool>> _unloadTasks = new();
        private readonly Queue<Vector2Int> _unloadQueue = new();

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

        public static Task<List<DataObject<Entity>>> GetChunk(Vector2Int position)
        {
            lock (Instance._instances) if (Instance._instances.TryGetValue(position, out var data)) return Task.FromResult(data);

            lock (Instance._loadTasks)
            lock (Instance._loadQueue)
            {
                if (Instance._loadTasks.TryGetValue(position, out var tcs)) return tcs.Task;

                tcs = new TaskCompletionSource<List<DataObject<Entity>>>();
                Instance._loadQueue.Enqueue(position);
                Instance._loadTasks.Add(position, tcs);
                return tcs.Task;
            }
        }

        public static Task<bool> RemoveChunk(Vector2Int position)
        {
            lock (Instance._instances) if (!Instance._instances.ContainsKey(position)) return Task.FromResult(true);

            lock (Instance._unloadTasks)
            lock (Instance._unloadQueue)
            {
                if (Instance._unloadTasks.TryGetValue(position, out var tcs)) return tcs.Task;

                tcs = new TaskCompletionSource<bool>();
                Instance._unloadQueue.Enqueue(position);
                Instance._unloadTasks.Add(position, tcs);
                return tcs.Task;
            }
        }

        private async void LoadChunkAsync(Vector2Int position)
        {
            //if (_instances.ContainsKey(position)) return;

            var instances = new List<DataObject<Entity>>();
            _instances[position] = instances;

            var chunk = await WorldGenerator.GetChunk(position);

            foreach (var entity in chunk.Entities)
            {
                var type = entity.Value.Type.Value;
                var pool = GetPool(type);
                var instance = (DataObject<Entity>)pool.Get();

                instances.Add(instance);
                instance.Data.Bind(entity);
            }

            if (_loadTasks.Remove(position, out var tcs)) tcs.SetResult(instances);
            Debug.Log($"Loaded chunk at {position}");
        }

        private void UnloadChunk(Vector2Int position)
        {
            if (!_instances.TryGetValue(position, out var instances)) return;

            foreach (var instance in instances)
            {
                var type = instance.GetType();

                instance.Data.Unbind();
                GetPool(type).Release(instance);
            }

            _instances.Remove(position);
            if (_unloadTasks.Remove(position, out var tcs)) tcs.SetResult(true);
            Debug.Log($"Unloaded chunk at {position}");
        }

        private void UpdateVisibleChunks(Camera camera)
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

            foreach (var position in _instances.Keys)
            {
                if ((position - cameraChunkPosition).sqrMagnitude > sqrRadius)
                    RemoveChunk(position);
            }

            for (var y = -Mathf.FloorToInt(LoadRadius); y <= Mathf.CeilToInt(LoadRadius); y++)
            for (var x = -Mathf.FloorToInt(LoadRadius); x <= Mathf.CeilToInt(LoadRadius); x++)
            {
                var position = cameraChunkPosition + new Vector2Int(x, y);
                if ((position - cameraChunkPosition).sqrMagnitude <= sqrRadius)
                    GetChunk(position);
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

            while (_loadQueue.Count > 0)
                LoadChunkAsync(_loadQueue.Dequeue());

            while (_unloadQueue.Count > 0)
                UnloadChunk(_unloadQueue.Dequeue());
        }
    }
}
