using System;
using System.Collections;
using System.Collections.Generic;

namespace Utilities.Observables
{
    public class ObservableList<T> : IList<T>, IObservable
    {
        private readonly List<T> _list = new();
        public event Action OnChanged;
        public bool ObserveItems;

        public ObservableList(bool observeItems = true)
        {
            ObserveItems = observeItems;
        }

        private void InnerSubscribe(T item)
        {
            if (item is IObservable observable) observable.OnChanged += InnerChanged;
        }

        private void InnerUnsubscribe(T item)
        {
            if (item is IObservable observable) observable.OnChanged -= InnerChanged;
        }

        private void InnerChanged()
        {
            if (ObserveItems) OnChanged?.Invoke();
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => _list[index];
            set
            {
                InnerUnsubscribe(_list[index]);
                _list[index] = value;
                InnerSubscribe(value);
                OnChanged?.Invoke();
            }
        }

        public bool Contains(T item) => _list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        public int IndexOf(T item) => _list.IndexOf(item);

        public void Add(T item)
        {
            _list.Add(item);
            InnerSubscribe(item);
            OnChanged?.Invoke();
        }

        public void Clear()
        {
            foreach (var item in _list) InnerUnsubscribe(item);
            _list.Clear();
            OnChanged?.Invoke();
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            InnerSubscribe(item);
            OnChanged?.Invoke();
        }

        public bool Remove(T item)
        {
            var r = _list.Remove(item);
            if (r)
            {
                InnerUnsubscribe(item);
                OnChanged?.Invoke();
            }
            return r;
        }

        public void RemoveAt(int index)
        {
            InnerUnsubscribe(_list[index]);
            _list.RemoveAt(index);
            OnChanged?.Invoke();
        }
    }
}
