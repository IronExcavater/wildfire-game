using UnityEngine;
using Utilities.Observables;

namespace Generation.Objects
{
    public abstract class DataObject<TData> : MonoBehaviour
    {
        public Property<TData> Data = new();

        protected virtual void Awake()
        {
            Data.AddListener(OnDataChanged);
        }

        /// <summary>
        /// Bind all data properties to instance properties
        /// </summary>
        protected abstract void OnDataChanged(PropertyBase<TData, TData, ValueChange<TData>> property, ValueChange<TData> change);
    }
}
