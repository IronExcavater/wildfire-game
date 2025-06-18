using System.Collections.Generic;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Data
{
    public sealed class Chunk
    {
        public Vector2Int Position;
        public readonly List<Property<Entity>> Entities = new();

        public Chunk(Vector2Int position)
        {
            Position = position;
        }

        public void AddEntity(Property<Entity> entity)
        {
            Entities.Add(entity);
        }

        public List<Property<Entity>> GetEntitiesOfType(System.Type type)
        {
            return Entities.FindAll(entity => entity.Value.Type.Value == type);
        }

        public Property<Entity> GetEntityOfType(System.Type type)
        {
            return Entities.Find(entity => entity.Value.Type.Value == type);
        }

        public bool TryGetEntitiesOfType(System.Type type, out List<Property<Entity>> entities)
        {
            entities = Entities.FindAll(entity => entity.Value.Type.Value == type);
            return entities.Count > 0;
        }

        public bool TryGetEntityOfType(System.Type type, out Property<Entity> entity)
        {
            entity = Entities.Find(entity => entity.Value.Type.Value == type);
            return entity != null;
        }
    }
}
