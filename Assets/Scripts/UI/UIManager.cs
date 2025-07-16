using UI.Controllers;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        private VisualElement _root;
        [SerializeField] private UIMenu _rootMenu;

        protected override void Awake()
        {
            base.Awake();
            _root = GetComponent<UIDocument>().rootVisualElement;
            var menu = new MainMenuController();
            Instance._root.Add(menu.Root);
        }
    }
}
