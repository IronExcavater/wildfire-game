using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class SettingsMenuController : UIMenu
    {
        public override ParentDisplay ParentDisplay { get; protected set; } = ParentDisplay.Hide;

        private SliderInt _masterVolumeSlider;
        private Toggle _invertToggle;

        public SettingsMenuController() : base("UI/Views/SettingsMenu")
        {
            Root.Q<Button>("back-button").clicked += Close;

            _masterVolumeSlider = Root.Q<SliderInt>("master-volume-slider");
            _invertToggle = Root.Q<Toggle>("invert-toggle");
        }
    }
}
