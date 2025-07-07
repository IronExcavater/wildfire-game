using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Serializables
{
    /// <summary>
    /// Dictionary needs to be initialized at runtime, use SerializeDictionary.Initialize() in OnEnable().
    /// </summary>
    [Serializable]
    public abstract class SerializedDictionary<TEntry, TKey, TValue> where TEntry : KeyValuePair<TKey, TValue>
    {
        [SerializeField] protected List<TEntry> entries = new();
        protected Dictionary<TKey, TValue> _dictionary;

        public IReadOnlyDictionary<TKey, TValue> Dictionary => _dictionary;

        public void Initialize()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            foreach (var entry in entries) _dictionary.TryAdd(entry.Key, entry.Value);
        }
    }

    [Serializable]
    public abstract class KeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}
