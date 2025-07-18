using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class MainMenuController : UIMenu
    {
        public MainMenuController() : base("UI/Views/MainMenu") { }

        public override UIMenu Init()
        {
            base.Init();
            Root.Q<Button>("settings-button").clicked += OpenSettings;
            return this;
        }

        private void OpenSettings() => Open(new SettingsMenuController().Init());
    }
}
