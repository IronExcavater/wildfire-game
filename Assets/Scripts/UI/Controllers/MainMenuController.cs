using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class MainMenuControllerController : MenuController
    {
        public MainMenuControllerController() : base("UI/Views/MainMenu") { }

        public override MenuController Init()
        {
            base.Init();
            Root.Q<Button>("settings-button").clicked += OpenSettings;
            return this;
        }

        private void OpenSettings() => Open(new SettingsMenuController().Init());
    }
}
