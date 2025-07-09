using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Generation.Objects;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Data
{
    public sealed class Chunk : IDisposable, IObservable<Chunk, ValueChange<Chunk>>
    {
        public Vector2Int Position;
        public readonly ObservableList<Property<Entity>> Entities = new();

        public event Action<ValueChange<Chunk>> OnChanged;

        public Chunk(Vector2Int position)
        {
            InitializeListeners();
            Position = position;
        }

        private void InitializeListeners()
        {
            Entities.AddListener((_, _) => InvokeOnChanged());
        }

        private void InvokeOnChanged()
        {
            OnChanged?.Invoke(new ValueChange<Chunk>(this, this));
        }

        public void AddEntity(Property<Entity> entity)
        {
            Entities.Add(entity);
        }

        public List<Property<Entity>> GetEntitiesOfType(Type type)
        {
            return Entities.ReadOnly.Where(entity => entity.Value.Type.Value == type).ToList();
        }

        public Property<Entity> GetEntityOfType(Type type)
        {
            return Entities.ReadOnly.FirstOrDefault(entity => entity.Value.Type.Value == type);
        }

        public bool TryGetEntitiesOfType(Type type, out List<Property<Entity>> entities)
        {
            entities = Entities.ReadOnly.Where(entity => entity.Value.Type.Value == type).ToList();
            return entities.Count > 0;
        }

        public bool TryGetEntityOfType(Type type, out Property<Entity> entity)
        {
            entity = Entities.ReadOnly.FirstOrDefault(entity => entity.Value.Type.Value == type);
            return entity != null;
        }

        public Property<float[,]> GetHeightmap()
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var size = chunkSize * WorldGenerator.Resolution;

            if (!TryGetEntityOfType(typeof(TerrainObject), out var terrain))
            {
                terrain = new Property<Entity>(new Entity(typeof(TerrainObject), this));
                terrain.Value.Position.Value = new Vector3(Position.x * chunkSize, 0, Position.y * chunkSize);
                AddEntity(terrain);
            }

            if (!terrain.Value.TryGetProperty("Heightmap", out Property<float[,]> heightmap) ||
                heightmap.Value.Length != (int)Math.Pow(size + 1, 2) || heightmap.Value.Rank != 2)
            {
                heightmap = new Property<float[,]>(new float[size + 1, size + 1]);
                terrain.Value.SetProperty("Heightmap", heightmap);
            }

            return heightmap;
        }

        public void Dispose()
        {
            _ = WorldLoader.RemoveChunk(Position);
            foreach (var entity in Entities)
                entity.Value.Dispose();
            Entities.Clear();
        }
    }
}
