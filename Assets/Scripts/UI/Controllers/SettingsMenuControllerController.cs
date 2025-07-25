using Load;
using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class SettingsMenuControllerController : MenuController
    {
        private SliderInt _masterVolumeSlider;
        private Toggle _invertPanToggle;

        public SettingsMenuControllerController() : base("UI/Views/SettingsMenu") { }

        public override MenuController Init()
        {
            base.Init();
            Root.Q<Button>("back-button").clicked += Close;

            _masterVolumeSlider = Root.Q<SliderInt>("master-volume-slider");
            _invertPanToggle = Root.Q<Toggle>("invert-pan-toggle");

            var settings = SaveManager.Settings;
            var cameraSettings = settings.Value.Camera;

            _invertPanToggle.Bind(cameraSettings.Value.InvertPan);
            return this;
        }
    }
}
