using System;
using System.Linq;
using System.Threading.Tasks;
using UI.Transitions;
using UnityEngine.UIElements;
using Utilities;
using Utilities.Observables;

namespace UI
{
    public static class Utils
    {
        public static async Task Sequence(params Func<Task>[] steps)
        {
            foreach (var step in steps) await step();
        }

        public static Func<Task> DOFade(this VisualElement element, float from, float to, float duration = 200,
            float delay = 0, EasingMode mode = EasingMode.EaseInOut)
        {
            return new OpacityTween(from, to, duration, delay, mode).Tween(element);
        }

        public static void SetTransitionDuration(this IStyle style, params float[] values)
        {
            var durations = values
                .Select(f => new TimeValue(f, TimeUnit.Millisecond))
                .ToList();
            style.transitionDuration = new StyleList<TimeValue>(durations);
        }

        public static void SetTransitionTimingFunction(this IStyle style, params EasingMode[] easingFunctions)
        {
            var easings = easingFunctions
                .Select(e => new EasingFunction(e))
                .ToList();
            style.transitionTimingFunction = new StyleList<EasingFunction>(easings);
        }

        public static void SetTransitionProperty(this IStyle style, params string[] propertyNames)
        {
            var properties = propertyNames
                .Select(p => new StylePropertyName(p))
                .ToList();
            style.transitionProperty = new StyleList<StylePropertyName>(properties);
        }

        public static void Bind<T>(this INotifyValueChanged<T> field, Property<T> property)
        {
            field.RegisterValueChangedCallback(evt => property.Value = evt.newValue);
            property.AddListener((_, change) => field.SetValueWithoutNotify(change.NewValue));
            field.SetValueWithoutNotify(property.Value);
        }

        public static void SetRange<T>(this BaseSlider<T> slider, T min, T max) where T : IComparable<T>
        {
            slider.lowValue = min;
            slider.highValue = max;
        }
    }
}
