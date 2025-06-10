using UnityEngine;
using Utilities;

namespace Generation.Data
{
    public abstract class DataObject<TData> : MonoBehaviour
    {
        public Property<TData> Data;

        protected virtual void Awake()
        {
            Data.AddListener(OnDataChanged);
        }

        /// <summary>
        /// Bind all data properties to instance properties
        /// </summary>
        protected abstract void OnDataChanged(Property<TData> data, TData oldData, TData newData);
    }
}
