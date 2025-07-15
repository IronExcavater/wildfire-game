using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class MainMenuController : UIMenu
    {
        public MainMenuController()
        {
            _root = Resources.Load<VisualTreeAsset>("UI/Views/MainMenu");
            Root = _root.CloneTree();

        }

        private void OpenSettings() => UIManager.
    }
}
