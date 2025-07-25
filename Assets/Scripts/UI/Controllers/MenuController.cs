using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public enum ParentDisplay
    {
        Show,
        Hide
    }

    [Serializable]
    public class MenuController
    {
        [HideInInspector, SerializeField] private VisualTreeAsset _asset;
        private string _uxmlPath;
        public VisualElement Root { get; protected set; }
        public MenuController Parent { get; protected set; }
        public ParentDisplay ParentDisplay { get; protected set; }

        protected VisualElement _menuContainer;
        protected VisualElement _submenuContainer;

        protected MenuController(string uxmlPath, ParentDisplay parentDisplay = ParentDisplay.Hide)
        {
            _uxmlPath = uxmlPath;
            ParentDisplay = parentDisplay;
        }

        public virtual MenuController Init()
        {
            _asset = Resources.Load<VisualTreeAsset>(_uxmlPath);
            Root = _asset.CloneTree();

            _menuContainer = Root.Q<VisualElement>("menu-container");
            _submenuContainer = Root.Q<VisualElement>("submenu-container");
            return this;
        }

        public void Open(MenuController child)
        {
            _submenuContainer.Clear();

            child.Parent = this;

            if (child.ParentDisplay == ParentDisplay.Hide) Hide(() =>
            {
                _submenuContainer.Add(child.Root);
                child.Show();
            });
            else child.Show();
        }

        public void Close()
        {
            Hide(() =>
            {
                Parent._submenuContainer.Remove(Root);
                Parent.Show();
            });
        }

        public async void Show(Action onShown = null)
        {
            await _menuContainer.DOFade(0, 1)();
            onShown?.Invoke();
        }

        public async void Hide(Action onHidden = null)
        {
            await _menuContainer.DOFade(1, 0)();
            onHidden?.Invoke();
        }
    }
}
