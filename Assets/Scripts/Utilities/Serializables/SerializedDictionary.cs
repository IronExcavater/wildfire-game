using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public abstract class SerializedDictionary<TEntry, TKey, TValue> where TEntry : KeyValuePair<TKey, TValue>
    {
        [SerializeField] protected List<TEntry> entries = new();
        protected Dictionary<TKey, TValue> _dictionary;

        public IReadOnlyDictionary<TKey, TValue> Dictionary => _dictionary;

        protected virtual void OnEnable()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            foreach (var entry in entries) _dictionary.TryAdd(entry.Key, entry.Value);
        }

        public abstract TValue GetValue(TKey key);
    }

    [Serializable]
    public abstract class KeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}
