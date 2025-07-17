using System;
using System.Collections.Generic;

namespace Utilities.Observables
{
    public class Property<T> : PropertyBase<T, T, ValueChange<T>>, IComparable<Property<T>>
    {
        public Property(T initialValue = default, bool observeInnerValue = true)
            : base(initialValue, observeInnerValue) { }

        public Property() { }

        public override void SetValue(T newValue, bool isStable = false)
        {
            if (EqualityComparer<T>.Default.Equals(Value, newValue)) return;

            var oldValue = Value;

            if (isStable)
            {
                ApplyFrom(oldValue, newValue, () =>
                {
                    ValueUnsubscribe();
                    _value = newValue;
                    ValueSubscribe();
                });
            }
            else
            {
                ValueUnsubscribe();
                _value = newValue;
                ValueSubscribe();
            }

            NotifyListeners(new ValueChange<T>(oldValue, newValue));
        }

        protected override void BindChanged(PropertyBase<T, T, ValueChange<T>> other, ValueChange<T> change)
        {
            if (StopBindPropagation) return;

            _boundTo.StopBindPropagation = true;
            Value = change.NewValue;

            NotifyListeners(change);
            _boundTo.StopBindPropagation = false;
        }

        public int CompareTo(Property<T> other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;

            if (Value is IComparable<T> comparable && other.Value != null)
                return comparable.CompareTo(other.Value);

            // fallback hash comparison if not comparable
            var thisHash = Value?.GetHashCode() ?? 0;
            var otherHash = other.Value?.GetHashCode() ?? 0;
            return thisHash.CompareTo(otherHash);
        }
    }
}
