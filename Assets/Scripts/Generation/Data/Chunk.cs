using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Generation.Data
{
    public sealed class Chunk
    {
        public Vector2Int Position;
        public List<Property<Entity>> Entities = new();

        public Chunk(Vector2Int position)
        {
            Position = position;
        }
    }
}
