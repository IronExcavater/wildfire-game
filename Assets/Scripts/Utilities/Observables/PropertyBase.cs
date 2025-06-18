using System;
using System.Collections.Generic;

namespace Utilities.Observables
{
    public interface IProperty { }

    public abstract class PropertyBase<T, TValue, TChange> : IProperty
    where TChange : IChange<T>
    {
        protected TValue _value;

        protected readonly List<Action<PropertyBase<T, TValue, TChange>, TChange>> _listeners = new();
        protected PropertyBase<T, TValue, TChange> _boundTo;

        public bool StopBindPropagation;
        public bool ObserveInnerValue;

        protected PropertyBase(TValue initialValue = default, bool observeInnerValue = true)
        {
            _value = initialValue;
            ObserveInnerValue = observeInnerValue;
        }

        public TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public TValue GetValue()
        {
            return _value;
        }

        public abstract void SetValue(TValue newValue);

        public void AddListener(Action<PropertyBase<T, TValue, TChange>, TChange> listener)
        {
            if (_listeners.Contains(listener)) return;
            _listeners.Add(listener);
        }

        public void RemoveListener(Action<PropertyBase<T, TValue, TChange>, TChange> listener)
        {
            _listeners.Remove(listener);
        }

        protected void NotifyListeners(TChange change)
        {
            _listeners.ForEach(listener => listener.Invoke(this, change));
        }

        public bool IsBound => _boundTo != null;

        public void Bind(PropertyBase<T, TValue, TChange> other)
        {
            Unbind();
            _boundTo = other;
            Value = other.Value;
            other.AddListener(BindChanged);
        }

        public void Unbind()
        {
            _boundTo?.RemoveListener(BindChanged);
            _boundTo = null;
        }

        public void BindBidirectional(PropertyBase<T, TValue, TChange> other)
        {
            Bind(other);
            other.Bind(this);
        }

        public void UnbindBidirectional(PropertyBase<T, TValue, TChange> other)
        {
            Unbind();
            other.Unbind();
        }

        protected abstract void BindChanged(PropertyBase<T, TValue, TChange> other, TChange change);

        protected void ValueSubscribe()
        {
            if (Value is IObservable<T, TChange> observable) observable.OnChanged += ValueChanged;
        }

        protected void ValueUnsubscribe()
        {
            if (Value is IObservable<T, TChange> observable) observable.OnChanged -= ValueChanged;
        }

        protected void ValueChanged(TChange change)
        {
            if (!ObserveInnerValue) return;
            NotifyListeners(change);
        }

        public override string ToString()
        {
            var valueStr = Value?.ToString() ?? "null";
            var boundInfo = IsBound ? $", bound to {_boundTo.Value?.ToString() ?? "null"}" : "";
            var listenerInfo = _listeners.Count > 0 ? $", {_listeners.Count} listener{(_listeners.Count > 1 ? "s" : "")}" : "";

            return $"Property<{typeof(TValue).Name}> {{{valueStr}{boundInfo}{listenerInfo}}}";
        }
    }
}
