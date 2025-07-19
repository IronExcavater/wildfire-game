using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UIElements
{
    [UxmlElement]
    public partial class ExtendedSlider : VisualElement
    {
        [UxmlAttribute(name = "show-fill")] public bool showFill { get; set; } = true;
        [UxmlAttribute(name = "show-value")] public bool showValueLabel { get; set; } = true;
        [UxmlAttribute(name = "show-min-max")] public bool showMinMaxLabels { get; set; } = true;

        private readonly Slider _slider;
        private readonly VisualElement _fill;
        private readonly Label _valueLabel, _lowLabel, _highLabel;

        public ExtendedSlider()
        {
            AddToClassList("extended-slider");

            _slider = new Slider(0, 100);
            _fill = new VisualElement { name = "extended-filler" };
            _fill.AddToClassList("extended-slider__filler");
            _valueLabel = new Label { name = "value-label" };
            _lowLabel = new Label { name = "low-label" };
            _highLabel = new Label { name = "high-label" };

            if (showMinMaxLabels)
            {
                var sliderInput = _slider.Q(className: "unity-slider__input");
                sliderInput.Insert(0, _lowLabel);
                sliderInput.Add(_highLabel);
            }

            if (showFill)
            {
                var dragContainer = _slider.Q("unity-drag-container");
                dragContainer.Insert(1, _fill);
            }

            if (showValueLabel)
            {
                var dragger = _slider.Q("unity-dragger");
                dragger.Add(_valueLabel);
            }

            Add(_slider);
            _slider.RegisterValueChangedCallback(_ => ValueChanged());
            ValueChanged();
        }

        private void ValueChanged()
        {
            var percent = Mathf.InverseLerp(_slider.lowValue, _slider.highValue, _slider.value);
            _fill.style.width = Length.Percent(percent * 100);
            _valueLabel.text = _slider.value.ToString("N0");
            _lowLabel.text = _slider.lowValue.ToString("N0");
            _highLabel.text = _slider.highValue.ToString("N0");
        }
    }
}
