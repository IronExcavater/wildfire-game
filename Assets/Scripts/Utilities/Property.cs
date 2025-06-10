using System;
using System.Collections.Generic;

namespace Utilities
{
    public interface IProperty { }

    public class Property<T> : IProperty
    {
        private T _value;
        private readonly List<Action<Property<T>, T, T>> _listeners = new();
        private Property<T> _boundTo;
        private bool _isUpdatingFromBinding;

        public Property(T initialValue = default)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public T GetValue()
        {
            return _value;
        }

        public void SetValue(T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(_value, newValue)) return;

            var oldValue = _value;
            _value = newValue;

            if (!_isUpdatingFromBinding && _boundTo != null)
            {
                _boundTo._isUpdatingFromBinding = true;
                _boundTo.Value = newValue;
                _boundTo._isUpdatingFromBinding = false;
            }

            NotifyListeners(oldValue, newValue);
        }

        public void AddListener(Action<Property<T>, T, T> listener)
        {
            if (_listeners.Contains(listener)) return;
            _listeners.Add(listener);
        }

        public void RemoveListener(Action<Property<T>, T, T> listener)
        {
            _listeners.Remove(listener);
        }

        private void NotifyListeners(T oldValue, T newValue)
        {
            _listeners.ForEach(listener => listener.Invoke(this, oldValue, newValue));
        }

        public bool IsBound => _boundTo != null;

        public void Bind(Property<T> other)
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

        public void BindBidirectional(Property<T> other)
        {
            Bind(other);
            other.Bind(this);
        }

        public void UnbindBidirectional(Property<T> other)
        {
            Unbind();
            other.Unbind();
        }

        private void BindChanged(Property<T> other, T oldValue, T newValue)
        {
            _isUpdatingFromBinding = true;
            Value = newValue;
            _isUpdatingFromBinding = false;
        }

        public override string ToString()
        {
            var valueStr = _value?.ToString() ?? "null";
            var boundInfo = IsBound ? $", bound to {_boundTo._value?.ToString() ?? "null"}" : "";
            var listenerInfo = _listeners.Count > 0 ? $", {_listeners.Count} listener{(_listeners.Count > 1 ? "s" : "")}" : "";

            return $"Property<{typeof(T).Name}> {{{valueStr}{boundInfo}{listenerInfo}}}";
        }
    }
}
