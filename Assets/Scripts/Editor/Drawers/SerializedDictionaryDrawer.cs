using System;
using System.Collections.Generic;
using Editor.CssRect;
using Editor.Utilities;
using UnityEditor;
using UnityEngine;
using Utilities.Attributes;
using Utilities.Serializables;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SerializedDictionaryFieldAttribute))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private readonly SerializedDictionaryValidator _validator = new();
        private readonly Dictionary<string, string> _searches = new();
        private readonly Dictionary<string, Dictionary<int, bool>> _foldouts = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsPropertyValid())
            {
                EditorGUI.HelpBox(position,
                    $"{fieldInfo.FieldType} must inherit SerializedDictionary<,,> to use [SerializedDictionaryField]!",
                    MessageType.Error);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            BuildLayout(position, property, label, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var wrapper = BuildLayout(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 0), property, label);
            return wrapper.Rect.Value.height;
        }

        private BoxRect BuildLayout(Rect position, SerializedProperty property, GUIContent label, bool drawLayout = false)
        {
            var path = property.propertyPath;

            _searches.TryAdd(path, string.Empty);
            _foldouts.TryAdd(path, new());

            var foldouts = _foldouts.GetValueOrDefault(path, new());
            if (drawLayout) _foldouts[path] = foldouts;

            var attr = attribute as SerializedDictionaryFieldAttribute;
            var keyLabel = attr?.KeyLabel ?? "Key";
            var valueLabel = attr?.ValueLabel ?? "Value";

            var wrapperBox = new BoxRect(position.position, new(position.width, 0));

            if (!IsPropertyValid())
            {
                var errorBox = new BoxRect(wrapperBox, EditorUtils.LineHeight * 2);
                if (drawLayout)
                    EditorGUI.HelpBox(errorBox.Rect.Value,
                        $"{fieldInfo.FieldType} must inherit SerializedDictionary<,,> to use [SerializedDictionaryField]!",
                        MessageType.Error);
                return wrapperBox;
            }

            var labelBox = new BoxRect(wrapperBox);
            if (drawLayout) EditorGUI.LabelField(labelBox.Rect.Value, label, EditorStyles.boldLabel);

            var searchBox = new BoxRect(wrapperBox);
            if (drawLayout)
            {
                GUI.SetNextControlName("AdvancedTextField");
                _searches[path] = EditorGUI.TextField(searchBox.Rect.Value, _searches[path]);
                if (string.IsNullOrEmpty(_searches[path]) && GUI.GetNameOfFocusedControl() != "AdvancedTextField")
                    EditorGUI.LabelField(searchBox.Rect.Value, "Search... ");
            }

            var entriesProp = property.FindPropertyRelative("entries");
            var entriesBox = new BoxRect(wrapperBox, 0);

            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var keyProp = entryProp.FindPropertyRelative("Key");
                var valueProp = entryProp.FindPropertyRelative("Value");

                var entryHash = entryProp.propertyPath.GetHashCode();

                _validator.ValidateEntry(property, keyProp, valueProp, out var message, out var messageType);
                var hasError = !string.IsNullOrEmpty(message);
                var searchResult = IsSearchResult(_searches[path], keyProp, valueProp);

                var entryBox = new BoxRect(entriesBox)
                {
                    Padding = { Value = new BoxInsets(top: EditorUtils.LargeGap) }
                };

                var errorBox = hasError && searchResult ? new BoxRect(entryBox, EditorUtils.LineHeight * 2) : null;

                foldouts.TryAdd(entryHash, true);
                var foldoutBox = searchResult ? new BoxRect(entryBox) : null;
                if (drawLayout && foldoutBox != null)
                    foldouts[entryHash] = EditorGUI.Foldout(foldoutBox.Rect.Value, foldouts[entryHash], GUIContent.none);

                var isExpanded = foldouts[entryHash];
                var keyBox = searchResult ? new BoxRect(foldoutBox, keyProp) : null;
                var valueBox = isExpanded && searchResult ? new BoxRect(foldoutBox, valueProp)
                {
                    Padding = { Value = new BoxInsets(left: EditorUtils.Indent) }
                } : null;

                if (drawLayout)
                {
                    if (errorBox != null) EditorGUI.HelpBox(errorBox.Rect.Value, message, messageType);
                    if (keyBox != null) EditorGUI.PropertyField(keyBox.Rect.Value, keyProp, new GUIContent(keyLabel), true);
                    if (valueBox != null) EditorGUI.PropertyField(valueBox.Rect.Value, valueProp, new GUIContent(valueLabel), true);
                }
            }

            var buttonsBox = new BoxRect(wrapperBox)
            {
                Align = { Value = new BoxAlign(100) },
                Display = { Value = BoxDisplay.Inline }
            };

            var addButtonBox = new BoxRect(buttonsBox, width: 30);
            var minusButtonBox = new BoxRect(buttonsBox, width: 30);
            var expandAllBox = new BoxRect(buttonsBox, width: 30);
            var collapseAllBox = new BoxRect(buttonsBox, width: 30);

            if (drawLayout)
            {
                if (GUI.Button(addButtonBox.Rect.Value, "+"))
                {
                    entriesProp.InsertArrayElementAtIndex(entriesProp.arraySize);
                    property.serializedObject.ApplyModifiedProperties();
                }

                if (GUI.Button(minusButtonBox.Rect.Value, "-") && entriesProp.arraySize > 0)
                {
                    entriesProp.DeleteArrayElementAtIndex(entriesProp.arraySize - 1);
                    property.serializedObject.ApplyModifiedProperties();
                }

                if (GUI.Button(expandAllBox.Rect.Value, "v") && entriesProp.arraySize > 0)
                {
                    foreach (var key in foldouts.Keys)
                        foldouts[key] = true;
                }

                if (GUI.Button(collapseAllBox.Rect.Value, "ʌ") && entriesProp.arraySize > 0)
                {
                    foreach (var key in foldouts.Keys)
                        foldouts[key] = false;
                }
            }

            return wrapperBox;
        }

        private bool IsPropertyValid()
        {
            var fieldType = fieldInfo.FieldType;
            while (fieldType != null)
            {
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(SerializedDictionary<,,>))
                    return true;
                fieldType = fieldType.BaseType;
            }
            return false;
        }

        private bool IsSearchResult(string search, SerializedProperty keyProperty, SerializedProperty valueProperty)
        {
            var keyString = EditorUtils.GetSearchableString(keyProperty);
            var valueString = EditorUtils.GetSearchableString(valueProperty);

            return string.IsNullOrEmpty(search) ||
                   keyString.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                   valueString.Contains(search, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
