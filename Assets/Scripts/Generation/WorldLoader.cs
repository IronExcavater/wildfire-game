using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Generation.Data;
using Generation.Jobs;
using Generation.Objects;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class WorldLoader : Singleton<WorldLoader>
    {
        [SerializeField, Range(1, 1000)] private float _loadRadius = 10;
        public static float LoadRadius => Instance._loadRadius;

        private Dictionary<Type, IObjectPool> _pools = new();
        public static IReadOnlyDictionary<Type, IObjectPool> Pools => Instance._pools;

        [SerializeField] private GeneratorPools _generatorPools;

        private readonly Dictionary<Vector2Int, List<DataObject<Entity>>> _instances = new();
        public static IReadOnlyDictionary<Vector2Int, List<DataObject<Entity>>> Instances => Instance._instances;

        private Camera _camera;

        public static void AddPool<T>(params T[] prefabs) where T : DataObject<Entity>
        {
            var pool = new ObjectPool<T>(10, Instance.transform, prefabs);
            Instance._pools.Add(typeof(T), pool);
        }

        public static ObjectPool<T> GetPool<T>() where T : MonoBehaviour
        {
            var type = typeof(T);
            if (GetPool(type) is ObjectPool<T> pool)
                return pool;

            throw new InvalidOperationException(
                $"Malformed ObjectPool: Key `{type.FullName}` exists but value is not ObjectPool<{type.Name}>. " +
                $"The registered pool type does not match the expected generic argument."
            );
        }

        public static IObjectPool GetPool(Type type)
        {
            if (Instance._pools.TryGetValue(type, out var pool))
                return pool;

            throw new InvalidOperationException($"ObjectPool<{type.Name}> not found");
        }

        public static List<DataObject<Entity>> GetInstancesAtPosition(Vector2Int position)
        {
            return Instance._instances[position];
        }

        public static bool TryGetInstancesAtPosition(Vector2Int position, out List<DataObject<Entity>> instances)
        {
            return Instance._instances.TryGetValue(position, out instances);
        }

        public static List<DataObject<Entity>> GetInstancesOfTypeAtPosition(Vector2Int position, Type type)
        {
            return GetInstancesAtPosition(position).FindAll(instance => instance.GetType() == type);
        }

        public static DataObject<Entity> GetInstanceOfTypeAtPosition(Vector2Int position, Type type)
        {
            return GetInstancesAtPosition(position).Find(instance => instance.GetType() == type);
        }

        public static bool TryGetInstancesOfTypeAtPosition(Vector2Int position, Type type, out List<DataObject<Entity>> instances)
        {
            instances = GetInstancesAtPosition(position).FindAll(instance => instance.GetType() == type);
            return instances.Count > 0;
        }

        public static bool TryGetInstanceOfTypeAtPosition(Vector2Int position, Type type, out DataObject<Entity> instances)
        {
            instances = GetInstancesAtPosition(position).Find(instance => instance.GetType() == type);
            return instances != null;
        }

        public static async Task<List<DataObject<Entity>>> GetChunk(Vector2Int position)
        {
            if (!TryGetInstancesAtPosition(position, out var instances))
            {
                instances = await JobManager.Enqueue(new LoadChunkJob(position));
                Instance._instances[position] = instances;
            }

            return instances;
        }

        public static async Task<bool> RemoveChunk(Vector2Int position)
        {
            if (TryGetInstancesAtPosition(position, out _))
            {
                await JobManager.Enqueue(new UnloadChunkJob(position));
                Instance._instances.Remove(position);
            }

            return true;
        }

        private void UpdateVisibleChunks(Camera camera)
        {
            if (camera == null) return;

            var chunks = WorldGenerator.World.Chunks;
            var sqrRadius = LoadRadius * LoadRadius;

            if (chunks == null) return;

            var cameraChunkPosition = CameraChunkPosition();

            foreach (var position in _instances.Keys)
            {
                if ((position - cameraChunkPosition).sqrMagnitude > sqrRadius)
                    _ = RemoveChunk(position);
            }

            for (var y = -Mathf.FloorToInt(LoadRadius); y <= Mathf.CeilToInt(LoadRadius); y++)
            for (var x = -Mathf.FloorToInt(LoadRadius); x <= Mathf.CeilToInt(LoadRadius); x++)
            {
                var position = cameraChunkPosition + new Vector2Int(x, y);
                if ((position - cameraChunkPosition).sqrMagnitude <= sqrRadius)
                    _ = GetChunk(position);
            }
        }

        public static Vector3 CameraPosition() => Instance._camera?.transform.position ?? Vector3.zero;

        public static Vector2Int CameraChunkPosition()
        {
            if (Instance._camera == null) return new Vector2Int(int.MaxValue, int.MaxValue);

            var cameraPosition = Instance._camera.transform.position;
            return new Vector2Int(
                Mathf.FloorToInt(cameraPosition.x / WorldGenerator.ChunkSize),
                Mathf.FloorToInt(cameraPosition.z / WorldGenerator.ChunkSize)
            );
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
