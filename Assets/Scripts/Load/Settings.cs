using Audio;
using System;
using Player;
using Utilities.Observables;

namespace Load
{
    [Serializable]
    public class Settings : IObservable<Settings, ValueChange<Settings>>
    {
        public Property<AudioSettings> Audio = new();
        public Property<CameraSettings> Camera = new();

        public event Action<ValueChange<Settings>> OnChanged;
        public void InvokeOnChanged() => OnChanged?.Invoke(new ValueChange<Settings>(this, this));

        public Settings()
        {
            Audio.AddListener((_, _) => InvokeOnChanged());
            Camera.AddListener((_, _) => InvokeOnChanged());
        }
    }
}
