using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utilities.Observables
{
    public interface IProperty { }

    public abstract class PropertyBase<T, TValue, TChange> : IProperty
        where TChange : IChange<T>
    {
        [NonSerialized] protected TValue _value;

        [NonSerialized] protected readonly List<Action<PropertyBase<T, TValue, TChange>, TChange>> _listeners = new();
        [NonSerialized] protected PropertyBase<T, TValue, TChange> _boundTo;

        [NonSerialized] public bool StopBindPropagation;
        [NonSerialized] public bool ObserveInnerValue;

        protected PropertyBase(TValue initialValue = default, bool observeInnerValue = true)
        {
            var type = typeof(TValue);
            if (initialValue == null && type.IsClass && type.GetConstructor(Type.EmptyTypes) != null)
                _value = (TValue)Activator.CreateInstance(type);
            else
                _value = initialValue;

            ObserveInnerValue = observeInnerValue;
            ValueSubscribe();
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

        /// <summary>
        /// Sets the property's value with optional reference stability.
        /// <para/>
        /// When <paramref name="isStable"/> is <c>true</c>, merges values into the existing reference
        /// via <see cref="ApplyFrom"/>, preserving listeners and minimising allocations. 100% reference stability is
        /// impossible for this system, so use this only for stable, known object trees (e.g. settings, UI state).
        /// <para/>
        /// For dynamic or procedurally generated date (e.g. world chunks, entities, prefer the default behaviour
        /// (<c>isStable = false</c>)to fully replace the value and avoid stale states.
        /// </summary>
        public abstract void SetValue(TValue newValue, bool isStable = false);

        protected void ApplyFrom(T oldValue, T newValue, Action fallbackHandler)
        {
            if (oldValue is IProperty oldProp && newValue is IProperty newProp)
            {
                var setValue = typeof(T).GetMethod("SetValue");
                setValue?.Invoke(oldProp,
                    new[] { typeof(T).GetProperty("Value")?.GetValue(newValue), true });
            }
            else if (!typeof(T).IsPrimitive && !typeof(T).IsValueType && typeof(T) != typeof(string) &&
                     oldValue != null && newValue != null)
            {
                var fields = typeof(T)
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(NonSerializedAttribute))) continue;
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;

                    var oldField = field.GetValue(oldValue);
                    var newField = field.GetValue(newValue);

                    if (oldField is IProperty oldFieldProp && newField is IProperty newFieldProp)
                    {
                        var setValue = oldField.GetType().GetMethod("SetValue");
                        setValue?.Invoke(oldFieldProp,
                            new[] { newField.GetType().GetProperty("Value")?.GetValue(newField), true });
                        continue;
                    }

                    field.SetValue(oldValue, newField);
                }
            }
            else fallbackHandler.Invoke();
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
