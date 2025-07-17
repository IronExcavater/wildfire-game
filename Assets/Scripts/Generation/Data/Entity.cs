using System;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Data
{
    public class Entity : IDisposable, IObservable<Entity, ValueChange<Entity>>, IComparable<Entity>
    {
        public Guid Id { get; private set; }
        public readonly Property<Chunk> Chunk = new();
        public readonly Property<Vector3> Position = new();
        public readonly Property<Quaternion> Rotation = new();
        public readonly Property<Vector3> Scale = new();
        public readonly Property<Type> Type = new();
        public readonly ObservableDictionary<string, IProperty> Properties = new();

        public event Action<ValueChange<Entity>> OnChanged;
        public void InvokeOnChanged() => OnChanged?.Invoke(new ValueChange<Entity>(this, this));

        public Entity(Guid id, Type type, Chunk chunk)
        {
            InitializeListeners();
            Id = id;
            Type.Value = type;
            Chunk.Value = chunk;
        }
        public Entity(Type type, Chunk chunk)
            : this(Guid.NewGuid(), type, chunk) { }

        public Entity(Guid id, Type type, Chunk chunk, params (string key, IProperty value)[] properties)
            : this(id, type, chunk)
        {
            InitializeListeners();
            foreach (var (key, value) in properties)
                Properties[key] = value;
        }
        public Entity(Type type, Chunk chunk, params (string key, IProperty value)[] properties)
            : this(Guid.NewGuid(), type, chunk, properties) { }

        private void InitializeListeners()
        {
            Chunk.AddListener((_, _) => InvokeOnChanged());
            Position.AddListener((_, _) => InvokeOnChanged());
            Type.AddListener((_, _) => InvokeOnChanged());
            Properties.AddListener((_, _) => InvokeOnChanged());
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

        public int CompareTo(Entity other) => Id.CompareTo(other.Id);

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
