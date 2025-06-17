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


            var wrapperBox = new BoxRect(position.position, position.width);

            var labelBox = new BoxRect(wrapperBox);
            EditorGUI.LabelField(labelBox.Rect.Value, label, EditorStyles.boldLabel);

            var entriesProp = property.FindPropertyRelative("entries");
            var entriesBox = new BoxRect(wrapperBox, 0);

            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var entryBox = new BoxRect(entriesBox, entryProp);

                EditorGUI.PropertyField(entryBox.Rect.Value, entryProp, GUIContent.none, true);

                /*var keyProp = entryProp.FindPropertyRelative("Key");
                var keyBox = new BoxRect(entryBox, size, keyProp);

                var valueProp = entryProp.FindPropertyRelative("Value");
                var valueBox = new BoxRect(entryBox, size, valueProp);

                EditorGUI.PropertyField(keyBox.Rect.Value, keyProp, GUIContent.none);
                EditorGUI.PropertyField(valueBox.Rect.Value, valueProp, GUIContent.none);*/
            }

            var buttonsBox = new BoxRect(wrapperBox)
            {
                Align = { Value = new BoxAlign(0.6f) },
                Display = { Value = BoxDisplay.Inline }
            };

            var addButtonBox = new BoxRect(buttonsBox, width: 30);
            var minusButtonBox = new BoxRect(buttonsBox, width: 30);

            if (GUI.Button(addButtonBox.Rect.Value, "+"))
            {
                Debug.Log("Pressed +");
                entriesProp.InsertArrayElementAtIndex(entriesProp.arraySize);
            }
            if (GUI.Button(minusButtonBox.Rect.Value, "-") && entriesProp.arraySize > 0)
                entriesProp.DeleteArrayElementAtIndex(entriesProp.arraySize - 1);

            EditorGUI.DrawRect(wrapperBox.Rect.Value, Color.red);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            if (PropertyValidation(property))
            {
                var entries = property.FindPropertyRelative("entries");
                height += entries.arraySize * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
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
