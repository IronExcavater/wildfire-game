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

            InnerUnsubscribe();
            _value = newValue;
            InnerSubscribe();

            if (!IsUpdatingFromBinding && _boundTo != null)
            {
                _boundTo.IsUpdatingFromBinding = true;
                _boundTo.Value = newValue;
                _boundTo.IsUpdatingFromBinding = false;
            }

            NotifyListeners(new ValueChange<T>(oldValue, newValue));
        }

        protected override void BindChanged(PropertyBase<T, T, ValueChange<T>> other, ValueChange<T> change)
        {
            IsUpdatingFromBinding = true;
            Value = change.NewValue;
            IsUpdatingFromBinding = false;
        }
    }
}
