using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public enum ParentDisplay
    {
        Show,
        Hide
    }

    public abstract class UIMenu
    {
        private VisualTreeAsset _asset;
        public VisualElement Root { get; protected set; }
        public UIMenu Parent { get; protected set; }
        public abstract ParentDisplay ParentDisplay { get; protected set; }

        protected VisualElement _submenuContainer;

        protected UIMenu(string uxmlPath)
        {
            _asset = Resources.Load<VisualTreeAsset>(uxmlPath);
            Root = _asset.CloneTree();

            _submenuContainer = Root.Q<VisualElement>("submenu-container");
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
            Root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            Root.style.display = DisplayStyle.None;
        }
    }
}
