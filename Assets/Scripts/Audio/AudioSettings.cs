using System;
using Utilities.Observables;

namespace Audio
{
    [Serializable]
    public class AudioSettings : IObservable<AudioSettings, ValueChange<AudioSettings>>
    {
        public Property<float> MasterVolume = new(100);
        public Property<float> MusicVolume = new(100);
        public Property<float> SfxVolume = new(100);

        public event Action<ValueChange<AudioSettings>> OnChanged;
        public void InvokeOnChanged() => OnChanged?.Invoke(new ValueChange<AudioSettings>(this, this));

        public AudioSettings()
        {
            MasterVolume.AddListener((_, _) => InvokeOnChanged());
            MusicVolume.AddListener((_, _) => InvokeOnChanged());
            SfxVolume.AddListener((_, _) => InvokeOnChanged());
        }
    }
}
