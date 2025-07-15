using Generation.Data;
using UnityEngine;
using Utilities.Observables;

namespace Generation.Objects
{
    public abstract class DataObject<TData> : MonoBehaviour where TData : Entity
    {
        public readonly Property<TData> Data = new();

        protected readonly Property<Chunk> _chunk = new();
        protected readonly Property<Vector3> _position = new();
        protected readonly Property<Quaternion> _rotation = new();
        protected readonly Property<Vector3> _scale = new();

        protected virtual void Awake()
        {
            Data.AddListener(OnDataChanged);
            _position.AddListener((_, change) => transform.position = change.NewValue);
            _rotation.AddListener((_, change) => transform.localRotation = change.NewValue);
            _scale.AddListener((_, change) => transform.localScale = change.NewValue);
        }

        /// <summary>
        /// Bind all data properties to instance properties
        /// </summary>
        protected virtual void OnDataChanged(PropertyBase<TData, TData, ValueChange<TData>> property,
            ValueChange<TData> change)
        {
            _chunk.BindBidirectional(change.NewValue.Chunk);
            _position.BindBidirectional(change.NewValue.Position);
            _rotation.BindBidirectional(change.NewValue.Rotation);
            _scale.BindBidirectional(change.NewValue.Scale);
        }
    }
}
