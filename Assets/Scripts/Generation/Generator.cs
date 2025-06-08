using System.Collections.Generic;
using Generation.Data;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class Generator : Singleton<Generator>
    {
        [SerializeField] private Vector2Int worldSize;
        private World _world;

        private readonly List<IGeneratorPass> _passes = new();

        private Dictionary<System.Type, object> _pools = new();
        private readonly Dictionary<Vector2Int, List<DataObject<Entity>>> _instances = new();

        public void AddPass(IGeneratorPass pass) => _passes.Add(pass);

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

        public void LoadChunk(Chunk chunk)
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

        public void UnloadChunk(Chunk chunk)
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


        protected override void Awake()
        {
            _world = new World(worldSize.x, worldSize.y, _passes.ToArray());
        }
    }
}
