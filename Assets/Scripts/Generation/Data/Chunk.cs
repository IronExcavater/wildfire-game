using System;
using System.Collections.Generic;
using System.Linq;
using Generation.Objects;
using Generation.Passes;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Data
{
    public sealed class Chunk : IDisposable, IObservable<Chunk, ValueChange<Chunk>>
    {
        public Vector2Int Position;
        public Vector2 WorldPosition => new Vector2();//WorldGenerator.ChunkToWorld(Position, 32);

        public readonly ObservableList<Property<Entity>> Entities = new();

        public event Action<ValueChange<Chunk>> OnChanged;
        public void InvokeOnChanged() => OnChanged?.Invoke(new ValueChange<Chunk>(this, this));

        private readonly object _entityLock = new();

        /*public Chunk(World world, Vector2Int position)
        {
            InitializeListeners();
            Position = position;
        }*/

        private void InitializeListeners()
        {
            Entities.AddListener((_, _) => InvokeOnChanged());
        }

        public void AddEntity(Property<Entity> entity)
        {
            lock (_entityLock) Entities.Add(entity);
        }

        public bool TryGetEntitiesOfType<T>(out List<Property<Entity>> entities)
        {
            lock (_entityLock)
            {
                entities = Entities.ReadOnly.Where(entity => entity.Value.Type.Value == typeof(T)).ToList();
                return entities.Count > 0;
            }
        }

        public bool TryGetEntityOfType<T>(out Property<Entity> entity) where T : DataObject<Entity>
        {
            lock (_entityLock)
            {
                entity = Entities.ReadOnly.FirstOrDefault(entity => entity.Value.Type.Value == typeof(T));
                return entity != null;
            }
        }

        /*public Property<float[,]> GetHeightmap()
        {
            var chunkSize = WorldGenerator.ChunkSize;
            var size = chunkSize * WorldGenerator.Resolution;

            if (!TryGetEntityOfType<TerrainObject>(out var terrain))
            {
                terrain = new Property<Entity>(new Entity(typeof(TerrainObject), this));
                terrain.Value.Position.Value = WorldPosition;
                AddEntity(terrain);
            }

            var dim = size + 1;
            if (!terrain.Value.TryGetProperty("Heightmap", out Property<float[,]> heightmap) ||
                heightmap.Value.Length != dim * dim || heightmap.Value.Rank != 2)
            {
                heightmap = new Property<float[,]>(new float[dim, dim]);
                terrain.Value.SetProperty("Heightmap", heightmap);
            }

            return heightmap;
        }*/

        private GenerationStage _completedStage = GenerationStage.None;
        public GenerationStage CompletedStage => _completedStage;

        public bool IsStageComplete(GenerationStage stage) => _completedStage >= stage;

        public void MarkStageComplete(GenerationStage stage)
        {
            if (stage > _completedStage) _completedStage = stage;
        }

        public void Dispose()
        {
            lock (_entityLock)
            {
                //_ = WorldLoader.RemoveChunk(Position);
                foreach (var entity in Entities)
                    entity.Value.Dispose();
                Entities.ClearListeners();
                Entities.Clear();
            }
        }
    }
}
