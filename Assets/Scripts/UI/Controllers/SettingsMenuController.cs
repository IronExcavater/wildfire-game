using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class SettingsMenuController : UIMenu
    {
        private SliderInt _masterVolumeSlider;
        private Toggle _invertToggle;

        public SettingsMenuController() : base("UI/Views/SettingsMenu")
        {
            Root.Q<Button>("back-button").clicked += UIManager.PopMenu;

            _masterVolumeSlider = Root.Q<SliderInt>("master-volume-slider");
            _invertToggle = Root.Q<Toggle>("invert-toggle");
        }
    }
}
