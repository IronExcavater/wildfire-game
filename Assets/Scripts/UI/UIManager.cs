using System.Collections.Generic;
using UI.Controllers;
using UnityEngine.UIElements;
using Utilities;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        private VisualElement _root;
        private Stack<UIMenu> _menuStack = new();

        protected override void Awake()
        {
            base.Awake();
            _root = GetComponent<UIDocument>().rootVisualElement;
            PushMenu(new MainMenuController());
        }

        public static void PushMenu(UIMenu menu)
        {
            Instance._root.Add(menu.Root);
            Instance._menuStack.Push(menu);
        }

        public static void PopMenu()
        {
            var top = Instance._menuStack.Pop();
            if (top == null) return;
            Instance._root.Remove(top.Root);
        }
    }
}
