using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Generation.Data
{
    public sealed class Chunk
    {
        public Vector2Int Position;
        public readonly List<ValueProperty<Entity>> Entities = new();

        public Chunk(Vector2Int position)
        {
            Position = position;
        }

        public void AddEntity(ValueProperty<Entity> entity)
        {
            Entities.Add(entity);
        }

        public List<ValueProperty<Entity>> GetEntitiesOfType(System.Type type)
        {
            return Entities.FindAll(entity => entity.Value.Type.Value == type);
        }

        public ValueProperty<Entity> GetEntityOfType(System.Type type)
        {
            return Entities.Find(entity => entity.Value.Type.Value == type);
        }

        public bool TryGetEntitiesOfType(System.Type type, out List<ValueProperty<Entity>> entities)
        {
            entities = Entities.FindAll(entity => entity.Value.Type.Value == type);
            return entities.Count > 0;
        }

        public bool TryGetEntityOfType(System.Type type, out ValueProperty<Entity> entity)
        {
            entity = Entities.Find(entity => entity.Value.Type.Value == type);
            return entity != null;
        }
    }
}
