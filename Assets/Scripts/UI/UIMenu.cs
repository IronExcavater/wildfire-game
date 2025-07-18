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
    public class UIMenu
    {
        [HideInInspector, SerializeField] private VisualTreeAsset _asset;
        private string _uxmlPath;
        public VisualElement Root { get; protected set; }
        public UIMenu Parent { get; protected set; }
        public ParentDisplay ParentDisplay { get; protected set; }

        protected VisualElement _menuContainer;
        protected VisualElement _submenuContainer;

        protected UIMenu(string uxmlPath, ParentDisplay parentDisplay = ParentDisplay.Hide)
        {
            _uxmlPath = uxmlPath;
            ParentDisplay = parentDisplay;
        }

        public virtual UIMenu Init()
        {
            _asset = Resources.Load<VisualTreeAsset>(_uxmlPath);
            Root = _asset.CloneTree();

            _menuContainer = Root.Q<VisualElement>("menu-container");
            _submenuContainer = Root.Q<VisualElement>("submenu-container");
            return this;
        }

        public void Open(UIMenu child)
        {
            _submenuContainer.Clear();
            _submenuContainer.Add(child.Root);
            child.Parent = this;
            if (child.ParentDisplay == ParentDisplay.Hide) Hide();
        }

        public void Close()
        {
            Root.RemoveFromHierarchy();
            Parent?.Show();
        }

        public void Show()
        {
            _menuContainer.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            _menuContainer.style.display = DisplayStyle.None;
        }
    }
}
