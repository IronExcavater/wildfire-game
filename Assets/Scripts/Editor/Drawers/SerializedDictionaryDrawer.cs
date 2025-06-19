using Editor.CssRect;
using Editor.Utilities;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Attributes;
using Utils = Editor.Utilities.Utils;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SerializedDictionaryFieldAttribute))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private SerializedDictionaryValidator _validator = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!PropertyValidation())
            {
                EditorGUI.LabelField(position, $"⚠ {fieldInfo.FieldType} must inherit SerializedDictionary<,,> to use [SerializedDictionaryField]!");
                return;
            }

            var wrapperBox = new BoxRect(position.position, new(position.width, 0));

            var labelBox = new BoxRect(wrapperBox);
            EditorGUI.LabelField(labelBox.Rect.Value, label, EditorStyles.boldLabel);

            var entriesProp = property.FindPropertyRelative("entries");
            var entriesBox = new BoxRect(wrapperBox, 0);

            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var keyProp = entryProp.FindPropertyRelative("Key");
                var valueProp = entryProp.FindPropertyRelative("Value");

                _validator.ValidateEntry(property, keyProp, valueProp, out var message, out var messageType);
                var hasError = !string.IsNullOrEmpty(message);

                var entryBox = new BoxRect(entriesBox);
                var errorBox = hasError ? new BoxRect(entryBox, Utils.LineHeight * 2) : null;
                var keyBox = new BoxRect(entryBox, keyProp);
                var valueBox = new BoxRect(entryBox, valueProp);

                if (hasError) EditorGUI.HelpBox(errorBox.Rect.Value, message, messageType);
                EditorGUI.PropertyField(keyBox.Rect.Value, keyProp, GUIContent.none, true);
                EditorGUI.PropertyField(valueBox.Rect.Value, valueProp, GUIContent.none, true);
            }

            var buttonsBox = new BoxRect(wrapperBox)
            {
                Align = { Value = new BoxAlign(100) },
                Display = { Value = BoxDisplay.Inline }
            };

            var addButtonBox = new BoxRect(buttonsBox, width: 30);
            var minusButtonBox = new BoxRect(buttonsBox, width: 30);

            if (GUI.Button(addButtonBox.Rect.Value, "+"))
                entriesProp.InsertArrayElementAtIndex(entriesProp.arraySize);
            if (GUI.Button(minusButtonBox.Rect.Value, "-") && entriesProp.arraySize > 0)
                entriesProp.DeleteArrayElementAtIndex(entriesProp.arraySize - 1);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = Utils.LineHeight * 2 + Utils.LargeGap;

            if (PropertyValidation())
            {
                var entriesProp = property.FindPropertyRelative("entries");

                for (var i = 0; i < entriesProp.arraySize; i++)
                {
                    var entryProp = entriesProp.GetArrayElementAtIndex(i);
                    var keyProp = entryProp.FindPropertyRelative("Key");
                    var valueProp = entryProp.FindPropertyRelative("Value");

                    _validator.ValidateEntry(property, keyProp, valueProp, out var message, out _);
                    var hasError = !string.IsNullOrEmpty(message);
                    if (hasError) height += Utils.LineHeight * 2 + Utils.LargeGap;

                    height += EditorGUI.GetPropertyHeight(keyProp, true);
                    height += EditorGUI.GetPropertyHeight(valueProp, true);
                    height += Utils.LargeGap;

                    if (i < entriesProp.arraySize - 1) height += Utils.LargeGap;
                }
            }

            return height;
        }

        public bool PropertyValidation()
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
    }
}
