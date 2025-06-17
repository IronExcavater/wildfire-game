using System.Collections.Generic;
using Utilities.Observables;

namespace Utilities
{
    public class ValueProperty<T> : PropertyBase<T, T, ValueChange<T>>
    {
        public ValueProperty(T initialValue = default)
        {
            Value = initialValue;
        }

        public override void SetValue(T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(Value, newValue)) return;

            var oldValue = Value;

            ValueUnsubscribe();
            _value = newValue;
            ValueSubscribe();

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
    }
}
