using UnityEngine;
using Utilities;
using Utilities.Observables;

namespace Generation.Data
{
    public abstract class DataObject<TData> : MonoBehaviour
    {
        public ValueProperty<TData> Data;

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
