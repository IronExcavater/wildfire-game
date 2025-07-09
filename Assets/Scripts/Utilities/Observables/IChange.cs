using System;
using System.Collections.Generic;

namespace Utilities.Observables
{
    public interface IChange<out T> { }

    public readonly struct ValueChange<T> : IChange<T>
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public ValueChange(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public enum ListChangeType { Permutation, Add, Remove, Replace, Update }

    public readonly struct ListChange<T> : IChange<T>
    {
        private readonly IReadOnlyList<T> _list;
        private readonly List<T> _removedList;
        private readonly ListChangeType _type;
        public readonly int From; // Inclusive index
        public readonly int To; // Exclusive index

        public ListChange(List<T> list, ListChangeType type, int from, int to, List<T> removedList = null)
        {
            _list = new List<T>(list);
            _removedList = removedList ?? new List<T>();
            _type = type;
            From = from;
            To = to;
        }

        public bool WasPermutated => _type == ListChangeType.Permutation;
        public bool WasAdded => _type == ListChangeType.Add | _type == ListChangeType.Replace;
        public bool WasRemoved => _type == ListChangeType.Remove | _type == ListChangeType.Replace;
        public bool WasReplaced => WasAdded && WasRemoved;
        public bool WasUpdated => _type == ListChangeType.Update;

        public IReadOnlyList<T> GetList => _list;

        public T GetPermutation(int index) => _list[Math.Clamp(index, From, To)];

        public List<T> GetAdded => WasAdded ? _list.GetRange(From, To - From) : new List<T>();

        public List<T> GetRemoved => _removedList;
    }

    public enum DictionaryChangeType { Add, Remove, Replace, Update, Clear, Set }

    public readonly struct DictionaryChange<TKey, TValue> : IChange<TValue>
    {
        private readonly IReadOnlyDictionary<TKey, TValue> _dictionary;
        public readonly DictionaryChangeType Type;
        public readonly TKey Key;
        public readonly TValue OldValue;
        public readonly TValue NewValue;

        public DictionaryChange(Dictionary<TKey, TValue> dictionary, DictionaryChangeType type, TKey key = default,
            TValue oldValue = default, TValue newValue = default)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);
            Type = type;
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public IReadOnlyDictionary<TKey, TValue> GetDictionary => _dictionary;
    }
}
