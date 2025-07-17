using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.Observables
{
    public class ObservableList<T> : PropertyBase<T, SortedList<T, T>, DictionaryChange<T, T>>, IEnumerable<T>
        where T : IComparable<T>
    {
        public ObservableList(bool observeInnerValue = true) : base(new(), observeInnerValue) { }

        public ObservableList() : base(new()) { }

        protected void ItemSubscribe(T item)
        {
            if (item is IObservable<T, ValueChange<T>> observable) observable.OnChanged += ItemChanged;
        }

        private void ItemUnsubscribe(T item)
        {
            if (item is IObservable<T, ValueChange<T>> observable) observable.OnChanged -= ItemChanged;
        }

        private void ItemChanged(ValueChange<T> change)
        {
            if (!ObserveInnerValue) return;

            NotifyListeners(new DictionaryChange<T, T>(DictionaryChangeType.Update, change.OldValue, change.OldValue, change.NewValue));
        }

        public int Count => Value.Count;
        public bool IsReadOnly => false;

        public T this[int index] => Value.Values[index];

        public bool Contains(T item) => Value.ContainsKey(item);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() => Value.Values.GetEnumerator();
        public int IndexOf(T item) => Value.IndexOfKey(item);

        public List<T> ToList() => Value.Values.ToList();
        public IReadOnlyList<T> ReadOnly => ToList().AsReadOnly();

        public void Add(T item)
        {
            Value[item] = item;
            ItemSubscribe(item);

            NotifyListeners(new DictionaryChange<T, T>(DictionaryChangeType.Add, item, default, item));
        }

        public void Clear()
        {
            foreach (var value in Value.Values)
                ItemUnsubscribe(value);

            Value.Clear();
            NotifyListeners(new DictionaryChange<T, T>(DictionaryChangeType.Clear));
        }

        public bool Remove(T item)
        {
            if (!Value.TryGetValue(item, out _)) return false;
            var r = Value.Remove(item);
            if (r)
            {
                ItemUnsubscribe(item);

                NotifyListeners(new DictionaryChange<T, T>(DictionaryChangeType.Remove, item, item));
            }
            return r;
        }

        public void RemoveAt(int index)
        {
            var item = Value.Values[index];
            if (!Value.ContainsValue(item)) return;

            ItemUnsubscribe(item);
            Value.RemoveAt(index);

            NotifyListeners(new DictionaryChange<T, T>(DictionaryChangeType.Remove, item, item));
        }

        public override void SetValue(SortedList<T, T> newValue, bool isStable = false)
        {
            if (EqualityComparer<SortedList<T, T>>.Default.Equals(Value, newValue)) return;

            if (isStable)
            {
                var oldKeys = new HashSet<T>(Value.Keys);

                foreach (var key in oldKeys)
                {
                    if (newValue.ContainsKey(key)) continue;
                    ItemUnsubscribe(Value[key]);
                    Value.Remove(key);
                }

                foreach (var kvp in newValue)
                {
                    if (Value.TryGetValue(kvp.Key, out var oldValue))
                    {
                        ApplyFrom(oldValue, kvp.Value, () =>
                        {
                            ItemUnsubscribe(oldValue);
                            Value[kvp.Key] = kvp.Value;
                            ItemSubscribe(kvp.Value);
                        });
                    }
                    else
                    {
                        Value.Add(kvp.Key, kvp.Value);
                        ItemSubscribe(kvp.Value);
                    }
                }
            }
            else
            {
                foreach (var value in Value.Values)
                    ItemUnsubscribe(value);

                _value = newValue;

                foreach (var value in Value.Values)
                    ItemSubscribe(value);
            }

            NotifyListeners(new DictionaryChange<T, T>(DictionaryChangeType.Set));
        }

        protected override void BindChanged(PropertyBase<T, SortedList<T, T>, DictionaryChange<T, T>> other,
            DictionaryChange<T, T> change)
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
