using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Data
{
    public class Entity
    {
        public readonly Property<Vector3> Position = new();
        public readonly Property<Type> Type = new();
        public readonly Dictionary<string, IProperty> Properties = new();

        public Entity(Type type, Vector3 position = default)
        {
            Type.Value = type;
            Position.Value = position;
        }

        public Entity(Type type, Vector3 position = default, params (string key, IProperty value)[] properties)
            : this(type, position)
        {
            foreach (var (key, value) in properties)
                Properties[key] = value;
        }

        public void SetProperty<T>(string key, Property<T> property)
        {
            Properties[key] = property;
        }

        public bool TryGetProperty<T>(string key, out Property<T> property)
        {
            property = GetProperty<T>(key);
            return property != null;
        }

        public Property<T> GetProperty<T>(string key)
        {
            if (Properties.TryGetValue(key, out var value) && value is Property<T> casted)
                return casted;
            return null;
        }
    }
}
