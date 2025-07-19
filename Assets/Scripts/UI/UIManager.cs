using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using Utilities.Attributes;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MenuManager : Singleton<MenuManager>
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        [SerializeField, SerializeReference, PolymorphicField] private MenuController rootMenu;

        protected override void Awake()
        {
            base.Awake();
            _uiDocument = GetComponent<UIDocument>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            BuildUI();
        }

        private void BuildUI()
        {
            _root = _uiDocument.rootVisualElement;

            var stylesheets = Resources.LoadAll<StyleSheet>("UI/Styles");
            foreach (var style in stylesheets)
                _root.styleSheets.Add(style);

            Instance._root.Add(rootMenu?.Init().Root);
        }
    }
}
