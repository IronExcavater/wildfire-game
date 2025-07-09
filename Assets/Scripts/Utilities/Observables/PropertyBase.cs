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
            foreach (var listener in _listeners.ToArray()) // ← safe copy
                listener.Invoke(this, change);
        }

        public bool IsBound => _boundTo != null;

        public void Bind(PropertyBase<T, TValue, TChange> other)
        {
            Unbind();
            _boundTo = other;
            Value = _boundTo.Value;
            _boundTo.AddListener(BindChanged);
        }

        public void Unbind()
        {
            _boundTo?.RemoveListener(BindChanged);
            _boundTo = null;
        }

        public void BindBidirectional(PropertyBase<T, TValue, TChange> other)
        {
            UnbindBidirectional();
            Bind(other);
            other.Bind(this);
        }

        public void UnbindBidirectional()
        {
            if (IsBound)
            {
                _boundTo.RemoveListener(BindChanged);
                _boundTo._boundTo?.RemoveListener(_boundTo.BindChanged);
                _boundTo._boundTo = null;
            }
            _boundTo = null;
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
