using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class MainMenuController : UIMenu
    {
        public override ParentDisplay ParentDisplay { get; protected set; } = ParentDisplay.Hide;

        public MainMenuController() : base("UI/Views/MainMenu")
        {
            Root.Q<Button>("settings-button").clicked += OpenSettings;
        }

        private void OpenSettings() => Open(new SettingsMenuController());
    }
}
