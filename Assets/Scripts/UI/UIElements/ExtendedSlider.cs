using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UIElements
{
    [UxmlElement]
    public partial class ExtendedSlider : Slider
    {
        private VisualElement _fill, _dragger;
        private Label _valueLabel, _lowLabel, _highLabel;

        public ExtendedSlider()
        {
            AddToClassList("extended-slider");
            RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (_fill == null) Init();
                else UpdatePosition();
            });
        }

        private void Init()
        {
            _fill = new VisualElement { name = "extended-filler" };
            _fill.AddToClassList("extended-slider__filler");

            _valueLabel = new Label { name = "extended-value-label" };
            _valueLabel.AddToClassList("extended-slider__value");

            _lowLabel = new Label { name = "extended-low-label" };
            _lowLabel.AddToClassList("extended-slider__low");

            _highLabel = new Label { name = "extended-high-label" };
            _highLabel.AddToClassList("extended-slider__high");

            var sliderInput = this.Q(className: "unity-slider__input");
            sliderInput.Insert(0, _lowLabel);
            sliderInput.Add(_highLabel);

            var dragContainer = this.Q("unity-drag-container");
            dragContainer.Insert(1, _fill);
            dragContainer.Add(_valueLabel);

            _dragger = this.Q("unity-dragger");

            this.RegisterValueChangedCallback(_ => UpdateValue());
            UpdateValue();

            schedule.Execute(() =>
            {
                var highLabelWidth = _highLabel.resolvedStyle.width;
                _dragger.style.width = highLabelWidth;
                schedule.Execute(UpdatePosition);
            });
        }

        private void UpdateValue()
        {
            var percent = Mathf.InverseLerp(lowValue, highValue, value);
            _fill.style.width = Length.Percent(percent * 100);

            _valueLabel.text = value.ToString("N0");
            _lowLabel.text = lowValue.ToString("N0");
            _highLabel.text = highValue.ToString("N0");

            schedule.Execute(UpdatePosition);
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            var xTranslate = _dragger.resolvedStyle.translate.x
                             + _dragger.resolvedStyle.width / 2
                             - _valueLabel.resolvedStyle.width / 2;
            _valueLabel.style.translate = new Translate(xTranslate, 0);
        }
    }
}
