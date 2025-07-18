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
        [SerializeField, SerializeReference, PolymorphicField] private UIMenu _rootMenu;

        protected override void Awake()
        {
            base.Awake();
            _root = GetComponent<UIDocument>().rootVisualElement;

            var stylesheet = Resources.Load<StyleSheet>("UI/Styles/main");
            _root.styleSheets.Add(stylesheet);

            Instance._root.Add(_rootMenu?.Init().Root);
        }
    }
}
