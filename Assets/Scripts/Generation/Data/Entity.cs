using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Generation.Data
{
    public class Entity
    {
        public readonly ValueProperty<Vector3> Position = new();
        public readonly ValueProperty<System.Type> Type = new();
        public readonly Dictionary<string, IProperty> Properties = new();

        public Entity(System.Type type, Vector3 position = default)
        {
            Type.Value = type;
            Position.Value = position;
        }

        public Entity(System.Type type, Vector3 position = default, params (string key, IProperty value)[] properties)
            : this(type, position)
        {
            foreach (var (key, value) in properties)
                Properties[key] = value;
        }

        public void AddProperty<T>(string key, ValueProperty<T> property)
        {
            Properties.Add(key, property);
        }

        public bool TryGetProperty<T>(string key, out ValueProperty<T> property)
        {
            property = GetProperty<T>(key);
            return property != null;
        }

        public ValueProperty<T> GetProperty<T>(string key)
        {
            if (Properties.TryGetValue(key, out var value) && value is ValueProperty<T> casted)
                return casted;
            return null;
        }
    }
}
