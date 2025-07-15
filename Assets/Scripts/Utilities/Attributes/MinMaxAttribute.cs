using System;
using UnityEngine;

namespace Utilities.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MinMaxAttribute : PropertyAttribute
    {
        public readonly float min;
        public readonly float max;
        public readonly bool useSlider;

        public MinMaxAttribute(float min, float max, bool useSlider = false)
        {
            this.min = min;
            this.max = max;
            this.useSlider = useSlider;
        }
    }
}
