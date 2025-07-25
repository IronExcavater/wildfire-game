using System;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace UI.Transitions
{
    public abstract class ITween
    {
        protected readonly object _from;
        protected readonly object _to;
        protected readonly float _duration;
        protected readonly float _delay;
        protected readonly EasingMode _mode;

        public ITween(object from, object to, float duration = 200, float delay = 0, EasingMode mode = EasingMode.EaseInOut)
        {
            _from = from;
            _to = to;
            _duration = duration;
            _delay = delay;
            _mode = mode;
        }

        public abstract Func<Task> Tween(VisualElement element);
    }
}
