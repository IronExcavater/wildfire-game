using System;
using System.Collections.Generic;
using System.Linq;
using Editor.CssRect;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Attributes;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PolymorphicFieldAttribute))]
    public class PolymorphicFieldDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, List<Type>> _typeCaches = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!PropertyValidation(property))
            {
                EditorGUI.HelpBox(position, "[PolymorphicField] requires [SerializeReference] attribute to work!", MessageType.Error);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var dropdownBox = new BoxRect(position.position, new(position.width, EditorGUIUtility.singleLineHeight))
            {
                Padding = { Value = new BoxInsets(left: 6) }
            };

            var baseType = fieldInfo.FieldType.GetBaseType();

            if (!_typeCaches.TryGetValue(baseType, out var subtypes))
            {
                subtypes = baseType.GetSubtypes(type => !type.IsAbstract && !type.IsInterface);
                _typeCaches[baseType] = subtypes;
            }

            var typeNames = subtypes.Select(t => t.Name).ToList();

            var currentType = property.managedReferenceValue?.GetType() ?? subtypes.First();
            var currentIndex = subtypes.FindIndex(t => t == currentType);

            var newIndex = EditorGUI.Popup(dropdownBox.Rect.Value, currentIndex, typeNames.ToArray());
            if (newIndex != currentIndex)
            {
                var newType = subtypes[newIndex];
                var instance = Activator.CreateInstance(newType);
                property.managedReferenceValue = instance;
            }

            if (property.managedReferenceValue != null)
            {
                var contentBox = new BoxRect(position.position, new(position.width, 0), property);

                EditorGUI.PropertyField(contentBox.Rect.Value, property, GUIContent.none, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            if (PropertyValidation(property) && property.managedReferenceValue != null)
            {
                height += EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.standardVerticalSpacing;
                height -= EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        public bool PropertyValidation(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ManagedReference;
        }
    }
}
