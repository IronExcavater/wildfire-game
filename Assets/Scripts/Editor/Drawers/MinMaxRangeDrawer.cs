using System.Reflection;
using Editor.Utilities;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Attributes;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minProp = property.FindPropertyRelative("min");
            var maxProp = property.FindPropertyRelative("max");

            var rangeAttr = fieldInfo?.GetCustomAttribute<MinMaxAttribute>();
            var rangeMin = rangeAttr?.min ?? float.NegativeInfinity;
            var rangeMax = rangeAttr?.max ?? float.PositiveInfinity;
            var useSlider = rangeAttr?.useSlider ?? false;

            var labelWidth = EditorGUIUtility.labelWidth;

            EditorGUI.BeginProperty(position, label, property);
            var content = EditorGUI.PrefixLabel(position, label);
            content.x -= 13;
            var half = content.width / 2f;

            EditorGUIUtility.labelWidth = 45;

            var min = minProp.floatValue;
            var max = maxProp.floatValue;

            if (useSlider)
            {
                var fieldWidth = Mathf.Clamp(half / 2, 30, 65);

                var minFieldRect = new Rect(content.x, content.y, fieldWidth, content.height);
                var maxFieldRect = new Rect(position.xMax - fieldWidth, content.y, fieldWidth, content.height);
                var sliderRect = new Rect(minFieldRect.xMax - EditorUtils.LargeGap, content.y,
                    content.width - fieldWidth * 2 + EditorUtils.LargeGap * 3, content.height);

                min = EditorGUI.FloatField(minFieldRect, min);
                max = EditorGUI.FloatField(maxFieldRect, max);
                EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, rangeMin, rangeMax);
            }
            else
            {
                var fieldWidth = half + EditorUtils.LargeGap;

                var minRect = new Rect(content.x, content.y, fieldWidth, position.height);
                var maxRect = new Rect(position.xMax - fieldWidth, content.y, fieldWidth, position.height);

                min = EditorGUI.FloatField(minRect, new GUIContent("Min"), min);
                max = EditorGUI.FloatField(maxRect, new GUIContent("Max"), max);
            }

            min = Mathf.Clamp(min, rangeMin, rangeMax);
            max = Mathf.Clamp(max, min, rangeMax);

            minProp.floatValue = min;
            maxProp.floatValue = max;

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.EndProperty();
        }
    }
}
