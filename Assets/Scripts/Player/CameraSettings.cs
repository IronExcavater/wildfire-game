using System;
using Utilities;
using Utilities.Observables;

namespace Player
{
    [Serializable]
    public class CameraSettings : IObservable<CameraSettings, ValueChange<CameraSettings>>
    {
        public Property<bool> InvertPan = new();
        public Property<bool> InvertRotate = new();
        public Property<bool> InvertZoom = new();

        public Property<float> PanSpeed = new(1f);
        public Property<float> RotateSpeed = new(10f);
        public Property<float> ZoomSpeed = new(60f);

        [NonSerialized] public MinMax PanSpeedRange = new(0.1f, 5);
        [NonSerialized] public MinMax RotateSpeedRange = new(5, 20);
        [NonSerialized] public MinMax ZoomSpeedRange = new(20, 100);

        public event Action<ValueChange<CameraSettings>> OnChanged;
        public void InvokeOnChanged() => OnChanged?.Invoke(new ValueChange<CameraSettings>(this, this));

        public CameraSettings()
        {
            InvertPan.AddListener((_, _) => InvokeOnChanged());
            InvertRotate.AddListener((_, _) => InvokeOnChanged());
            InvertZoom.AddListener((_, _) => InvokeOnChanged());
            PanSpeed.AddListener((_, _) => InvokeOnChanged());
            RotateSpeed.AddListener((_, _) => InvokeOnChanged());
            ZoomSpeed.AddListener((_, _) => InvokeOnChanged());
        }
    }
}
