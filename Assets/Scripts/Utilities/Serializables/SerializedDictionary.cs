using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Serializables
{
    /// <summary>
    /// Dictionary needs to be initialized at runtime, use SerializeDictionary.Initialize() in OnEnable().
    /// </summary>
    [Serializable]
    public abstract class SerializedDictionary<TEntry, TKey, TValue> : IEnumerable
        where TEntry : KeyValuePair<TKey, TValue>
    {
        [SerializeField] protected List<TEntry> entries = new();
        private Dictionary<TKey, TValue> _dictionary;

        public void Initialize()
        {
            if (_dictionary != null) return;

            _dictionary = new Dictionary<TKey, TValue>();
            foreach (var entry in entries)
            {
                entry.Initialize();
                _dictionary.TryAdd(entry.Key, entry.Value);
            }
        }

        public Dictionary<TKey, TValue> Dictionary
        {
            get
            {
                Initialize();
                return _dictionary;
            }
        }

        public bool ContainsKey(TKey key)
        {
            Initialize();
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            Initialize();
            return _dictionary.ContainsValue(value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Initialize();
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                Initialize();
                return _dictionary[key];
            }
            set
            {
                Initialize();
                _dictionary[key] = value;
            }
    }

        public ICollection<TKey> Keys
        {
            get
            {
                Initialize();
                return _dictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                Initialize();
                return _dictionary.Values;
            }
        }

        public int Count
        {
            get
            {
                Initialize();
                return _dictionary.Count;
            }
        }

        public IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Initialize();
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Serializable]
    public abstract class KeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public virtual void Initialize() {}
    }
}
