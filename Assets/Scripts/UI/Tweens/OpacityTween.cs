using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace UI.Transitions
{
    public class OpacityTween : ITween
    {
        public OpacityTween(float from, float to, float duration = 1000, float delay = 0,
            EasingMode mode = EasingMode.EaseInOut)
            : base(from, to, duration, delay, mode) { }

        public override Func<Task> Tween(VisualElement element)
        {
            return async () =>
            {
                var from = (float)_from;
                var to = (float)_to;

                await Task.Delay((int)_delay);
                element.style.SetTransitionDuration(_duration);
                element.style.SetTransitionTimingFunction(_mode);

                element.style.display = DisplayStyle.Flex;
                element.style.opacity = from;

                await Task.Yield();
                element.style.SetTransitionProperty("opacity");
                await Task.Yield();

                element.style.opacity = to;

                await Task.Delay((int)_duration);
                if (to.Equals(0f)) element.style.display = DisplayStyle.None;
            };
        }
    }
}
