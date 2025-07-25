using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using Utilities.Attributes;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MenuManager : Singleton<MenuManager>
    {
        private VisualElement _root;
        [SerializeField, SerializeReference, PolymorphicField] private MenuController rootMenu;

        protected override void Awake()
        {
            base.Awake();
            _root = GetComponent<UIDocument>().rootVisualElement;

            var stylesheets = Resources.LoadAll<StyleSheet>("UI/Styles");
            foreach (var style in stylesheets)
                _root.styleSheets.Add(style);

            Instance._root.Add(rootMenu?.Init().Root);
        }
    }
}
