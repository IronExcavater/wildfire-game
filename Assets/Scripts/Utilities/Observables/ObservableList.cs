using System;
using System.Collections;
using System.Collections.Generic;

namespace Utilities.Observables
{
    public class ObservableList<T> : PropertyBase<T, List<T>, ListChange<T>>, IList<T>
    {
        public ObservableList(bool observeInnerValue = true)
            : base(new(), observeInnerValue) { }

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

            var index = Value.IndexOf(change.OldValue);
            NotifyListeners(new ListChange<T>(
                Value, ListChangeType.Update,
                index, index
            ));
        }

        public int Count => Value.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => Value[index];
            set
            {
                ItemUnsubscribe(Value[index]);
                var replaced = Value[index] != null;

                Value[index] = value;
                ItemSubscribe(value);

                NotifyListeners(new ListChange<T>(
                    Value, replaced ? ListChangeType.Replace : ListChangeType.Add,
                    index, index,
                    Value.GetRange(index, replaced ? 1 : 0)
                ));
            }
        }

        public bool Contains(T item) => Value.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Value.CopyTo(array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => Value.GetEnumerator();
        public IEnumerator<T> GetEnumerator() => Value.GetEnumerator();
        public int IndexOf(T item) => Value.IndexOf(item);

        public void Add(T item)
        {
            Value.Add(item);
            ItemSubscribe(item);

            var index = Value.Count - 1;
            NotifyListeners(new ListChange<T>(
                Value, ListChangeType.Add,
                index, index + 1
            ));
        }

        public void AddRange(IEnumerable<T> items)
        {
            var added = new List<T>(items);
            var from = Value.Count;
            Value.AddRange(added);
            added.ForEach(ItemSubscribe);

            NotifyListeners(new ListChange<T>(
                Value, ListChangeType.Add,
                from, Value.Count
            ));
        }

        public void Clear()
        {
            Value.ForEach(ItemUnsubscribe);

            NotifyListeners(new ListChange<T>(
                Value, ListChangeType.Remove,
                0, Value.Count - 1,
                Value.GetRange(0, Value.Count)
            ));

            Value.Clear();
        }

        public void Insert(int index, T item)
        {
            Value.Insert(index, item);
            ItemSubscribe(item);

            NotifyListeners(new ListChange<T>(
                Value, ListChangeType.Add,
                index, index + 1
            ));
        }

        public bool Remove(T item)
        {
            var index = Value.IndexOf(item);
            var r = Value.Remove(item);
            if (r)
            {
                ItemUnsubscribe(item);

                NotifyListeners(new ListChange<T>(
                    Value, ListChangeType.Remove,
                    index, index + 1,
                    new List<T>{item}
                ));
            }
            return r;
        }

        public void RemoveAt(int index)
        {
            var item = Value[index];
            ItemUnsubscribe(item);
            Value.RemoveAt(index);

            NotifyListeners(new ListChange<T>(
                Value, ListChangeType.Remove,
                index, index + 1,
                new List<T>{item}
            ));
        }

        public override void SetValue(List<T> newValue)
        {
            if (EqualityComparer<List<T>>.Default.Equals(Value, newValue)) return;

            var oldValue = Value;

            oldValue?.ForEach(ItemUnsubscribe);
            _value = newValue;
            Value.ForEach(ItemSubscribe);

            var oldCount = oldValue?.Count ?? 0;
            var replaceTo = Math.Min(oldCount, newValue.Count);
            if (replaceTo > 0)
                NotifyListeners(new ListChange<T>(
                    Value, ListChangeType.Replace,
                    0, replaceTo,
                    Value.GetRange(0, replaceTo + 1)
                ));

            var maxTo = Math.Max(oldCount, newValue.Count);
            var isRemove = replaceTo < oldCount;
            NotifyListeners(new ListChange<T>(
                Value, isRemove ? ListChangeType.Remove : ListChangeType.Add,
                replaceTo, maxTo,
                Value.GetRange(replaceTo, isRemove ? maxTo - replaceTo + 1 : 0)
            ));
        }

        protected override void BindChanged(PropertyBase<T, List<T>, ListChange<T>> other, ListChange<T> change)
        {
            if (StopBindPropagation) return;
            _boundTo.StopBindPropagation = true;

            var otherList = change.GetList;

            if (change.WasPermutated)
            {
                Value.ForEach(ItemUnsubscribe);

                _value.Clear();
                _value.AddRange(otherList);

                Value.ForEach(ItemSubscribe);
            }

            change.GetRemoved.ForEach(item =>
            {
                ItemUnsubscribe(item);
                _value.Remove(item);
            });

            if (change.WasAdded)
                for (var i = change.From; i < change.To; i++)
                {
                    var item = otherList[i];
                    _value.Insert(i, item);
                    ItemSubscribe(item);
                }

            if (change.WasUpdated)
                for (var i = change.From; i < change.To; i++)
                    if (!EqualityComparer<T>.Default.Equals(_value[i], otherList[i]))
                        _value[i] = otherList[i];

            NotifyListeners(change);
            _boundTo.StopBindPropagation = false;
        }
    }
}
