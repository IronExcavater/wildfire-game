using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public abstract class UIMenu
    {
        protected VisualTreeAsset _root;
        public VisualElement Root;

        protected UIMenu(string uxmlPath)
        {
            _root = Resources.Load<VisualTreeAsset>(uxmlPath);
            Root = _root.CloneTree();
        }
    }
}
