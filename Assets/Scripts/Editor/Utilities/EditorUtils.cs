using UnityEditor;

namespace Editor.Utilities
{
    public static class EditorUtils
    {
        public static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        public const float SmallGap = 5;
        public const float LargeGap = 10;
        public const float Indent = 15;

        public static int PropertyHash(SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}:{property.propertyPath}"
                .GetHashCode();
        }

        public static string GetSearchableString(SerializedProperty property)
        {
            var result = "";
            if (property == null) return result;

            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
            {
                for (var i = 0; i < property.arraySize; i++)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    result += GetSearchableString(element) + "\n";
                }
            }

            var value = property.propertyType switch
            {
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.Integer => property.intValue.ToString(),
                SerializedPropertyType.Float => property.floatValue.ToString("G"),
                SerializedPropertyType.Boolean => property.boolValue.ToString(),
                SerializedPropertyType.Character => property.stringValue,
                SerializedPropertyType.Vector2 => property.vector2Value.ToString(),
                SerializedPropertyType.Vector2Int => property.vector2IntValue.ToString(),
                SerializedPropertyType.Vector3 => property.vector3Value.ToString(),
                SerializedPropertyType.Vector4 => property.vector4Value.ToString(),
                SerializedPropertyType.Rect => property.rectValue.ToString(),
                SerializedPropertyType.RectInt => property.rectIntValue.ToString(),
                SerializedPropertyType.Bounds => property.boundsValue.ToString(),
                SerializedPropertyType.BoundsInt => property.boundsIntValue.ToString(),
                SerializedPropertyType.Color => property.colorValue.ToString(),
                SerializedPropertyType.Quaternion => property.quaternionValue.ToString(),
                SerializedPropertyType.Enum => property.enumNames[property.enumValueIndex],
                SerializedPropertyType.ObjectReference => property.objectReferenceValue.ToString(),
                SerializedPropertyType.ExposedReference => property.exposedReferenceValue.ToString(),
                SerializedPropertyType.ManagedReference => property.managedReferenceFullTypename,
                _ => ""
            };
            if (!string.IsNullOrEmpty(value)) result += value + "\n";

            var iter = property.Copy();
            var end = property.GetEndProperty();
            while (iter.NextVisible(true) && !SerializedProperty.EqualContents(iter, end))
            {
                result += GetSearchableString(iter) + "\n";
            }

            return result;
        }
    }
}
