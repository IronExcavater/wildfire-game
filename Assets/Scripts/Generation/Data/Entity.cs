using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Generation.Data
{
    public class Entity
    {
        public Property<Vector3> Position;
        public Property<System.Type> Type;
        public Dictionary<string, Property<object>> Properties;

        public Entity(Vector3 position, System.Type type, Dictionary<string, Property<object>> properties)
        {
            Position = new Property<Vector3>(position);
            Type = new Property<System.Type>(type);
            Properties = properties;
        }
    }
}
