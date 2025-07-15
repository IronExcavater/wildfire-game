using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class MainMenuController : UIMenu
    {
        public MainMenuController() : base("UI/Views/MainMenu")
        {
            Root.Q<Button>("settings-button").clicked += OpenSettings;
        }

        private void OpenSettings() => UIManager.PushMenu(new SettingsMenuController());
    }
}
