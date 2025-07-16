using System;
using System.Collections.Generic;
using System.Reflection;

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
            var type = typeof(TValue);
            if (initialValue == null && type.IsClass && type.GetConstructor(Type.EmptyTypes) != null)
                _value = (TValue)Activator.CreateInstance(type);
            else
                _value = initialValue;

            ObserveInnerValue = observeInnerValue;
        }

        public TValue Value
        {
            get => GetValue();
            set => ApplyFromValue(value);
        }

        public TValue GetValue()
        {
            return _value;
        }

        public abstract void SetValue(TValue newValue);

        public void ApplyFrom(PropertyBase<T, TValue, TChange> other)
        {
            var otherValue = other.GetType().GetProperty("Value")?.GetValue(other);
            ApplyFromValue((TValue)otherValue);
        }

        public void ApplyFromValue(TValue newValue)
        {
            var valueType = typeof(TValue);

            if (valueType.IsPrimitive || valueType == typeof(string) ||
                Value == null || newValue == null)
            {
                SetValue(newValue);
                return;
            }

            var setValueMethod = valueType.GetMethod("SetValue");
            if (setValueMethod != null)
            {
                setValueMethod.Invoke(Value, new object[] { newValue });
                return;
            }

            foreach (var field in valueType.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                      BindingFlags.NonPublic))
            {
                var currentField = field.GetValue(Value);
                var incomingField = field.GetValue(newValue);
                var fieldType = field.FieldType;
                if (incomingField == null) continue;

                if (typeof(IProperty).IsAssignableFrom(fieldType))
                {
                    var applyFromMethod = fieldType.GetMethod(nameof(ApplyFrom));
                    applyFromMethod?.Invoke(currentField, new[] { incomingField });
                }
                else if (fieldType.IsPrimitive || fieldType == typeof(string))
                {
                    field.SetValue(Value, incomingField);
                }
            }
        }

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
