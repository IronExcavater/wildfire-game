using Load;
using UI.UIElements;
using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class SettingsMenuController : MenuController
    {
        private ExtendedSlider _masterVolumeSlider;
        private ExtendedSlider _musicVolumeSlider;
        private ExtendedSlider _sfxVolumeSlider;

        private Toggle _invertPanToggle;
        private Toggle _invertRotateToggle;
        private Toggle _invertZoomToggle;

        private ExtendedSlider _panSpeedSlider;
        private ExtendedSlider _rotateSpeedSlider;
        private ExtendedSlider _zoomSpeedSlider;

        public SettingsMenuController() : base("UI/Views/SettingsMenu") { }

        public override MenuController Init()
        {
            base.Init();
            Root.Q<Button>("back-button").clicked += Close;

            _masterVolumeSlider = Root.Q<ExtendedSlider>("master-volume-slider");
            _musicVolumeSlider = Root.Q<ExtendedSlider>("music-volume-slider");
            _sfxVolumeSlider = Root.Q<ExtendedSlider>("sfx-volume-slider");

            _invertPanToggle = Root.Q<Toggle>("invert-pan-toggle");
            _invertRotateToggle = Root.Q<Toggle>("invert-rotate-toggle");
            _invertZoomToggle = Root.Q<Toggle>("invert-zoom-toggle");

            _panSpeedSlider = Root.Q<ExtendedSlider>("pan-speed-slider");
            _rotateSpeedSlider = Root.Q<ExtendedSlider>("rotate-speed-slider");
            _zoomSpeedSlider = Root.Q<ExtendedSlider>("zoom-speed-slider");

            var settings = SaveManager.Settings.Value;
            var audioSettings = settings.Audio.Value;
            var cameraSettings = settings.Camera.Value;

            _masterVolumeSlider.SetRange(0, 100);
            _masterVolumeSlider.Bind(audioSettings.MasterVolume);
            _musicVolumeSlider.SetRange(0, 100);
            _musicVolumeSlider.Bind(audioSettings.MusicVolume);
            _sfxVolumeSlider.SetRange(0, 100);
            _sfxVolumeSlider.Bind(audioSettings.SfxVolume);

            _invertPanToggle.Bind(cameraSettings.InvertPan);
            _invertRotateToggle.Bind(cameraSettings.InvertRotate);
            _invertZoomToggle.Bind(cameraSettings.InvertZoom);

            var panRange = cameraSettings.PanSpeedRange;
            _panSpeedSlider.SetRange(panRange.min, panRange.max);
            _panSpeedSlider.Bind(cameraSettings.PanSpeed);

            var rotateRange = cameraSettings.RotateSpeedRange;
            _rotateSpeedSlider.SetRange(rotateRange.min, rotateRange.max);
            _rotateSpeedSlider.Bind(cameraSettings.RotateSpeed);

            var zoomRange = cameraSettings.ZoomSpeedRange;
            _zoomSpeedSlider.SetRange(zoomRange.min, zoomRange.max);
            _zoomSpeedSlider.Bind(cameraSettings.ZoomSpeed);
            return this;
        }
    }
}
