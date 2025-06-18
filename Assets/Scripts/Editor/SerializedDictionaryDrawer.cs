using Editor.CssRect;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Attributes;

namespace Editor
{
    [CustomPropertyDrawer(typeof(SerializedDictionaryFieldAttribute))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!PropertyValidation(property))
            {
                EditorGUI.LabelField(position, $"⚠ {fieldInfo.FieldType} must inherit SerializedDictionary<,,> to use [SerializedDictionaryField]!");
                return;
            }

            var wrapperBox = new BoxRect(position.position, new(position.width, 0))
            {
                Gap = { Value = new BoxGap(0, 10) }
            };

            var labelBox = new BoxRect(wrapperBox);
            EditorGUI.LabelField(labelBox.Rect.Value, label, EditorStyles.boldLabel);

            var entriesProp = property.FindPropertyRelative("entries");
            var entriesBox = new BoxRect(wrapperBox, 0);

            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var entryBox = new BoxRect(entriesBox);

                var keyProp = entryProp.FindPropertyRelative("Key");
                var keyBox = new BoxRect(entryBox, keyProp);

                var valueProp = entryProp.FindPropertyRelative("Value");
                var valueBox = new BoxRect(entryBox, valueProp);

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
            var height = EditorGUIUtility.singleLineHeight * 2;

            if (PropertyValidation(property))
            {
                var entriesProp = property.FindPropertyRelative("entries");

                for (var i = 0; i < entriesProp.arraySize; i++)
                {
                    var entryProp = entriesProp.GetArrayElementAtIndex(i);
                    var keyProp = entryProp.FindPropertyRelative("Key");
                    var valueProp = entryProp.FindPropertyRelative("Value");

                    height += EditorGUI.GetPropertyHeight(keyProp, true);
                    height += EditorGUI.GetPropertyHeight(valueProp, true);
                }
            }

            return height;
        }

        public bool PropertyValidation(SerializedProperty property)
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
