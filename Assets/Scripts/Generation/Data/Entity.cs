using System;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Data
{
    public class Entity : IDisposable, IObservable<Entity, ValueChange<Entity>>
    {
        public readonly Property<Chunk> Chunk = new();
        public readonly Property<Vector3> Position = new();
        public readonly Property<Quaternion> Rotation = new();
        public readonly Property<Vector3> Scale = new();
        public readonly Property<Type> Type = new();
        public readonly ObservableDictionary<string, IProperty> Properties = new();

        public event Action<ValueChange<Entity>> OnChanged;

        public Entity(Type type, Chunk chunk, Vector3 position = default)
        {
            InitializeListeners();
            Type.Value = type;
            Chunk.Value = chunk;
            Position.Value = position;
        }

        public Entity(Type type, Chunk chunk, Vector3 position = default, params (string key, IProperty value)[] properties)
            : this(type, chunk, position)
        {
            InitializeListeners();
            foreach (var (key, value) in properties)
                Properties[key] = value;
        }

        private void InitializeListeners()
        {
            Chunk.AddListener((_, _) => InvokeOnChanged());
            Position.AddListener((_, _) => InvokeOnChanged());
            Type.AddListener((_, _) => InvokeOnChanged());
            Properties.AddListener((_, _) => InvokeOnChanged());
        }

        private void InvokeOnChanged()
        {
            OnChanged?.Invoke(new ValueChange<Entity>(this, this));
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

        public override string ToString()
        {
            return $"{Type.Value.Name} at Chunk {Chunk.Value.Position}";
        }

        public void Dispose()
        {
            Chunk.Value = null;
            Type.Value = null;
            Properties.Clear();
        }
    }
}
