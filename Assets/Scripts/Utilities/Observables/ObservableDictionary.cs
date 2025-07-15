using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Utilities.Observables
{
    public class ObservableDictionary<TKey, TValue> : PropertyBase<TValue, Dictionary<TKey, TValue>, DictionaryChange<TKey, TValue>>,
        IDictionary<TKey, TValue>
    {
        public ObservableDictionary(bool observeInnerValue = true)
            : base(new(), observeInnerValue) { }

        protected void ItemSubscribe(TValue item)
        {
            if (item is IObservable<TValue, ValueChange<TValue>> observable) observable.OnChanged += ItemChanged;
        }

        private void ItemUnsubscribe(TValue item)
        {
            if (item is IObservable<TValue, ValueChange<TValue>> observable) observable.OnChanged -= ItemChanged;
        }

        private void ItemChanged(ValueChange<TValue> change)
        {
            if (!ObserveInnerValue) return;

            foreach (var kvp in Value)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, change.OldValue))
                {
                    NotifyListeners(new DictionaryChange<TKey, TValue>(
                        Value, DictionaryChangeType.Update, kvp.Key, change.OldValue, change.NewValue
                    ));
                    break;
                }
            }
        }

        public ICollection<TKey> Keys => Value.Keys;
        public ICollection<TValue> Values => Value.Values;
        public int Count => Value.Count;
        public bool IsReadOnly => false;

        public IReadOnlyDictionary<TKey, TValue> ReadOnly => Value;

        public TValue this[TKey key]
        {
            get => Value[key];
            set
            {
                Value.TryGetValue(key, out var oldValue);
                ItemUnsubscribe(oldValue);
                var replaced = oldValue != null;

                Value[key] = value;
                ItemSubscribe(value);

                NotifyListeners(new DictionaryChange<TKey, TValue>(
                    Value, replaced ? DictionaryChangeType.Replace : DictionaryChangeType.Add,
                    key, oldValue, value
                ));
            }
        }

        public bool ContainsKey(TKey key) => Value.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => Value.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => Value.GetEnumerator();
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Value.GetEnumerator();

        public void Add(TKey key, TValue value)
        {
            Value.Add(key, value);
            ItemSubscribe(value);

            NotifyListeners(new DictionaryChange<TKey, TValue>(
                Value, DictionaryChangeType.Add, key, default, value
            ));
        }

        public void Clear()
        {
            foreach (var value in Value.Values)
                ItemUnsubscribe(value);

            NotifyListeners(new DictionaryChange<TKey, TValue>(Value, DictionaryChangeType.Clear));

            Value.Clear();
        }

        public bool Remove(TKey key)
        {

            if (!Value.TryGetValue(key, out var value)) return false;
            var r = Value.Remove(key);
            if (r)
            {
                ItemUnsubscribe(value);

                NotifyListeners(new DictionaryChange<TKey, TValue>(
                    Value, DictionaryChangeType.Remove, key, value
                ));
            }
            return r;
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public bool Contains(KeyValuePair<TKey, TValue> item) =>
            Value.TryGetValue(item.Key, out var v) &&
            EqualityComparer<TValue>.Default.Equals(v, item.Value);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            ((IDictionary<TKey, TValue>)Value).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public override void SetValue(Dictionary<TKey, TValue> newValue)
        {
            if (EqualityComparer<Dictionary<TKey, TValue>>.Default.Equals(Value, newValue)) return;

            foreach (var value in Value.Values)
                ItemUnsubscribe(value);

            _value = newValue;

            foreach (var value in Value.Values)
                ItemSubscribe(value);

            NotifyListeners(new DictionaryChange<TKey, TValue>(Value, DictionaryChangeType.Set));
        }

        protected override void BindChanged(PropertyBase<TValue, Dictionary<TKey, TValue>, DictionaryChange<TKey, TValue>> other,
            DictionaryChange<TKey, TValue> change)
        {
            if (StopBindPropagation) return;
            _boundTo.StopBindPropagation = true;

            switch (change.Type)
            {
                case DictionaryChangeType.Add:
                    Value[change.Key] = change.NewValue;
                    ItemSubscribe(change.NewValue);
                    break;
                case DictionaryChangeType.Remove:
                    Value.Remove(change.Key);
                    ItemUnsubscribe(change.OldValue);
                    break;
                case DictionaryChangeType.Replace:
                    Value[change.Key] = change.NewValue;
                    ItemUnsubscribe(change.OldValue);
                    ItemSubscribe(change.NewValue);
                    break;
                case DictionaryChangeType.Update:
                    NotifyListeners(change);
                    break;
                case DictionaryChangeType.Clear:
                    foreach (var v in Value.Values)
                        ItemUnsubscribe(v);
                    Value.Clear();
                    break;
                case DictionaryChangeType.Set:
                    foreach (var v in Value.Values)
                        ItemUnsubscribe(v);
                    Value.Clear();
                    foreach (var kvp in other.Value)
                    {
                        Value[kvp.Key] = kvp.Value;
                        ItemSubscribe(kvp.Value);
                    }
                    break;
            }

            NotifyListeners(change);
            _boundTo.StopBindPropagation = false;
        }
    }
}
